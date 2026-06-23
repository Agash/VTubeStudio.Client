using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of a <c>CurrentModelResponse</c>: details about the currently loaded model.</summary>
public sealed record CurrentModelResponse
{
    /// <summary>True when a model is currently loaded; when false the remaining fields are empty/default.</summary>
    [JsonPropertyName("modelLoaded")] public required bool ModelLoaded { get; init; }

    /// <summary>The model's display name.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The model's unique identifier.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The VTube Studio model config (<c>.vtube.json</c>) file name.</summary>
    [JsonPropertyName("vtsModelName")] public string? VtsModelName { get; init; }

    /// <summary>The model's icon file name (empty when the model has no icon).</summary>
    [JsonPropertyName("vtsModelIconName")] public string? VtsModelIconName { get; init; }

    /// <summary>The underlying Live2D model (<c>.model3.json</c>) file name.</summary>
    [JsonPropertyName("live2DModelName")] public string? Live2DModelName { get; init; }

    /// <summary>Time it took to load the model, in milliseconds.</summary>
    [JsonPropertyName("modelLoadTime")] public long ModelLoadTime { get; init; }

    /// <summary>Milliseconds elapsed since the model was loaded.</summary>
    [JsonPropertyName("timeSinceModelLoaded")] public long TimeSinceModelLoaded { get; init; }

    /// <summary>Number of Live2D parameters in the model.</summary>
    [JsonPropertyName("numberOfLive2DParameters")] public int NumberOfLive2DParameters { get; init; }

    /// <summary>Number of ArtMeshes in the model.</summary>
    [JsonPropertyName("numberOfLive2DArtmeshes")] public int NumberOfLive2DArtmeshes { get; init; }

    /// <summary>True when the model has a physics file.</summary>
    [JsonPropertyName("hasPhysicsFile")] public bool HasPhysicsFile { get; init; }

    /// <summary>Number of textures used by the model.</summary>
    [JsonPropertyName("numberOfTextures")] public int NumberOfTextures { get; init; }

    /// <summary>Texture resolution in pixels.</summary>
    [JsonPropertyName("textureResolution")] public int TextureResolution { get; init; }
}

/// <summary>Payload of an <c>AvailableModelsResponse</c>: every model available on the machine.</summary>
public sealed record AvailableModelsResponse
{
    /// <summary>Total number of available models.</summary>
    [JsonPropertyName("numberOfModels")] public int NumberOfModels { get; init; }

    /// <summary>The available models.</summary>
    [JsonPropertyName("availableModels")] public IReadOnlyList<AvailableModel> AvailableModels { get; init; } = [];
}

/// <summary>An entry in <see cref="AvailableModelsResponse.AvailableModels"/> describing one available model.</summary>
public sealed record AvailableModel
{
    /// <summary>True when this model is the one currently loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The model's display name.</summary>
    [JsonPropertyName("modelName")] public required string ModelName { get; init; }

    /// <summary>The model's unique identifier, used to load it via <see cref="ModelLoadRequest"/>.</summary>
    [JsonPropertyName("modelID")] public required string ModelId { get; init; }

    /// <summary>The VTube Studio model config file name.</summary>
    [JsonPropertyName("vtsModelName")] public string? VtsModelName { get; init; }

    /// <summary>The model's icon file name (empty when the model has no icon).</summary>
    [JsonPropertyName("vtsModelIconName")] public string? VtsModelIconName { get; init; }
}

/// <summary>Payload of a <c>ModelLoadRequest</c>: loads a model by id (empty id unloads the current model).</summary>
public sealed record ModelLoadRequest
{
    /// <summary>The id of the model to load; an empty string unloads the current model.</summary>
    [JsonPropertyName("modelID")] public required string ModelId { get; init; }
}

/// <summary>Payload of a <c>ModelLoadResponse</c>: confirms which model was loaded.</summary>
public sealed record ModelLoadResponse
{
    /// <summary>The id of the model that was loaded.</summary>
    [JsonPropertyName("modelID")] public required string ModelId { get; init; }
}

/// <summary>Payload of a <c>MoveModelRequest</c>: moves, rotates, and scales the loaded model.</summary>
public sealed record MoveModelRequest
{
    /// <summary>Duration of the move animation in seconds (0-2); 0 moves instantly.</summary>
    [JsonPropertyName("timeInSeconds")] public required double TimeInSeconds { get; init; }

    /// <summary>When true the supplied values are added relative to the model's current state instead of absolute.</summary>
    [JsonPropertyName("valuesAreRelativeToModel")] public bool ValuesAreRelativeToModel { get; init; }

    /// <summary>Target X position (-1000 to 1000); null leaves the X position unchanged.</summary>
    [JsonPropertyName("positionX")] public double? PositionX { get; init; }

    /// <summary>Target Y position (-1000 to 1000); null leaves the Y position unchanged.</summary>
    [JsonPropertyName("positionY")] public double? PositionY { get; init; }

    /// <summary>Target rotation in degrees (-360 to 360); null leaves the rotation unchanged.</summary>
    [JsonPropertyName("rotation")] public double? Rotation { get; init; }

    /// <summary>Target size (-100 to 100); null leaves the size unchanged.</summary>
    [JsonPropertyName("size")] public double? Size { get; init; }
}
