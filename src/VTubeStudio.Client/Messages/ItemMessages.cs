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

    /// <summary>False when menus or dialogs prevent loading items right now.</summary>
    [JsonPropertyName("canLoadItemsRightNow")] public bool CanLoadItemsRightNow { get; init; }

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

    /// <summary>Frame count (animated items only).</summary>
    [JsonPropertyName("frameCount")] public int FrameCount { get; init; }

    /// <summary>Current frame (animated items only).</summary>
    [JsonPropertyName("currentFrame")] public int CurrentFrame { get; init; }

    /// <summary>True when the item is pinned to the model.</summary>
    [JsonPropertyName("pinnedToModel")] public bool PinnedToModel { get; init; }

    /// <summary>The id of the model the item is pinned to.</summary>
    [JsonPropertyName("pinnedModelID")] public string? PinnedModelId { get; init; }

    /// <summary>The id of the ArtMesh the item is pinned to.</summary>
    [JsonPropertyName("pinnedArtMeshID")] public string? PinnedArtMeshId { get; init; }

    /// <summary>The group the item belongs to.</summary>
    [JsonPropertyName("groupName")] public string? GroupName { get; init; }

    /// <summary>The scene the item belongs to.</summary>
    [JsonPropertyName("sceneName")] public string? SceneName { get; init; }

    /// <summary>True when the item comes from the Steam workshop.</summary>
    [JsonPropertyName("fromWorkshop")] public bool FromWorkshop { get; init; }
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

    /// <summary>Base64-encoded PNG, JPG or GIF data to load as an item; null loads from file. Requires permission.</summary>
    [JsonPropertyName("customDataBase64")] public string? CustomDataBase64 { get; init; }

    /// <summary>When true, VTube Studio asks the user before loading custom data.</summary>
    [JsonPropertyName("customDataAskUserFirst")] public bool? CustomDataAskUserFirst { get; init; }

    /// <summary>When false, the user prompt shows even for whitelisted custom data.</summary>
    [JsonPropertyName("customDataSkipAskingUserIfWhitelisted")] public bool? CustomDataSkipAskingUserIfWhitelisted { get; init; }

    /// <summary>Seconds the custom-data prompt stays open; 0 or less shows it until decided.</summary>
    [JsonPropertyName("customDataAskTimer")] public double? CustomDataAskTimer { get; init; }
}

/// <summary>Payload of an <c>ItemLoadResponse</c>: confirms the loaded item's instance id.</summary>
public sealed record ItemLoadResponse
{
    /// <summary>The instance id of the newly loaded item.</summary>
    [JsonPropertyName("instanceID")] public required string InstanceId { get; init; }

    /// <summary>The file name of the loaded item; generated by VTube Studio for custom data.</summary>
    [JsonPropertyName("fileName")] public string? FileName { get; init; }
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
    [JsonPropertyName("unloadedItems")] public IReadOnlyList<UnloadedItem> UnloadedItems { get; init; } = [];
}

/// <summary>One entry in <see cref="ItemUnloadResponse.UnloadedItems"/>.</summary>
public sealed record UnloadedItem
{
    /// <summary>The instance id of the unloaded item.</summary>
    [JsonPropertyName("instanceID")] public string? InstanceId { get; init; }

    /// <summary>The file name of the unloaded item.</summary>
    [JsonPropertyName("fileName")] public string? FileName { get; init; }
}

/// <summary>Payload of an <c>ItemAnimationControlRequest</c>: controls item playback and appearance.</summary>
public sealed record ItemAnimationControlRequest
{
    /// <summary>The instance id of the item to control.</summary>
    [JsonPropertyName("itemInstanceID")] public required string ItemInstanceId { get; init; }

    /// <summary>Animation framerate; -1 or omitted leaves it unchanged.</summary>
    [JsonPropertyName("framerate")] public double? Framerate { get; init; }

    /// <summary>Frame to jump to; -1 or omitted leaves it unchanged.</summary>
    [JsonPropertyName("frame")] public int? Frame { get; init; }

    /// <summary>Brightness multiplier; -1 or omitted leaves it unchanged.</summary>
    [JsonPropertyName("brightness")] public double? Brightness { get; init; }

    /// <summary>Opacity; -1 or omitted leaves it unchanged.</summary>
    [JsonPropertyName("opacity")] public double? Opacity { get; init; }

    /// <summary>When true, <see cref="AutoStopFrames"/> replaces the auto-stop frames.</summary>
    [JsonPropertyName("setAutoStopFrames")] public bool SetAutoStopFrames { get; init; }

    /// <summary>Frame indices the animation stops on.</summary>
    [JsonPropertyName("autoStopFrames")] public IReadOnlyList<int> AutoStopFrames { get; init; } = [];

    /// <summary>When true, <see cref="AnimationPlayState"/> is applied.</summary>
    [JsonPropertyName("setAnimationPlayState")] public bool SetAnimationPlayState { get; init; }

    /// <summary>True to play, false to stop the animation.</summary>
    [JsonPropertyName("animationPlayState")] public bool AnimationPlayState { get; init; }
}

/// <summary>Payload of an <c>ItemAnimationControlResponse</c>: the item animation state.</summary>
public sealed record ItemAnimationControlResponse
{
    /// <summary>Current frame index.</summary>
    [JsonPropertyName("frame")] public int Frame { get; init; }

    /// <summary>True when the animation is currently playing.</summary>
    [JsonPropertyName("animationPlaying")] public bool AnimationPlaying { get; init; }
}

/// <summary>Payload of an <c>ItemMoveRequest</c>: moves items in the scene.</summary>
public sealed record ItemMoveRequest
{
    /// <summary>The items to move; at most 64 entries are used.</summary>
    [JsonPropertyName("itemsToMove")] public required IReadOnlyList<ItemMoveInstruction> ItemsToMove { get; init; }
}

/// <summary>One entry in <see cref="ItemMoveRequest.ItemsToMove"/>.</summary>
public sealed record ItemMoveInstruction
{
    /// <summary>The instance id of the item to move.</summary>
    [JsonPropertyName("itemInstanceID")] public required string ItemInstanceId { get; init; }

    /// <summary>Fade duration in seconds (0-30); 0 moves instantly.</summary>
    [JsonPropertyName("timeInSeconds")] public double TimeInSeconds { get; init; }

    /// <summary>Fade mode: <c>linear</c>, <c>easeIn</c>, <c>easeOut</c>, <c>easeBoth</c>, <c>overshoot</c> or <c>zip</c>.</summary>
    [JsonPropertyName("fadeMode")] public string? FadeMode { get; init; }

    /// <summary>Target X position; -1000 or lower leaves it unchanged.</summary>
    [JsonPropertyName("positionX")] public double? PositionX { get; init; }

    /// <summary>Target Y position; -1000 or lower leaves it unchanged.</summary>
    [JsonPropertyName("positionY")] public double? PositionY { get; init; }

    /// <summary>Target size; -1000 or lower leaves it unchanged.</summary>
    [JsonPropertyName("size")] public double? Size { get; init; }

    /// <summary>Target rotation; -1000 or lower leaves it unchanged.</summary>
    [JsonPropertyName("rotation")] public double? Rotation { get; init; }

    /// <summary>Target order; -1000 or lower leaves it unchanged.</summary>
    [JsonPropertyName("order")] public int? Order { get; init; }

    /// <summary>When true, <see cref="Flip"/> is applied.</summary>
    [JsonPropertyName("setFlip")] public bool SetFlip { get; init; }

    /// <summary>Flip state to set.</summary>
    [JsonPropertyName("flip")] public bool Flip { get; init; }

    /// <summary>When true, the user can stop the movement by interacting with the item.</summary>
    [JsonPropertyName("userCanStop")] public bool UserCanStop { get; init; }
}

/// <summary>Payload of an <c>ItemMoveResponse</c>: per-item move results.</summary>
public sealed record ItemMoveResponse
{
    /// <summary>One result per requested item.</summary>
    [JsonPropertyName("movedItems")] public IReadOnlyList<ItemMoveResult> MovedItems { get; init; } = [];
}

/// <summary>One entry in <see cref="ItemMoveResponse.MovedItems"/>.</summary>
public sealed record ItemMoveResult
{
    /// <summary>The instance id of the moved item.</summary>
    [JsonPropertyName("itemInstanceID")] public string? ItemInstanceId { get; init; }

    /// <summary>True when the move succeeded.</summary>
    [JsonPropertyName("success")] public bool Success { get; init; }

    /// <summary>Error id on failure; -1 on success.</summary>
    [JsonPropertyName("errorID")] public int ErrorId { get; init; }
}

/// <summary>Payload of an <c>ItemSortRequest</c>: sorts an item between model layers.</summary>
public sealed record ItemSortRequest
{
    /// <summary>The instance id of the item to sort.</summary>
    [JsonPropertyName("itemInstanceID")] public required string ItemInstanceId { get; init; }

    /// <summary>True to insert the item into the model.</summary>
    [JsonPropertyName("frontOn")] public bool FrontOn { get; init; }

    /// <summary>True to also insert the back part of a Live2D item.</summary>
    [JsonPropertyName("backOn")] public bool BackOn { get; init; }

    /// <summary>How <see cref="SplitAt"/> is interpreted: <c>Unchanged</c>, <c>UseArtMeshID</c>.</summary>
    [JsonPropertyName("setSplitPoint")] public string? SetSplitPoint { get; init; }

    /// <summary>How <see cref="WithinModelOrderFront"/> is interpreted: <c>Unchanged</c>, <c>UseArtMeshID</c>, <c>UseSpecialID</c>.</summary>
    [JsonPropertyName("setFrontOrder")] public string? SetFrontOrder { get; init; }

    /// <summary>How <see cref="WithinModelOrderBack"/> is interpreted: <c>Unchanged</c>, <c>UseArtMeshID</c>, <c>UseSpecialID</c>.</summary>
    [JsonPropertyName("setBackOrder")] public string? SetBackOrder { get; init; }

    /// <summary>Split point for Live2D items.</summary>
    [JsonPropertyName("splitAt")] public string? SplitAt { get; init; }

    /// <summary>Front insertion point: ArtMesh id, <c>FullyInFront</c> or <c>FullyInBack</c>.</summary>
    [JsonPropertyName("withinModelOrderFront")] public string? WithinModelOrderFront { get; init; }

    /// <summary>Back insertion point: ArtMesh id or <c>FullyInBack</c>.</summary>
    [JsonPropertyName("withinModelOrderBack")] public string? WithinModelOrderBack { get; init; }
}

/// <summary>Payload of an <c>ItemSortResponse</c>: the applied within-model sorting.</summary>
public sealed record ItemSortResponse
{
    /// <summary>The instance id of the sorted item.</summary>
    [JsonPropertyName("itemInstanceID")] public string? ItemInstanceId { get; init; }

    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The id of the loaded model.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The name of the loaded model.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>True when the requested front layer was found in the loaded model.</summary>
    [JsonPropertyName("loadedModelHadRequestedFrontLayer")] public bool LoadedModelHadRequestedFrontLayer { get; init; }

    /// <summary>True when the requested back layer was found in the loaded model.</summary>
    [JsonPropertyName("loadedModelHadRequestedBackLayer")] public bool LoadedModelHadRequestedBackLayer { get; init; }
}

/// <summary>Payload of an <c>ItemPinRequest</c>: pins an item to the model.</summary>
public sealed record ItemPinRequest
{
    /// <summary>False unpins the item; no other fields are needed then.</summary>
    [JsonPropertyName("pin")] public bool Pin { get; init; }

    /// <summary>The instance id of the item to pin.</summary>
    [JsonPropertyName("itemInstanceID")] public required string ItemInstanceId { get; init; }

    /// <summary>How the angle is interpreted: <c>RelativeToWorld</c>, <c>RelativeToCurrentItemRotation</c>, <c>RelativeToModel</c>, <c>RelativeToPinPosition</c>.</summary>
    [JsonPropertyName("angleRelativeTo")] public string? AngleRelativeTo { get; init; }

    /// <summary>How the size is interpreted: <c>RelativeToWorld</c>, <c>RelativeToCurrentItemSize</c>.</summary>
    [JsonPropertyName("sizeRelativeTo")] public string? SizeRelativeTo { get; init; }

    /// <summary>How the pin position is interpreted: <c>Provided</c>, <c>Center</c>, <c>Random</c>.</summary>
    [JsonPropertyName("vertexPinType")] public string? VertexPinType { get; init; }

    /// <summary>The pin position.</summary>
    [JsonPropertyName("pinInfo")] public ItemPinInfo? PinInfo { get; init; }
}

/// <summary>Pin position of an <see cref="ItemPinRequest"/>.</summary>
public sealed record ItemPinInfo
{
    /// <summary>The id of the model to pin to; empty pins to the loaded model.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The id of the ArtMesh to pin to; empty picks a random one.</summary>
    [JsonPropertyName("artMeshID")] public string? ArtMeshId { get; init; }

    /// <summary>Pin angle.</summary>
    [JsonPropertyName("angle")] public double Angle { get; init; }

    /// <summary>Pin size.</summary>
    [JsonPropertyName("size")] public double Size { get; init; }

    /// <summary>First vertex id of the pin triangle.</summary>
    [JsonPropertyName("vertexID1")] public int VertexId1 { get; init; }

    /// <summary>Second vertex id of the pin triangle.</summary>
    [JsonPropertyName("vertexID2")] public int VertexId2 { get; init; }

    /// <summary>Third vertex id of the pin triangle.</summary>
    [JsonPropertyName("vertexID3")] public int VertexId3 { get; init; }

    /// <summary>Barycentric weight for the first vertex.</summary>
    [JsonPropertyName("vertexWeight1")] public double VertexWeight1 { get; init; }

    /// <summary>Barycentric weight for the second vertex.</summary>
    [JsonPropertyName("vertexWeight2")] public double VertexWeight2 { get; init; }

    /// <summary>Barycentric weight for the third vertex.</summary>
    [JsonPropertyName("vertexWeight3")] public double VertexWeight3 { get; init; }
}

/// <summary>Payload of an <c>ItemPinResponse</c>: the pin state of the item.</summary>
public sealed record ItemPinResponse
{
    /// <summary>True when the item is now pinned.</summary>
    [JsonPropertyName("isPinned")] public bool IsPinned { get; init; }

    /// <summary>The instance id of the pinned item.</summary>
    [JsonPropertyName("itemInstanceID")] public string? ItemInstanceId { get; init; }

    /// <summary>The file name of the pinned item.</summary>
    [JsonPropertyName("itemFileName")] public string? ItemFileName { get; init; }
}
