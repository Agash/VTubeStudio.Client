using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record ItemListRequest
{
    [JsonPropertyName("includeAvailableSpots")] public bool IncludeAvailableSpots { get; init; }
    [JsonPropertyName("includeItemInstancesInScene")] public bool IncludeItemInstancesInScene { get; init; } = true;
    [JsonPropertyName("includeAvailableItemFiles")] public bool IncludeAvailableItemFiles { get; init; }
    [JsonPropertyName("onlyItemsWithFileName")] public string? OnlyItemsWithFileName { get; init; }
    [JsonPropertyName("onlyItemsWithInstanceID")] public string? OnlyItemsWithInstanceId { get; init; }
}

public sealed record ItemListResponse
{
    [JsonPropertyName("itemsInSceneCount")] public int ItemsInSceneCount { get; init; }
    [JsonPropertyName("totalItemsAllowedCount")] public int TotalItemsAllowedCount { get; init; }
    [JsonPropertyName("availableSpots")] public IReadOnlyList<int> AvailableSpots { get; init; } = [];
    [JsonPropertyName("itemInstancesInScene")] public IReadOnlyList<ItemInstance> ItemInstancesInScene { get; init; } = [];
    [JsonPropertyName("availableItemFiles")] public IReadOnlyList<AvailableItemFile> AvailableItemFiles { get; init; } = [];
}

public sealed record ItemInstance
{
    [JsonPropertyName("fileName")] public required string FileName { get; init; }
    [JsonPropertyName("instanceID")] public required string InstanceId { get; init; }
    [JsonPropertyName("order")] public int Order { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("censored")] public bool Censored { get; init; }
    [JsonPropertyName("flipped")] public bool Flipped { get; init; }
    [JsonPropertyName("locked")] public bool Locked { get; init; }
    [JsonPropertyName("smoothing")] public double Smoothing { get; init; }
    [JsonPropertyName("framerate")] public double Framerate { get; init; }
}

public sealed record AvailableItemFile
{
    [JsonPropertyName("fileName")] public required string FileName { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("loadedCount")] public int LoadedCount { get; init; }
}

public sealed record ItemLoadRequest
{
    [JsonPropertyName("fileName")] public required string FileName { get; init; }
    [JsonPropertyName("positionX")] public double? PositionX { get; init; }
    [JsonPropertyName("positionY")] public double? PositionY { get; init; }
    [JsonPropertyName("size")] public double? Size { get; init; }
    [JsonPropertyName("rotation")] public double? Rotation { get; init; }
    [JsonPropertyName("fadeTime")] public double FadeTime { get; init; }
    [JsonPropertyName("order")] public int? Order { get; init; }
    [JsonPropertyName("failIfOrderTaken")] public bool FailIfOrderTaken { get; init; }
    [JsonPropertyName("smoothing")] public double Smoothing { get; init; }
    [JsonPropertyName("censored")] public bool Censored { get; init; }
    [JsonPropertyName("flipped")] public bool Flipped { get; init; }
    [JsonPropertyName("locked")] public bool Locked { get; init; }
    [JsonPropertyName("unloadWhenPluginDisconnects")] public bool UnloadWhenPluginDisconnects { get; init; }
}

public sealed record ItemLoadResponse
{
    [JsonPropertyName("instanceID")] public required string InstanceId { get; init; }
    [JsonPropertyName("fileName")] public required string FileName { get; init; }
}

public sealed record ItemUnloadRequest
{
    [JsonPropertyName("unloadAllInScene")] public bool UnloadAllInScene { get; init; }
    [JsonPropertyName("unloadAllLoadedByThisPlugin")] public bool UnloadAllLoadedByThisPlugin { get; init; }
    [JsonPropertyName("allowUnloadingItemsLoadedByUserOrOtherPlugins")] public bool AllowUnloadingItemsLoadedByUserOrOtherPlugins { get; init; }
    [JsonPropertyName("instanceIDs")] public IReadOnlyList<string> InstanceIds { get; init; } = [];
    [JsonPropertyName("fileNames")] public IReadOnlyList<string> FileNames { get; init; } = [];
}

public sealed record ItemUnloadResponse
{
    [JsonPropertyName("unloadedItems")] public IReadOnlyList<ItemInstance> UnloadedItems { get; init; } = [];
}
