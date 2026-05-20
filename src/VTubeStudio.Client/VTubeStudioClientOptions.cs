namespace VTubeStudio.Client;

/// <summary>Configuration for a <see cref="VTubeStudioClient"/> instance.</summary>
/// <remarks>
/// Properties use <c>set</c> rather than <c>init</c> so that the options pattern's
/// <c>Action&lt;T&gt;</c> configurator can mutate the default instance. The client's
/// constructor validates non-empty <see cref="PluginName"/> and <see cref="PluginDeveloper"/>.
/// </remarks>
public sealed class VTubeStudioClientOptions
{
    /// <summary>WebSocket endpoint. Defaults to <see cref="VTubeStudioApi.DefaultEndpoint"/>.</summary>
    public Uri Endpoint { get; set; } = VTubeStudioApi.DefaultEndpoint;

    /// <summary>Plugin name surfaced in the VTube Studio approval dialog and required for authentication.</summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>Plugin developer name surfaced in the approval dialog and required for authentication.</summary>
    public string PluginDeveloper { get; set; } = string.Empty;

    /// <summary>Optional 128×128 PNG icon (base64-encoded, no <c>data:</c> prefix) shown in the approval dialog.</summary>
    public string? PluginIcon { get; set; }

    /// <summary>How long to wait for each request's response before timing out.</summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>How long to wait for the user to approve the auth-token prompt in VTube Studio.</summary>
    public TimeSpan AuthApprovalTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Buffer size for the receive loop (bytes). Increase for very large responses.</summary>
    public int ReceiveBufferSize { get; set; } = 16 * 1024;
}
