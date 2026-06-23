using System.Text.Json;
using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>
/// Wire envelope shared by every VTube Studio request, response, event, and error.
/// Carries protocol metadata; the typed payload lives in <see cref="Data"/>.
/// </summary>
public sealed record VTubeStudioEnvelope
{
    /// <summary>The API name; always <see cref="VTubeStudioApi.ApiName"/> (<c>"VTubeStudioPublicAPI"</c>).</summary>
    [JsonPropertyName("apiName")]
    public string ApiName { get; init; } = VTubeStudioApi.ApiName;

    /// <summary>The API version; <see cref="VTubeStudioApi.ApiVersion"/> (<c>"1.0"</c>) on requests this client sends.</summary>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; init; } = VTubeStudioApi.ApiVersion;

    /// <summary>Unix timestamp (milliseconds) set by VTube Studio on responses and events; unused on outgoing requests.</summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    /// <summary>The message discriminator; one of the values in <see cref="VTubeStudioMessageTypes"/>.</summary>
    [JsonPropertyName("messageType")]
    public required string MessageType { get; init; }

    /// <summary>Correlation id echoed back on the matching response; null on unsolicited event frames.</summary>
    [JsonPropertyName("requestID")]
    public string? RequestId { get; init; }

    /// <summary>Raw JSON payload - parsed against the appropriate typed record per <see cref="MessageType"/>.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}
