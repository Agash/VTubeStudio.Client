using System.Text.Json;
using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>
/// Wire envelope shared by every VTube Studio request, response, event, and error.
/// Carries protocol metadata; the typed payload lives in <see cref="Data"/>.
/// </summary>
public sealed record VTubeStudioEnvelope
{
    [JsonPropertyName("apiName")]
    public string ApiName { get; init; } = VTubeStudioApi.ApiName;

    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; init; } = VTubeStudioApi.ApiVersion;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonPropertyName("messageType")]
    public required string MessageType { get; init; }

    [JsonPropertyName("requestID")]
    public string? RequestId { get; init; }

    /// <summary>Raw JSON payload - parsed against the appropriate typed record per <see cref="MessageType"/>.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}
