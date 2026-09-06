using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VTubeStudio.Client.Errors;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client;

/// <summary>
/// Asynchronous WebSocket client for the VTube Studio Public API. Handles connection
/// lifecycle, request/response correlation, the two-step authentication-token flow, and
/// event dispatch. The transport, JSON, and protocol surfaces are protocol-native - no
/// host-application concepts leak into the public API.
/// </summary>
/// <remarks>
/// <para>
/// Typical use:
/// </para>
/// <code>
/// var client = new VTubeStudioClient(new VTubeStudioClientOptions
/// {
///     PluginName = "MyPlugin",
///     PluginDeveloper = "Me",
/// });
/// await client.ConnectAsync(ct);
/// string token = await client.RequestAndAuthenticateAsync(existingToken: null, ct);
/// await client.TriggerHotkeyAsync(new HotkeyTriggerRequest { HotkeyId = "..." }, ct);
/// </code>
/// </remarks>
public sealed partial class VTubeStudioClient : IAsyncDisposable
{
    private readonly VTubeStudioClientOptions _options;
    private readonly ILogger<VTubeStudioClient> _logger;

    internal readonly ConcurrentDictionary<string, TaskCompletionSource<VTubeStudioEnvelope>> _pending = new(StringComparer.Ordinal);
    private static readonly JsonElement _emptyData = JsonElement.Parse("{}");

    private ClientWebSocket? _ws;
    private CancellationTokenSource? _loopCts;
    private Task? _receiveLoop;
    private long _requestCounter;
    private bool _disposed;

    /// <summary>Creates a client with the given options and optional logger.</summary>
    /// <param name="options">Connection and plugin-identity options. <see cref="VTubeStudioClientOptions.PluginName"/> and <see cref="VTubeStudioClientOptions.PluginDeveloper"/> must be non-empty.</param>
    /// <param name="logger">Optional logger; a no-op logger is used when null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException"><see cref="VTubeStudioClientOptions.PluginName"/> or <see cref="VTubeStudioClientOptions.PluginDeveloper"/> is empty.</exception>
    public VTubeStudioClient(VTubeStudioClientOptions options, ILogger<VTubeStudioClient>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.PluginName))
        {
            throw new ArgumentException("PluginName must be non-empty.", nameof(options));
        }
        if (string.IsNullOrWhiteSpace(options.PluginDeveloper))
        {
            throw new ArgumentException("PluginDeveloper must be non-empty.", nameof(options));
        }
        _options = options;
        _logger = logger ?? NullLogger<VTubeStudioClient>.Instance;
        Events = new VTubeStudioEventHub();
    }

    /// <summary>Typed event hub. Use <c>Events.On&lt;HotkeyTriggeredEventPayload&gt;(...)</c> to register handlers.</summary>
    public VTubeStudioEventHub Events { get; }

    /// <summary>True once the WebSocket transport is open. Does not imply the session is authenticated.</summary>
    public bool IsConnected => _ws is { State: WebSocketState.Open };

    /// <summary>Raised for every event the server pushes that this client has subscribed to.</summary>
    public event EventHandler<VTubeStudioEventArgs>? EventReceived;

    /// <summary>Raised when the transport closes (planned or unexpected).</summary>
    public event EventHandler<EventArgs>? Disconnected;

    /// <summary>Open the WebSocket connection.</summary>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsConnected)
        {
            return;
        }

        _ws = new ClientWebSocket();
        await _ws.ConnectAsync(_options.Endpoint, ct).ConfigureAwait(false);
        _loopCts = new CancellationTokenSource();
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(_loopCts.Token), CancellationToken.None);
        LogConnected(_logger, _options.Endpoint);
    }

    /// <summary>Close the WebSocket cleanly. Idempotent.</summary>
    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_ws is null) return;
        try
        {
            if (_ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "client shutdown", ct).ConfigureAwait(false);
            }
        }
        catch (WebSocketException) { /* already torn down */ }

        await StopLoopAsync().ConfigureAwait(false);
        _ws.Dispose();
        _ws = null;
        // The receive loop also raises Disconnected on exit. We don't double-fire here.
    }

    /// <summary>
    /// Full two-step token flow: when <paramref name="existingToken"/> is null/empty, request a
    /// fresh token from the user (they approve in the VTube Studio UI), persist it for the caller
    /// (returned), and authenticate the session. When a stored token is supplied, authenticate
    /// with it directly - if the server rejects it, re-request and retry once.
    /// </summary>
    public async Task<string> RequestAndAuthenticateAsync(
        string? existingToken,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(existingToken))
        {
            AuthenticationResponse auth = await AuthenticateAsync(existingToken, ct).ConfigureAwait(false);
            if (auth.Authenticated)
            {
                return existingToken;
            }
            LogStoredTokenRejected(_logger, auth.Reason);
        }

        AuthenticationTokenResponse tokenResp = await RequestAuthenticationTokenAsync(ct).ConfigureAwait(false);
        AuthenticationResponse final = await AuthenticateAsync(tokenResp.AuthenticationToken, ct).ConfigureAwait(false);
        return final.Authenticated
            ? tokenResp.AuthenticationToken
            : throw new VTubeStudioApiException(
                VTubeStudioErrorId.Unknown,
                (int)VTubeStudioErrorId.Unknown,
                final.Reason ?? "Authentication failed after fresh token request.");
    }

    /// <summary>Request a fresh authentication token. The user must approve the prompt in VTube Studio.</summary>
    public async Task<AuthenticationTokenResponse> RequestAuthenticationTokenAsync(CancellationToken ct = default)
    {
        AuthenticationTokenRequest req = new()
        {
            PluginName = _options.PluginName,
            PluginDeveloper = _options.PluginDeveloper,
            PluginIcon = _options.PluginIcon,
        };
        // The user's approval timeout is longer than the regular request timeout.
        return await SendAsync(
            VTubeStudioMessageTypes.AuthenticationTokenRequest,
            req,
            VTubeStudioJsonContext.Default.AuthenticationTokenRequest,
            VTubeStudioJsonContext.Default.AuthenticationTokenResponse,
            _options.AuthApprovalTimeout,
            ct).ConfigureAwait(false);
    }

    /// <summary>Authenticate the current session with a previously-obtained token.</summary>
    public Task<AuthenticationResponse> AuthenticateAsync(string token, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        AuthenticationRequest req = new()
        {
            PluginName = _options.PluginName,
            PluginDeveloper = _options.PluginDeveloper,
            AuthenticationToken = token,
        };
        return SendAsync(
            VTubeStudioMessageTypes.AuthenticationRequest,
            req,
            VTubeStudioJsonContext.Default.AuthenticationRequest,
            VTubeStudioJsonContext.Default.AuthenticationResponse,
            _options.RequestTimeout,
            ct);
    }

    /// <summary>Request a permission, or list granted permissions when <paramref name="requestedPermission"/> is null. Requesting shows a VTube Studio popup.</summary>
    /// <param name="requestedPermission">The permission to request; null lists permissions without prompting.</param>
    /// <param name="timeout">How long to wait for the user; defaults to two minutes.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The grant result and the permission list.</returns>
    public Task<PermissionResponse> RequestPermissionAsync(string? requestedPermission = null, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        PermissionRequest req = new() { RequestedPermission = requestedPermission };
        return SendAsync(VTubeStudioMessageTypes.PermissionRequest, req,
            VTubeStudioJsonContext.Default.PermissionRequest, VTubeStudioJsonContext.Default.PermissionResponse,
            timeout ?? TimeSpan.FromMinutes(2), ct);
    }

    // ── API surface (full coverage) ────────────────────────────────────────

    /// <summary>Query whether the API is active and whether the current session is authenticated.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The API state response.</returns>
    public Task<ApiStateResponse> GetApiStateAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.ApiStateRequest, VTubeStudioJsonContext.Default.ApiStateResponse, ct);

    /// <summary>Query VTube Studio runtime statistics (uptime, framerate, plugin counts, window metrics).</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The statistics response.</returns>
    public Task<StatisticsResponse> GetStatisticsAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.StatisticsRequest, VTubeStudioJsonContext.Default.StatisticsResponse, ct);

    /// <summary>Query whether a face is currently being tracked.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The face-found response.</returns>
    public Task<FaceFoundResponse> GetFaceFoundAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.FaceFoundRequest, VTubeStudioJsonContext.Default.FaceFoundResponse, ct);

    /// <summary>Query the VTube Studio folder names.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The folder-info response.</returns>
    public Task<VtsFolderInfoResponse> GetVtsFolderInfoAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.VtsFolderInfoRequest, VTubeStudioJsonContext.Default.VtsFolderInfoResponse, ct);

    /// <summary>Query information about the currently loaded model.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The current-model response.</returns>
    public Task<CurrentModelResponse> GetCurrentModelAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.CurrentModelRequest, VTubeStudioJsonContext.Default.CurrentModelResponse, ct);

    /// <summary>List all models available on the machine.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The available-models response.</returns>
    public Task<AvailableModelsResponse> GetAvailableModelsAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.AvailableModelsRequest, VTubeStudioJsonContext.Default.AvailableModelsResponse, ct);

    /// <summary>Load a model by id (an empty id unloads the current model).</summary>
    /// <param name="request">The model-load request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The model-load response confirming which model was loaded.</returns>
    public Task<ModelLoadResponse> LoadModelAsync(ModelLoadRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ModelLoadRequest, request,
            VTubeStudioJsonContext.Default.ModelLoadRequest, VTubeStudioJsonContext.Default.ModelLoadResponse, _options.RequestTimeout, ct);

    /// <summary>Move, rotate, and scale the currently loaded model.</summary>
    /// <param name="request">The move-model request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    public Task MoveModelAsync(MoveModelRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.MoveModelRequest, request,
            VTubeStudioJsonContext.Default.MoveModelRequest, ct);

    /// <summary>Query the physics settings of the currently loaded model.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The physics-settings response.</returns>
    public Task<GetCurrentModelPhysicsResponse> GetCurrentModelPhysicsAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.GetCurrentModelPhysicsRequest, VTubeStudioJsonContext.Default.GetCurrentModelPhysicsResponse, ct);

    /// <summary>Temporarily override the physics settings of the currently loaded model. Overrides expire on their timers.</summary>
    /// <param name="request">The physics-override request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    public Task SetCurrentModelPhysicsAsync(SetCurrentModelPhysicsRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.SetCurrentModelPhysicsRequest, request,
            VTubeStudioJsonContext.Default.SetCurrentModelPhysicsRequest, ct);

    /// <summary>Query the scene lighting overlay state.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The lighting-overlay response.</returns>
    public Task<SceneColorOverlayInfoResponse> GetSceneColorOverlayInfoAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.SceneColorOverlayInfoRequest, VTubeStudioJsonContext.Default.SceneColorOverlayInfoResponse, ct);

    /// <summary>Query the NDI configuration.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The NDI configuration response.</returns>
    public Task<NdiConfigResponse> GetNdiConfigAsync(CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.NdiConfigRequest, new NdiConfigRequest(),
            VTubeStudioJsonContext.Default.NdiConfigRequest, VTubeStudioJsonContext.Default.NdiConfigResponse, _options.RequestTimeout, ct);

    /// <summary>Change the NDI configuration. Requires permission and honors a server-side cooldown.</summary>
    /// <param name="request">The NDI configuration to apply.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The NDI configuration response.</returns>
    public Task<NdiConfigResponse> SetNdiConfigAsync(NdiConfigRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.NdiConfigRequest, request,
            VTubeStudioJsonContext.Default.NdiConfigRequest, VTubeStudioJsonContext.Default.NdiConfigResponse, _options.RequestTimeout, ct);

    /// <summary>List post-processing effects and state.</summary>
    /// <param name="request">Selects which arrays are filled and filters effects.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The post-processing list response.</returns>
    public Task<PostProcessingListResponse> GetPostProcessingAsync(PostProcessingListRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.PostProcessingListRequest, request,
            VTubeStudioJsonContext.Default.PostProcessingListRequest, VTubeStudioJsonContext.Default.PostProcessingListResponse, _options.RequestTimeout, ct);

    /// <summary>Change post-processing effects.</summary>
    /// <param name="request">The post-processing update request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The post-processing state after the update.</returns>
    public Task<PostProcessingUpdateResponse> UpdatePostProcessingAsync(PostProcessingUpdateRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.PostProcessingUpdateRequest, request,
            VTubeStudioJsonContext.Default.PostProcessingUpdateRequest, VTubeStudioJsonContext.Default.PostProcessingUpdateResponse, _options.RequestTimeout, ct);

    /// <summary>List the hotkeys available in the current (or a specified) model.</summary>
    /// <param name="request">Optional request narrowing to a specific model or item; null queries the current model.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The hotkeys response.</returns>
    public Task<HotkeysInCurrentModelResponse> GetHotkeysAsync(HotkeysInCurrentModelRequest? request = null, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.HotkeysInCurrentModelRequest, request ?? new HotkeysInCurrentModelRequest(),
            VTubeStudioJsonContext.Default.HotkeysInCurrentModelRequest, VTubeStudioJsonContext.Default.HotkeysInCurrentModelResponse, _options.RequestTimeout, ct);

    /// <summary>Trigger (execute) a hotkey by id or name.</summary>
    /// <param name="request">The hotkey-trigger request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The response confirming which hotkey was triggered.</returns>
    public Task<HotkeyTriggerResponse> TriggerHotkeyAsync(HotkeyTriggerRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.HotkeyTriggerRequest, request,
            VTubeStudioJsonContext.Default.HotkeyTriggerRequest, VTubeStudioJsonContext.Default.HotkeyTriggerResponse, _options.RequestTimeout, ct);

    /// <summary>Query the activation state of the model's expressions.</summary>
    /// <param name="request">Optional request enabling details or narrowing to one expression; null queries all.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The expression-state response.</returns>
    public Task<ExpressionStateResponse> GetExpressionStateAsync(ExpressionStateRequest? request = null, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ExpressionStateRequest, request ?? new ExpressionStateRequest(),
            VTubeStudioJsonContext.Default.ExpressionStateRequest, VTubeStudioJsonContext.Default.ExpressionStateResponse, _options.RequestTimeout, ct);

    /// <summary>Activate or deactivate an expression.</summary>
    /// <param name="request">The expression-activation request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    public Task SetExpressionAsync(ExpressionActivationRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.ExpressionActivationRequest, request,
            VTubeStudioJsonContext.Default.ExpressionActivationRequest, ct);

    /// <summary>List the ArtMesh names and tags in the current model.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The ArtMesh-list response.</returns>
    public Task<ArtMeshListResponse> GetArtMeshListAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.ArtMeshListRequest, VTubeStudioJsonContext.Default.ArtMeshListResponse, ct);

    /// <summary>Apply a color tint to the ArtMeshes selected by the request's matcher.</summary>
    /// <param name="request">The color-tint request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The tint response reporting how many ArtMeshes were tinted.</returns>
    public Task<ColorTintResponse> TintArtMeshAsync(ColorTintRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ColorTintRequest, request,
            VTubeStudioJsonContext.Default.ColorTintRequest, VTubeStudioJsonContext.Default.ColorTintResponse, _options.RequestTimeout, ct);

    /// <summary>List the ArtMeshes at a position in the current model.</summary>
    /// <param name="request">The position request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The ArtMeshes at the checked position, topmost first.</returns>
    public Task<ArtMeshAtPositionResponse> GetArtMeshesAtPositionAsync(ArtMeshAtPositionRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ArtMeshAtPositionRequest, request,
            VTubeStudioJsonContext.Default.ArtMeshAtPositionRequest, VTubeStudioJsonContext.Default.ArtMeshAtPositionResponse, _options.RequestTimeout, ct);

    /// <summary>Ask the user to select ArtMeshes. The response arrives once the user confirms or cancels.</summary>
    /// <param name="request">The selection request.</param>
    /// <param name="timeout">How long to wait for the user; defaults to five minutes.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The user's selection.</returns>
    public Task<ArtMeshSelectionResponse> RequestArtMeshSelectionAsync(ArtMeshSelectionRequest request, TimeSpan? timeout = null, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ArtMeshSelectionRequest, request,
            VTubeStudioJsonContext.Default.ArtMeshSelectionRequest, VTubeStudioJsonContext.Default.ArtMeshSelectionResponse,
            timeout ?? TimeSpan.FromMinutes(5), ct);

    /// <summary>List the available tracking input parameters (default and custom).</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The input-parameter-list response.</returns>
    public Task<InputParameterListResponse> GetInputParametersAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.InputParameterListRequest, VTubeStudioJsonContext.Default.InputParameterListResponse, ct);

    /// <summary>List the current model's Live2D parameters and their values.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The Live2D-parameter-list response.</returns>
    public Task<Live2DParameterListResponse> GetLive2DParametersAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.Live2DParameterListRequest, VTubeStudioJsonContext.Default.Live2DParameterListResponse, ct);

    /// <summary>Query the current value and range of a single parameter.</summary>
    /// <param name="request">The parameter-value request naming the parameter.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The parameter's value and range.</returns>
    public Task<ParameterInfo> GetParameterValueAsync(ParameterValueRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ParameterValueRequest, request,
            VTubeStudioJsonContext.Default.ParameterValueRequest, VTubeStudioJsonContext.Default.ParameterInfo, _options.RequestTimeout, ct);

    /// <summary>Inject tracking data into one or more parameters.</summary>
    /// <param name="request">The inject-parameter-data request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    public Task InjectParameterDataAsync(InjectParameterDataRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.InjectParameterDataRequest, request,
            VTubeStudioJsonContext.Default.InjectParameterDataRequest, ct);

    /// <summary>Create a custom tracking parameter.</summary>
    /// <param name="request">The parameter-creation request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The creation response confirming the parameter name.</returns>
    public Task<ParameterCreationResponse> CreateParameterAsync(ParameterCreationRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ParameterCreationRequest, request,
            VTubeStudioJsonContext.Default.ParameterCreationRequest, VTubeStudioJsonContext.Default.ParameterCreationResponse, _options.RequestTimeout, ct);

    /// <summary>Delete a custom tracking parameter.</summary>
    /// <param name="request">The parameter-deletion request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The deletion response confirming the parameter name.</returns>
    public Task<ParameterDeletionResponse> DeleteParameterAsync(ParameterDeletionRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ParameterDeletionRequest, request,
            VTubeStudioJsonContext.Default.ParameterDeletionRequest, VTubeStudioJsonContext.Default.ParameterDeletionResponse, _options.RequestTimeout, ct);

    /// <summary>List available item files and/or the items currently loaded in the scene.</summary>
    /// <param name="request">Optional request selecting which lists to include; null uses defaults.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The item-list response.</returns>
    public Task<ItemListResponse> GetItemListAsync(ItemListRequest? request = null, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemListRequest, request ?? new ItemListRequest(),
            VTubeStudioJsonContext.Default.ItemListRequest, VTubeStudioJsonContext.Default.ItemListResponse, _options.RequestTimeout, ct);

    /// <summary>Load an item into the scene.</summary>
    /// <param name="request">The item-load request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The item-load response carrying the new item's instance id.</returns>
    public Task<ItemLoadResponse> LoadItemAsync(ItemLoadRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemLoadRequest, request,
            VTubeStudioJsonContext.Default.ItemLoadRequest, VTubeStudioJsonContext.Default.ItemLoadResponse, _options.RequestTimeout, ct);

    /// <summary>Unload one or more items from the scene.</summary>
    /// <param name="request">The item-unload request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The item-unload response listing the items that were unloaded.</returns>
    public Task<ItemUnloadResponse> UnloadItemAsync(ItemUnloadRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemUnloadRequest, request,
            VTubeStudioJsonContext.Default.ItemUnloadRequest, VTubeStudioJsonContext.Default.ItemUnloadResponse, _options.RequestTimeout, ct);

    /// <summary>Control playback and appearance of an item.</summary>
    /// <param name="request">The animation-control request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The item animation state.</returns>
    public Task<ItemAnimationControlResponse> ControlItemAnimationAsync(ItemAnimationControlRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemAnimationControlRequest, request,
            VTubeStudioJsonContext.Default.ItemAnimationControlRequest, VTubeStudioJsonContext.Default.ItemAnimationControlResponse, _options.RequestTimeout, ct);

    /// <summary>Move one or more items in the scene.</summary>
    /// <param name="request">The item-move request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The per-item move results.</returns>
    public Task<ItemMoveResponse> MoveItemsAsync(ItemMoveRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemMoveRequest, request,
            VTubeStudioJsonContext.Default.ItemMoveRequest, VTubeStudioJsonContext.Default.ItemMoveResponse, _options.RequestTimeout, ct);

    /// <summary>Sort an item between the layers of the model.</summary>
    /// <param name="request">The item-sort request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The applied sorting.</returns>
    public Task<ItemSortResponse> SortItemAsync(ItemSortRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemSortRequest, request,
            VTubeStudioJsonContext.Default.ItemSortRequest, VTubeStudioJsonContext.Default.ItemSortResponse, _options.RequestTimeout, ct);

    /// <summary>Pin an item to the model, or unpin it.</summary>
    /// <param name="request">The item-pin request.</param>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The pin state of the item.</returns>
    public Task<ItemPinResponse> PinItemAsync(ItemPinRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemPinRequest, request,
            VTubeStudioJsonContext.Default.ItemPinRequest, VTubeStudioJsonContext.Default.ItemPinResponse, _options.RequestTimeout, ct);

    /// <summary>
    /// Subscribe (or unsubscribe) the current session to a typed event payload. The wire-format
    /// event name is resolved from the payload type via <see cref="IVTubeStudioEvent{TSelf}"/>.
    /// </summary>
    public Task<EventSubscriptionResponse> SubscribeAsync<TPayload>(bool subscribe = true, CancellationToken ct = default)
        where TPayload : class, IVTubeStudioEvent<TPayload>
        => SubscribeAsync(TPayload.EventName, subscribe, ct);

    /// <summary>Subscribe (or unsubscribe) the current session to a named event without any config.</summary>
    public Task<EventSubscriptionResponse> SubscribeAsync(string eventName, bool subscribe = true, CancellationToken ct = default)
    {
        EventSubscriptionRequest req = new()
        {
            EventName = eventName,
            Subscribe = subscribe,
            Config = null,
        };
        return SendAsync(VTubeStudioMessageTypes.EventSubscriptionRequest, req,
            VTubeStudioJsonContext.Default.EventSubscriptionRequest, VTubeStudioJsonContext.Default.EventSubscriptionResponse, _options.RequestTimeout, ct);
    }

    /// <summary>Subscribe to a named event with a typed config record (e.g. <see cref="HotkeyTriggeredEventConfig"/>).</summary>
    public Task<EventSubscriptionResponse> SubscribeWithConfigAsync<TConfig>(
        string eventName,
        TConfig config,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TConfig> typeInfo,
        bool subscribe = true,
        CancellationToken ct = default)
        where TConfig : class
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(typeInfo);
        JsonElement configElement = JsonSerializer.SerializeToElement(config, typeInfo);
        EventSubscriptionRequest req = new()
        {
            EventName = eventName,
            Subscribe = subscribe,
            Config = configElement,
        };
        return SendAsync(VTubeStudioMessageTypes.EventSubscriptionRequest, req,
            VTubeStudioJsonContext.Default.EventSubscriptionRequest, VTubeStudioJsonContext.Default.EventSubscriptionResponse, _options.RequestTimeout, ct);
    }

    /// <summary>Unsubscribe the current session from all events.</summary>
    /// <param name="ct">Token to cancel the request.</param>
    /// <returns>The subscription response, with an empty event list.</returns>
    public Task<EventSubscriptionResponse> UnsubscribeFromAllEventsAsync(CancellationToken ct = default)
    {
        EventSubscriptionRequest req = new() { Subscribe = false };
        return SendAsync(VTubeStudioMessageTypes.EventSubscriptionRequest, req,
            VTubeStudioJsonContext.Default.EventSubscriptionRequest, VTubeStudioJsonContext.Default.EventSubscriptionResponse, _options.RequestTimeout, ct);
    }

    // ── Internals ──────────────────────────────────────────────────────────

    private async Task<TResp> SendEmptyRequestAsync<TResp>(
        string messageType,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TResp> responseType,
        CancellationToken ct)
    {
        VTubeStudioEnvelope env = await SendEnvelopeAsync(messageType, _emptyData, _options.RequestTimeout, ct).ConfigureAwait(false);
        return env.Data.Deserialize(responseType)
            ?? throw new InvalidOperationException($"Response payload for {messageType} was null.");
    }

    private async Task<TResp> SendAsync<TReq, TResp>(
        string messageType,
        TReq request,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TReq> requestType,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TResp> responseType,
        TimeSpan timeout,
        CancellationToken ct)
    {
        JsonElement data = JsonSerializer.SerializeToElement(request, requestType);
        VTubeStudioEnvelope env = await SendEnvelopeAsync(messageType, data, timeout, ct).ConfigureAwait(false);
        return env.Data.Deserialize(responseType)
            ?? throw new InvalidOperationException($"Response payload for {messageType} was null.");
    }

    private async Task SendAndDiscardAsync<TReq>(
        string messageType,
        TReq request,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TReq> requestType,
        CancellationToken ct)
    {
        JsonElement data = JsonSerializer.SerializeToElement(request, requestType);
        _ = await SendEnvelopeAsync(messageType, data, _options.RequestTimeout, ct).ConfigureAwait(false);
    }

    private async Task<VTubeStudioEnvelope> SendEnvelopeAsync(
        string messageType,
        JsonElement data,
        TimeSpan timeout,
        CancellationToken ct)
    {
        if (_ws is null || _ws.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
        }

        string requestId = (Interlocked.Increment(ref _requestCounter)).ToString(System.Globalization.CultureInfo.InvariantCulture);
        TaskCompletionSource<VTubeStudioEnvelope> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(requestId, tcs))
        {
            throw new InvalidOperationException($"Request id collision on '{requestId}' (should be impossible).");
        }

        VTubeStudioEnvelope envelope = new()
        {
            MessageType = messageType,
            RequestId = requestId,
            Data = data,
        };
        string json = JsonSerializer.Serialize(envelope, VTubeStudioJsonContext.Default.VTubeStudioEnvelope);
        byte[] buffer = Encoding.UTF8.GetBytes(json);

        await _ws.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage: true, ct).ConfigureAwait(false);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);
        try
        {
            using (timeoutCts.Token.Register(static state => ((TaskCompletionSource<VTubeStudioEnvelope>)state!).TrySetCanceled(), tcs))
            {
                VTubeStudioEnvelope response = await tcs.Task.ConfigureAwait(false);
                if (response.MessageType == VTubeStudioMessageTypes.ApiError)
                {
                    ApiErrorData? err = response.Data.Deserialize(VTubeStudioJsonContext.Default.ApiErrorData);
                    if (err is not null)
                    {
                        VTubeStudioErrorId errId = Enum.IsDefined(typeof(VTubeStudioErrorId), err.ErrorId)
                            ? (VTubeStudioErrorId)err.ErrorId
                            : VTubeStudioErrorId.Unknown;
                        throw new VTubeStudioApiException(errId, err.ErrorId, err.Message);
                    }
                }
                return response;
            }
        }
        finally
        {
            _ = _pending.TryRemove(requestId, out _);
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        if (_ws is null) return;
        byte[] buffer = new byte[_options.ReceiveBufferSize];
        using MemoryStream ms = new();
        try
        {
            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                ms.SetLength(0);
                WebSocketReceiveResult result;
                do
                {
                    result = await _ws.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                string json = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                DispatchMessage(json);
            }
        }
        catch (OperationCanceledException) { /* shutdown */ }
        catch (WebSocketException ex)
        {
            LogReceiveLoopFailed(_logger, ex);
        }
        finally
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    internal void DispatchMessage(string json)
    {
        try
        {
            VTubeStudioEnvelope? env = JsonSerializer.Deserialize(json, VTubeStudioJsonContext.Default.VTubeStudioEnvelope);
            if (env is null) return;

            // Event frames carry a requestID, so only messageType separates
            // events from responses. Events end in Event.
            if (env.MessageType.EndsWith("Event", StringComparison.Ordinal))
            {
                Events.Dispatch(env.MessageType, env.Data);
                EventReceived?.Invoke(this, new VTubeStudioEventArgs
                {
                    EventName = env.MessageType,
                    RawData = env.Data,
                    ReceivedAtUtc = DateTimeOffset.UtcNow,
                });
                return;
            }

            if (!string.IsNullOrEmpty(env.RequestId)
                && _pending.TryRemove(env.RequestId, out TaskCompletionSource<VTubeStudioEnvelope>? tcs))
            {
                _ = tcs.TrySetResult(env);
                return;
            }

            // Late or unknown responses are dropped.
        }
        catch (JsonException ex)
        {
            LogDeserializeFailed(_logger, ex);
        }
    }

    private async Task StopLoopAsync()
    {
        if (_loopCts is null) return;
        await _loopCts.CancelAsync().ConfigureAwait(false);
        try
        {
            if (_receiveLoop is not null)
            {
                await _receiveLoop.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { /* expected */ }
        _loopCts.Dispose();
        _loopCts = null;
        _receiveLoop = null;
    }

    /// <summary>Disconnects the client (if connected) and releases the underlying transport. Idempotent.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Connected to VTube Studio at {Endpoint}.")]
    private static partial void LogConnected(ILogger logger, Uri endpoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Stored token rejected by VTube Studio ({Reason}); requesting a fresh token.")]
    private static partial void LogStoredTokenRejected(ILogger logger, string? reason);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "VTube Studio WebSocket receive loop terminated.")]
    private static partial void LogReceiveLoopFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Failed to deserialize VTube Studio frame.")]
    private static partial void LogDeserializeFailed(ILogger logger, Exception ex);
}
