using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of an <c>AuthenticationTokenRequest</c>: asks the user to grant a fresh authentication token.</summary>
public sealed record AuthenticationTokenRequest
{
    /// <summary>Plugin name (3-32 characters) shown to the user in the approval prompt.</summary>
    [JsonPropertyName("pluginName")] public required string PluginName { get; init; }

    /// <summary>Plugin developer name (3-32 characters) shown to the user in the approval prompt.</summary>
    [JsonPropertyName("pluginDeveloper")] public required string PluginDeveloper { get; init; }

    /// <summary>Optional 128×128 PNG icon, base64-encoded (no <c>data:</c> prefix), shown in the approval prompt.</summary>
    [JsonPropertyName("pluginIcon")] public string? PluginIcon { get; init; }
}

/// <summary>Payload of an <c>AuthenticationTokenResponse</c>: the token granted by the user.</summary>
public sealed record AuthenticationTokenResponse
{
    /// <summary>The authentication token (max 64 characters) to store and reuse on future sessions.</summary>
    [JsonPropertyName("authenticationToken")] public required string AuthenticationToken { get; init; }
}

/// <summary>Payload of an <c>AuthenticationRequest</c>: authenticates the session with a stored token.</summary>
public sealed record AuthenticationRequest
{
    /// <summary>Plugin name; must match the value used when the token was requested.</summary>
    [JsonPropertyName("pluginName")] public required string PluginName { get; init; }

    /// <summary>Plugin developer name; must match the value used when the token was requested.</summary>
    [JsonPropertyName("pluginDeveloper")] public required string PluginDeveloper { get; init; }

    /// <summary>A previously granted authentication token.</summary>
    [JsonPropertyName("authenticationToken")] public required string AuthenticationToken { get; init; }
}

/// <summary>Payload of an <c>AuthenticationResponse</c>: reports whether the session is now authenticated.</summary>
public sealed record AuthenticationResponse
{
    /// <summary>True when the session is now authenticated.</summary>
    [JsonPropertyName("authenticated")] public required bool Authenticated { get; init; }

    /// <summary>Human-readable explanation of the authentication result (for example, why it failed).</summary>
    [JsonPropertyName("reason")] public string? Reason { get; init; }
}

/// <summary>Payload of an <c>APIError</c> frame: the numeric error id and message describing a failed request.</summary>
public sealed record ApiErrorData
{
    /// <summary>The numeric VTube Studio <c>errorID</c>. See <see cref="Errors.VTubeStudioErrorId"/> for known values.</summary>
    [JsonPropertyName("errorID")] public required int ErrorId { get; init; }

    /// <summary>Human-readable description of the error.</summary>
    [JsonPropertyName("message")] public required string Message { get; init; }
}
