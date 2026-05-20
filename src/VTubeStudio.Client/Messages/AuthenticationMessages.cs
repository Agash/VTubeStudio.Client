using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record AuthenticationTokenRequest
{
    [JsonPropertyName("pluginName")] public required string PluginName { get; init; }
    [JsonPropertyName("pluginDeveloper")] public required string PluginDeveloper { get; init; }
    /// <summary>Optional 128×128 PNG icon, base64-encoded (no <c>data:</c> prefix).</summary>
    [JsonPropertyName("pluginIcon")] public string? PluginIcon { get; init; }
}

public sealed record AuthenticationTokenResponse
{
    [JsonPropertyName("authenticationToken")] public required string AuthenticationToken { get; init; }
}

public sealed record AuthenticationRequest
{
    [JsonPropertyName("pluginName")] public required string PluginName { get; init; }
    [JsonPropertyName("pluginDeveloper")] public required string PluginDeveloper { get; init; }
    [JsonPropertyName("authenticationToken")] public required string AuthenticationToken { get; init; }
}

public sealed record AuthenticationResponse
{
    [JsonPropertyName("authenticated")] public required bool Authenticated { get; init; }
    [JsonPropertyName("reason")] public string? Reason { get; init; }
}

public sealed record ApiErrorData
{
    [JsonPropertyName("errorID")] public required int ErrorId { get; init; }
    [JsonPropertyName("message")] public required string Message { get; init; }
}
