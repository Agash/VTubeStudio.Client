using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record ExpressionStateRequest
{
    /// <summary>If true the response includes the expression parameter details (more verbose).</summary>
    [JsonPropertyName("details")] public bool Details { get; init; }
    /// <summary>Optional — fetch a single expression by file name.</summary>
    [JsonPropertyName("expressionFile")] public string? ExpressionFile { get; init; }
}

public sealed record ExpressionStateResponse
{
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("expressions")] public IReadOnlyList<ExpressionInfo> Expressions { get; init; } = [];
}

public sealed record ExpressionInfo
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("file")] public required string File { get; init; }
    [JsonPropertyName("active")] public bool Active { get; init; }
    [JsonPropertyName("deactivateWhenKeyIsLetGo")] public bool DeactivateWhenKeyIsLetGo { get; init; }
    [JsonPropertyName("autoDeactivateAfterSeconds")] public bool AutoDeactivateAfterSeconds { get; init; }
    [JsonPropertyName("secondsRemaining")] public double SecondsRemaining { get; init; }
}

public sealed record ExpressionActivationRequest
{
    [JsonPropertyName("expressionFile")] public required string ExpressionFile { get; init; }
    [JsonPropertyName("active")] public required bool Active { get; init; }
}
