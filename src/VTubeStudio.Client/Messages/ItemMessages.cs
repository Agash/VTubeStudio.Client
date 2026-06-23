using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of an <c>ItemListRequest</c>: asks for available item files and/or loaded item instances.</summary>
public sealed record ItemListRequest
{
    /// <summary>When true, the response includes the open ordering positions in <c>availableSpots</c>.</summary>
    [JsonPropertyName("includeAvailableSpots")] public bool IncludeAvailableSpots { get; init; }

    /// <summary>When true (default), the response includes the items currently loaded in the scene.</summary>
    [JsonPropertyName("includeItemInstancesInScene")] public bool IncludeItemInstancesInScene { get; init; } = true;

    /// <summary>When true, the response includes the item files available on disk.</summary>
    [JsonPropertyName("includeAvailableItemFiles")] public bool IncludeAvailableItemFiles { get; init; }

    /// <summary>Optional - restrict the results to items with this file name.</summary>
    [JsonPropertyName("onlyItemsWithFileName")] public string? OnlyItemsWithFileName { get; init; }

    /// <summary>Optional - restrict the results to the item with this instance id.</summary>
    [JsonPropertyName("onlyItemsWithInstanceID")] public string? OnlyItemsWithInstanceId { get; init; }
}

/// <summary>Payload of an <c>ItemListResponse</c>: items in the scene and/or available item files.</summary>
public sealed record ItemListResponse
{
    /// <summary>Number of items currently loaded in the scene.</summary>
    [JsonPropertyName("itemsInSceneCount")] public int ItemsInSceneCount { get; init; }

    /// <summary>Maximum number of items that may be loaded at once.</summary>
    [JsonPropertyName("totalItemsAllowedCount")] public int TotalItemsAllowedCount { get; init; }

    /// <summary>Open ordering positions (requested via <see cref="ItemListRequest.IncludeAvailableSpots"/>).</summary>
    [JsonPropertyName("availableSpots")] public IReadOnlyList<int> AvailableSpots { get; init; } = [];

    /// <summary>The items currently loaded in the scene.</summary>
    [JsonPropertyName("itemInstancesInScene")] public IReadOnlyList<ItemInstance> ItemInstancesInScene { get; init; } = [];

    /// <summary>The item files available on disk.</summary>
    [JsonPropertyName("availableItemFiles")] public IReadOnlyList<AvailableItemFile> AvailableItemFiles { get; init; } = [];
}

/// <summary>Describes one item instance currently loaded in the scene.</summary>
public sealed record ItemInstance
{
    /// <summary>The item's file name.</summary>
    [JsonPropertyName("fileName")] public required string FileName { get; init; }

    /// <summary>The item's unique instance id.</summary>
    [JsonPropertyName("instanceID")] public required string InstanceId { get; init; }

    /// <summary>The item's sorting order (layer position).</summary>
    [JsonPropertyName("order")] public int Order { get; init; }

    /// <summary>The item type (for example <c>PNG</c>, <c>JPG</c>, <c>GIF</c>, <c>AnimationFolder</c>, <c>Live2D</c>).</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }

    /// <summary>True when the item is censored.</summary>
    [JsonPropertyName("censored")] public bool Censored { get; init; }

    /// <summary>True when the item is horizontally flipped.</summary>
    [JsonPropertyName("flipped")] public bool Flipped { get; init; }

    /// <summary>True when the item is locked against user interaction.</summary>
    [JsonPropertyName("locked")] public bool Locked { get; init; }

    /// <summary>Movement smoothing factor (0-1).</summary>
    [JsonPropertyName("smoothing")] public double Smoothing { get; init; }

    /// <summary>Animation framerate in frames per second (for animated items).</summary>
    [JsonPropertyName("framerate")] public double Framerate { get; init; }
}

/// <summary>Describes an item file available on disk, with how many instances of it are loaded.</summary>
public sealed record AvailableItemFile
{
    /// <summary>The item's file name.</summary>
    [JsonPropertyName("fileName")] public required string FileName { get; init; }

    /// <summary>The item type (for example <c>PNG</c>, <c>JPG</c>, <c>GIF</c>, <c>AnimationFolder</c>, <c>Live2D</c>).</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }

    /// <summary>Number of instances of this file currently loaded in the scene.</summary>
    [JsonPropertyName("loadedCount")] public int LoadedCount { get; init; }
}

/// <summary>Payload of an <c>ItemLoadRequest</c>: loads an item into the scene.</summary>
public sealed record ItemLoadRequest
{
    /// <summary>The file name of the item to load.</summary>
    [JsonPropertyName("fileName")] public required string FileName { get; init; }

    /// <summary>X position (-1000 to 1000); null uses the default.</summary>
    [JsonPropertyName("positionX")] public double? PositionX { get; init; }

    /// <summary>Y position (-1000 to 1000); null uses the default.</summary>
    [JsonPropertyName("positionY")] public double? PositionY { get; init; }

    /// <summary>Size/scale (0-1); null uses the default.</summary>
    [JsonPropertyName("size")] public double? Size { get; init; }

    /// <summary>Rotation in degrees; null uses the default.</summary>
    [JsonPropertyName("rotation")] public double? Rotation { get; init; }

    /// <summary>Fade-in duration in seconds (0-2).</summary>
    [JsonPropertyName("fadeTime")] public double FadeTime { get; init; }

    /// <summary>Desired sorting order (layer position); null lets VTube Studio choose.</summary>
    [JsonPropertyName("order")] public int? Order { get; init; }

    /// <summary>When true, the request fails if the requested order position is already occupied.</summary>
    [JsonPropertyName("failIfOrderTaken")] public bool FailIfOrderTaken { get; init; }

    /// <summary>Movement smoothing factor (0-1).</summary>
    [JsonPropertyName("smoothing")] public double Smoothing { get; init; }

    /// <summary>When true, the item loads censored.</summary>
    [JsonPropertyName("censored")] public bool Censored { get; init; }

    /// <summary>When true, the item loads horizontally flipped.</summary>
    [JsonPropertyName("flipped")] public bool Flipped { get; init; }

    /// <summary>When true, the item loads locked against user interaction.</summary>
    [JsonPropertyName("locked")] public bool Locked { get; init; }

    /// <summary>When true, the item is automatically unloaded if this plugin disconnects.</summary>
    [JsonPropertyName("unloadWhenPluginDisconnects")] public bool UnloadWhenPluginDisconnects { get; init; }
}

/// <summary>Payload of an <c>ItemLoadResponse</c>: confirms the loaded item's instance id.</summary>
public sealed record ItemLoadResponse
{
    /// <summary>The instance id of the newly loaded item.</summary>
    [JsonPropertyName("instanceID")] public required string InstanceId { get; init; }

    /// <summary>The file name of the loaded item.</summary>
    [JsonPropertyName("fileName")] public required string FileName { get; init; }
}

/// <summary>Payload of an <c>ItemUnloadRequest</c>: unloads items from the scene by various criteria.</summary>
public sealed record ItemUnloadRequest
{
    /// <summary>When true, unloads every item in the scene.</summary>
    [JsonPropertyName("unloadAllInScene")] public bool UnloadAllInScene { get; init; }

    /// <summary>When true, unloads all items that were loaded by this plugin.</summary>
    [JsonPropertyName("unloadAllLoadedByThisPlugin")] public bool UnloadAllLoadedByThisPlugin { get; init; }

    /// <summary>When true, also allows unloading items loaded by the user or other plugins.</summary>
    [JsonPropertyName("allowUnloadingItemsLoadedByUserOrOtherPlugins")] public bool AllowUnloadingItemsLoadedByUserOrOtherPlugins { get; init; }

    /// <summary>Specific item instance ids to unload.</summary>
    [JsonPropertyName("instanceIDs")] public IReadOnlyList<string> InstanceIds { get; init; } = [];

    /// <summary>File names whose every loaded instance should be unloaded.</summary>
    [JsonPropertyName("fileNames")] public IReadOnlyList<string> FileNames { get; init; } = [];
}

/// <summary>Payload of an <c>ItemUnloadResponse</c>: the items that were unloaded.</summary>
public sealed record ItemUnloadResponse
{
    /// <summary>The items that were unloaded by the request.</summary>
    [JsonPropertyName("unloadedItems")] public IReadOnlyList<ItemInstance> UnloadedItems { get; init; } = [];
}
