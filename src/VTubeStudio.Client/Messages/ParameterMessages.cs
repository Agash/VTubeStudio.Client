using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of an <c>InputParameterListResponse</c>: the available tracking input parameters.</summary>
public sealed record InputParameterListResponse
{
    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The model's display name.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The model's unique identifier.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>Parameters created by plugins (custom tracking parameters).</summary>
    [JsonPropertyName("customParameters")] public IReadOnlyList<ParameterInfo> CustomParameters { get; init; } = [];

    /// <summary>Built-in default tracking parameters provided by VTube Studio.</summary>
    [JsonPropertyName("defaultParameters")] public IReadOnlyList<ParameterInfo> DefaultParameters { get; init; } = [];
}

/// <summary>Payload of a <c>Live2DParameterListResponse</c>: the model's Live2D parameters and their values.</summary>
public sealed record Live2DParameterListResponse
{
    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The model's display name.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The model's unique identifier.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The Live2D parameters of the model.</summary>
    [JsonPropertyName("parameters")] public IReadOnlyList<ParameterInfo> Parameters { get; init; } = [];
}

/// <summary>Describes a single tracking or Live2D parameter: its name, owner, current value, and range.</summary>
public sealed record ParameterInfo
{
    /// <summary>The parameter name.</summary>
    [JsonPropertyName("name")] public required string Name { get; init; }

    /// <summary>Who created the parameter: a plugin name, or <c>"VTube Studio"</c> for default parameters.</summary>
    [JsonPropertyName("addedBy")] public string? AddedBy { get; init; }

    /// <summary>The parameter's current value.</summary>
    [JsonPropertyName("value")] public double Value { get; init; }

    /// <summary>The parameter's default minimum.</summary>
    [JsonPropertyName("min")] public double Min { get; init; }

    /// <summary>The parameter's default maximum.</summary>
    [JsonPropertyName("max")] public double Max { get; init; }

    /// <summary>The parameter's default value.</summary>
    [JsonPropertyName("defaultValue")] public double DefaultValue { get; init; }
}

/// <summary>Payload of a <c>ParameterValueRequest</c>: asks for the current value of a single parameter.</summary>
public sealed record ParameterValueRequest
{
    /// <summary>The name of the parameter to query.</summary>
    [JsonPropertyName("name")] public required string Name { get; init; }
}

/// <summary>Payload of an <c>InjectParameterDataRequest</c>: feeds tracking data into one or more parameters.</summary>
public sealed record InjectParameterDataRequest
{
    /// <summary>Overrides VTube Studio's face-found state for this injection (whether a face is considered detected).</summary>
    [JsonPropertyName("faceFound")] public bool FaceFound { get; init; }

    /// <summary>How to apply the values: <c>"set"</c> overrides the parameter, <c>"add"</c> accumulates onto it.</summary>
    [JsonPropertyName("mode")] public string Mode { get; init; } = "set"; // "set" or "add"

    /// <summary>The parameter values to inject.</summary>
    [JsonPropertyName("parameterValues")] public required IReadOnlyList<ParameterValue> ParameterValues { get; init; }
}

/// <summary>Payload of a <c>ParameterCreationRequest</c>: creates a custom tracking parameter.</summary>
public sealed record ParameterCreationRequest
{
    /// <summary>Parameter name: unique, alphanumeric, 4-32 characters.</summary>
    [JsonPropertyName("parameterName")] public required string ParameterName { get; init; }

    /// <summary>Short explanation shown in the parameter details; under 256 characters.</summary>
    [JsonPropertyName("explanation")] public string? Explanation { get; init; }

    /// <summary>Default lower value for new mappings (-1000000 to 1000000).</summary>
    [JsonPropertyName("min")] public double Min { get; init; }

    /// <summary>Default upper value for new mappings (-1000000 to 1000000).</summary>
    [JsonPropertyName("max")] public double Max { get; init; }

    /// <summary>Default value (-1000000 to 1000000).</summary>
    [JsonPropertyName("defaultValue")] public double DefaultValue { get; init; }
}

/// <summary>Payload of a <c>ParameterCreationResponse</c>: confirms the created parameter.</summary>
public sealed record ParameterCreationResponse
{
    /// <summary>The name of the created parameter.</summary>
    [JsonPropertyName("parameterName")] public required string ParameterName { get; init; }
}

/// <summary>Payload of a <c>ParameterDeletionRequest</c>: deletes a custom tracking parameter.</summary>
public sealed record ParameterDeletionRequest
{
    /// <summary>The name of the parameter to delete.</summary>
    [JsonPropertyName("parameterName")] public required string ParameterName { get; init; }
}

/// <summary>Payload of a <c>ParameterDeletionResponse</c>: confirms the deleted parameter.</summary>
public sealed record ParameterDeletionResponse
{
    /// <summary>The name of the deleted parameter.</summary>
    [JsonPropertyName("parameterName")] public required string ParameterName { get; init; }
}

/// <summary>A single parameter update injected via <see cref="InjectParameterDataRequest"/>.</summary>
public sealed record ParameterValue
{
    /// <summary>The name of the parameter to set.</summary>
    [JsonPropertyName("id")] public required string Id { get; init; }

    /// <summary>The value to inject (-1000000 to 1000000).</summary>
    [JsonPropertyName("value")] public required double Value { get; init; }

    /// <summary>Optional mix ratio (0-1, default 1) controlling how strongly the value is applied.</summary>
    [JsonPropertyName("weight")] public double? Weight { get; init; }
}
