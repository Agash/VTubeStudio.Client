using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record CurrentModelResponse
{
    [JsonPropertyName("modelLoaded")] public required bool ModelLoaded { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("vtsModelName")] public string? VtsModelName { get; init; }
    [JsonPropertyName("vtsModelIconName")] public string? VtsModelIconName { get; init; }
    [JsonPropertyName("live2DModelName")] public string? Live2DModelName { get; init; }
    [JsonPropertyName("modelLoadTime")] public long ModelLoadTime { get; init; }
    [JsonPropertyName("timeSinceModelLoaded")] public long TimeSinceModelLoaded { get; init; }
    [JsonPropertyName("numberOfLive2DParameters")] public int NumberOfLive2DParameters { get; init; }
    [JsonPropertyName("numberOfLive2DArtmeshes")] public int NumberOfLive2DArtmeshes { get; init; }
    [JsonPropertyName("hasPhysicsFile")] public bool HasPhysicsFile { get; init; }
    [JsonPropertyName("numberOfTextures")] public int NumberOfTextures { get; init; }
    [JsonPropertyName("textureResolution")] public int TextureResolution { get; init; }
}

public sealed record AvailableModelsResponse
{
    [JsonPropertyName("numberOfModels")] public int NumberOfModels { get; init; }
    [JsonPropertyName("availableModels")] public IReadOnlyList<AvailableModel> AvailableModels { get; init; } = [];
}

public sealed record AvailableModel
{
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("modelName")] public required string ModelName { get; init; }
    [JsonPropertyName("modelID")] public required string ModelId { get; init; }
    [JsonPropertyName("vtsModelName")] public string? VtsModelName { get; init; }
    [JsonPropertyName("vtsModelIconName")] public string? VtsModelIconName { get; init; }
}

public sealed record ModelLoadRequest
{
    [JsonPropertyName("modelID")] public required string ModelId { get; init; }
}

public sealed record ModelLoadResponse
{
    [JsonPropertyName("modelID")] public required string ModelId { get; init; }
}

public sealed record MoveModelRequest
{
    [JsonPropertyName("timeInSeconds")] public required double TimeInSeconds { get; init; }
    [JsonPropertyName("valuesAreRelativeToModel")] public bool ValuesAreRelativeToModel { get; init; }
    [JsonPropertyName("positionX")] public double? PositionX { get; init; }
    [JsonPropertyName("positionY")] public double? PositionY { get; init; }
    [JsonPropertyName("rotation")] public double? Rotation { get; init; }
    [JsonPropertyName("size")] public double? Size { get; init; }
}
