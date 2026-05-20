using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record InputParameterListResponse
{
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("customParameters")] public IReadOnlyList<ParameterInfo> CustomParameters { get; init; } = [];
    [JsonPropertyName("defaultParameters")] public IReadOnlyList<ParameterInfo> DefaultParameters { get; init; } = [];
}

public sealed record Live2DParameterListResponse
{
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("parameters")] public IReadOnlyList<ParameterInfo> Parameters { get; init; } = [];
}

public sealed record ParameterInfo
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("addedBy")] public string? AddedBy { get; init; }
    [JsonPropertyName("value")] public double Value { get; init; }
    [JsonPropertyName("min")] public double Min { get; init; }
    [JsonPropertyName("max")] public double Max { get; init; }
    [JsonPropertyName("defaultValue")] public double DefaultValue { get; init; }
}

public sealed record ParameterValueRequest
{
    [JsonPropertyName("name")] public required string Name { get; init; }
}

public sealed record InjectParameterDataRequest
{
    /// <summary>Optional namespace prefix to disambiguate parameters from multiple plugins.</summary>
    [JsonPropertyName("faceFound")] public bool FaceFound { get; init; }
    [JsonPropertyName("mode")] public string Mode { get; init; } = "set"; // "set" or "add"
    [JsonPropertyName("parameterValues")] public required IReadOnlyList<ParameterValue> ParameterValues { get; init; }
}

public sealed record ParameterValue
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("value")] public required double Value { get; init; }
    [JsonPropertyName("weight")] public double? Weight { get; init; }
}
