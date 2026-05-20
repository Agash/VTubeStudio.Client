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
/// event dispatch. The transport, JSON, and protocol surfaces are protocol-native — no
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
    private readonly ConcurrentDictionary<string, TaskCompletionSource<VTubeStudioEnvelope>> _pending = new(StringComparer.Ordinal);
    private static readonly JsonElement _emptyData = JsonDocument.Parse("{}").RootElement.Clone();

    private ClientWebSocket? _ws;
    private CancellationTokenSource? _loopCts;
    private Task? _receiveLoop;
    private long _requestCounter;
    private bool _disposed;

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
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Full two-step token flow: when <paramref name="existingToken"/> is null/empty, request a
    /// fresh token from the user (they approve in the VTube Studio UI), persist it for the caller
    /// (returned), and authenticate the session. When a stored token is supplied, authenticate
    /// with it directly — if the server rejects it, re-request and retry once.
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
        if (!final.Authenticated)
        {
            throw new VTubeStudioApiException(
                VTubeStudioErrorId.AuthenticationTokenInvalid,
                (int)VTubeStudioErrorId.AuthenticationTokenInvalid,
                final.Reason ?? "Authentication failed after fresh token request.");
        }
        return tokenResp.AuthenticationToken;
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

    // ── API surface (full coverage) ────────────────────────────────────────

    public Task<ApiStateResponse> GetApiStateAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.ApiStateRequest, VTubeStudioJsonContext.Default.ApiStateResponse, ct);

    public Task<StatisticsResponse> GetStatisticsAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.StatisticsRequest, VTubeStudioJsonContext.Default.StatisticsResponse, ct);

    public Task<FaceFoundResponse> GetFaceFoundAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.FaceFoundRequest, VTubeStudioJsonContext.Default.FaceFoundResponse, ct);

    public Task<CurrentModelResponse> GetCurrentModelAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.CurrentModelRequest, VTubeStudioJsonContext.Default.CurrentModelResponse, ct);

    public Task<AvailableModelsResponse> GetAvailableModelsAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.AvailableModelsRequest, VTubeStudioJsonContext.Default.AvailableModelsResponse, ct);

    public Task<ModelLoadResponse> LoadModelAsync(ModelLoadRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ModelLoadRequest, request,
            VTubeStudioJsonContext.Default.ModelLoadRequest, VTubeStudioJsonContext.Default.ModelLoadResponse, _options.RequestTimeout, ct);

    public Task MoveModelAsync(MoveModelRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.MoveModelRequest, request,
            VTubeStudioJsonContext.Default.MoveModelRequest, ct);

    public Task<HotkeysInCurrentModelResponse> GetHotkeysAsync(HotkeysInCurrentModelRequest? request = null, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.HotkeysInCurrentModelRequest, request ?? new HotkeysInCurrentModelRequest(),
            VTubeStudioJsonContext.Default.HotkeysInCurrentModelRequest, VTubeStudioJsonContext.Default.HotkeysInCurrentModelResponse, _options.RequestTimeout, ct);

    public Task<HotkeyTriggerResponse> TriggerHotkeyAsync(HotkeyTriggerRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.HotkeyTriggerRequest, request,
            VTubeStudioJsonContext.Default.HotkeyTriggerRequest, VTubeStudioJsonContext.Default.HotkeyTriggerResponse, _options.RequestTimeout, ct);

    public Task<ExpressionStateResponse> GetExpressionStateAsync(ExpressionStateRequest? request = null, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ExpressionStateRequest, request ?? new ExpressionStateRequest(),
            VTubeStudioJsonContext.Default.ExpressionStateRequest, VTubeStudioJsonContext.Default.ExpressionStateResponse, _options.RequestTimeout, ct);

    public Task SetExpressionAsync(ExpressionActivationRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.ExpressionActivationRequest, request,
            VTubeStudioJsonContext.Default.ExpressionActivationRequest, ct);

    public Task<ArtMeshListResponse> GetArtMeshListAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.ArtMeshListRequest, VTubeStudioJsonContext.Default.ArtMeshListResponse, ct);

    public Task TintArtMeshAsync(ColorTintRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.ColorTintRequest, request,
            VTubeStudioJsonContext.Default.ColorTintRequest, ct);

    public Task<InputParameterListResponse> GetInputParametersAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.InputParameterListRequest, VTubeStudioJsonContext.Default.InputParameterListResponse, ct);

    public Task<Live2DParameterListResponse> GetLive2DParametersAsync(CancellationToken ct = default) =>
        SendEmptyRequestAsync(VTubeStudioMessageTypes.Live2DParameterListRequest, VTubeStudioJsonContext.Default.Live2DParameterListResponse, ct);

    public Task<ParameterInfo> GetParameterValueAsync(ParameterValueRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ParameterValueRequest, request,
            VTubeStudioJsonContext.Default.ParameterValueRequest, VTubeStudioJsonContext.Default.ParameterInfo, _options.RequestTimeout, ct);

    public Task InjectParameterDataAsync(InjectParameterDataRequest request, CancellationToken ct = default) =>
        SendAndDiscardAsync(VTubeStudioMessageTypes.InjectParameterDataRequest, request,
            VTubeStudioJsonContext.Default.InjectParameterDataRequest, ct);

    public Task<ItemListResponse> GetItemListAsync(ItemListRequest? request = null, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemListRequest, request ?? new ItemListRequest(),
            VTubeStudioJsonContext.Default.ItemListRequest, VTubeStudioJsonContext.Default.ItemListResponse, _options.RequestTimeout, ct);

    public Task<ItemLoadResponse> LoadItemAsync(ItemLoadRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemLoadRequest, request,
            VTubeStudioJsonContext.Default.ItemLoadRequest, VTubeStudioJsonContext.Default.ItemLoadResponse, _options.RequestTimeout, ct);

    public Task<ItemUnloadResponse> UnloadItemAsync(ItemUnloadRequest request, CancellationToken ct = default) =>
        SendAsync(VTubeStudioMessageTypes.ItemUnloadRequest, request,
            VTubeStudioJsonContext.Default.ItemUnloadRequest, VTubeStudioJsonContext.Default.ItemUnloadResponse, _options.RequestTimeout, ct);

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

    private void DispatchMessage(string json)
    {
        try
        {
            VTubeStudioEnvelope? env = JsonSerializer.Deserialize(json, VTubeStudioJsonContext.Default.VTubeStudioEnvelope);
            if (env is null) return;

            if (!string.IsNullOrEmpty(env.RequestId)
                && _pending.TryRemove(env.RequestId, out TaskCompletionSource<VTubeStudioEnvelope>? tcs))
            {
                _ = tcs.TrySetResult(env);
                return;
            }

            // Unsolicited message — dispatch to the typed hub + raw event for escape-hatch consumers.
            Events.Dispatch(env.MessageType, env.Data);
            EventReceived?.Invoke(this, new VTubeStudioEventArgs
            {
                EventName = env.MessageType,
                RawData = env.Data,
                ReceivedAtUtc = DateTimeOffset.UtcNow,
            });
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
