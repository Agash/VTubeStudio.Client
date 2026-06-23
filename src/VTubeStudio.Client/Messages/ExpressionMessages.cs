using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of an <c>ExpressionStateRequest</c>: asks for the activation state of the model's expressions.</summary>
public sealed record ExpressionStateRequest
{
    /// <summary>If true the response includes the expression parameter details (more verbose).</summary>
    [JsonPropertyName("details")] public bool Details { get; init; }

    /// <summary>Optional - fetch a single expression by file name.</summary>
    [JsonPropertyName("expressionFile")] public string? ExpressionFile { get; init; }
}

/// <summary>Payload of an <c>ExpressionStateResponse</c>: the activation state of each expression.</summary>
public sealed record ExpressionStateResponse
{
    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The model's display name.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The model's unique identifier.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>State of each expression in the model.</summary>
    [JsonPropertyName("expressions")] public IReadOnlyList<ExpressionInfo> Expressions { get; init; } = [];
}

/// <summary>An entry in <see cref="ExpressionStateResponse.Expressions"/> describing one expression.</summary>
public sealed record ExpressionInfo
{
    /// <summary>The expression name (file name without extension).</summary>
    [JsonPropertyName("name")] public required string Name { get; init; }

    /// <summary>The expression file name.</summary>
    [JsonPropertyName("file")] public required string File { get; init; }

    /// <summary>True when the expression is currently active.</summary>
    [JsonPropertyName("active")] public bool Active { get; init; }

    /// <summary>True when the expression's hotkey deactivates it on key release.</summary>
    [JsonPropertyName("deactivateWhenKeyIsLetGo")] public bool DeactivateWhenKeyIsLetGo { get; init; }

    /// <summary>True when the expression is configured to auto-deactivate after a number of seconds.</summary>
    [JsonPropertyName("autoDeactivateAfterSeconds")] public bool AutoDeactivateAfterSeconds { get; init; }

    /// <summary>Seconds remaining until the expression auto-deactivates (when auto-deactivate is enabled).</summary>
    [JsonPropertyName("secondsRemaining")] public double SecondsRemaining { get; init; }
}

/// <summary>Payload of an <c>ExpressionActivationRequest</c>: activates or deactivates an expression.</summary>
public sealed record ExpressionActivationRequest
{
    /// <summary>The expression file to activate or deactivate.</summary>
    [JsonPropertyName("expressionFile")] public required string ExpressionFile { get; init; }

    /// <summary>True to activate the expression, false to deactivate it.</summary>
    [JsonPropertyName("active")] public required bool Active { get; init; }
}
