using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Events;

/// <summary>Payload of a <c>ModelLoadedEvent</c>: fired when a model is loaded or unloaded.</summary>
public sealed record ModelLoadedEventPayload : IVTubeStudioEvent<ModelLoadedEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.ModelLoaded;

    /// <inheritdoc/>
    public static JsonTypeInfo<ModelLoadedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelLoadedEventPayload;

    /// <summary>True when a model was loaded; false when the current model was unloaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The display name of the loaded model.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The unique id of the loaded model.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
}

/// <summary>Payload of a <c>TrackingStatusChangedEvent</c>: fired when face or hand tracking is gained or lost.</summary>
public sealed record TrackingStatusChangedEventPayload : IVTubeStudioEvent<TrackingStatusChangedEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.TrackingStatusChanged;

    /// <inheritdoc/>
    public static JsonTypeInfo<TrackingStatusChangedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.TrackingStatusChangedEventPayload;

    /// <summary>True when a face is currently being tracked.</summary>
    [JsonPropertyName("faceFound")] public bool FaceFound { get; init; }

    /// <summary>True when the left hand is currently being tracked.</summary>
    [JsonPropertyName("leftHandFound")] public bool LeftHandFound { get; init; }

    /// <summary>True when the right hand is currently being tracked.</summary>
    [JsonPropertyName("rightHandFound")] public bool RightHandFound { get; init; }
}

/// <summary>Payload of a <c>BackgroundChangedEvent</c>: fired when the background changes.</summary>
public sealed record BackgroundChangedEventPayload : IVTubeStudioEvent<BackgroundChangedEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.BackgroundChanged;

    /// <inheritdoc/>
    public static JsonTypeInfo<BackgroundChangedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.BackgroundChangedEventPayload;

    /// <summary>The name of the newly selected background.</summary>
    [JsonPropertyName("backgroundName")] public required string BackgroundName { get; init; }
}

/// <summary>Payload of a <c>ModelConfigChangedEvent</c>: fired when the model configuration changes.</summary>
public sealed record ModelConfigChangedEventPayload : IVTubeStudioEvent<ModelConfigChangedEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.ModelConfigChanged;

    /// <inheritdoc/>
    public static JsonTypeInfo<ModelConfigChangedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelConfigChangedEventPayload;

    /// <summary>The id of the affected model.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The name of the affected model.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>True when the change affected the model's hotkey configuration.</summary>
    [JsonPropertyName("hotkeyConfigChanged")] public bool HotkeyConfigChanged { get; init; }
}

/// <summary>Payload of a <c>ModelMovedEvent</c>: fired when the model's position, rotation, or size changes.</summary>
public sealed record ModelMovedEventPayload : IVTubeStudioEvent<ModelMovedEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.ModelMoved;

    /// <inheritdoc/>
    public static JsonTypeInfo<ModelMovedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelMovedEventPayload;

    /// <summary>The id of the moved model.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The name of the moved model.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>The model's new position, rotation, and size.</summary>
    [JsonPropertyName("modelPosition")] public ModelPosition? ModelPosition { get; init; }
}

/// <summary>A model's on-screen transform: position, rotation, and size.</summary>
public sealed record ModelPosition
{
    /// <summary>X position.</summary>
    [JsonPropertyName("positionX")] public double PositionX { get; init; }

    /// <summary>Y position.</summary>
    [JsonPropertyName("positionY")] public double PositionY { get; init; }

    /// <summary>Size/scale value.</summary>
    [JsonPropertyName("size")] public double Size { get; init; }

    /// <summary>Rotation in degrees.</summary>
    [JsonPropertyName("rotation")] public double Rotation { get; init; }
}

/// <summary>Payload of a <c>HotkeyTriggeredEvent</c>: fired when a hotkey is triggered.</summary>
public sealed record HotkeyTriggeredEventPayload : IVTubeStudioEvent<HotkeyTriggeredEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.HotkeyTriggered;

    /// <inheritdoc/>
    public static JsonTypeInfo<HotkeyTriggeredEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.HotkeyTriggeredEventPayload;

    /// <summary>The id of the triggered hotkey.</summary>
    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }

    /// <summary>The display name of the triggered hotkey.</summary>
    [JsonPropertyName("hotkeyName")] public required string HotkeyName { get; init; }

    /// <summary>The hotkey's action type.</summary>
    [JsonPropertyName("hotkeyAction")] public string? HotkeyAction { get; init; }

    /// <summary>The file associated with the hotkey (for example the expression or animation file).</summary>
    [JsonPropertyName("hotkeyFile")] public string? HotkeyFile { get; init; }

    /// <summary>True when the hotkey was triggered via the API rather than by the user.</summary>
    [JsonPropertyName("hotkeyTriggeredByAPI")] public bool HotkeyTriggeredByApi { get; init; }

    /// <summary>The id of the model the hotkey belongs to.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The name of the model the hotkey belongs to.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>True when the hotkey belongs to a Live2D item rather than the main model.</summary>
    [JsonPropertyName("isLive2DItem")] public bool IsLive2DItem { get; init; }
}

/// <summary>Payload of a <c>ModelAnimationEvent</c>: fired during model animation playback.</summary>
public sealed record ModelAnimationEventPayload : IVTubeStudioEvent<ModelAnimationEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.ModelAnimation;

    /// <inheritdoc/>
    public static JsonTypeInfo<ModelAnimationEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelAnimationEventPayload;

    /// <summary>The type of animation event (for example animation start, end, or a custom animation event).</summary>
    [JsonPropertyName("animationEventType")] public string? AnimationEventType { get; init; }

    /// <summary>The time within the animation at which the event occurred, in seconds.</summary>
    [JsonPropertyName("animationEventTime")] public double AnimationEventTime { get; init; }

    /// <summary>Custom data carried by the animation event, if any.</summary>
    [JsonPropertyName("animationEventData")] public string? AnimationEventData { get; init; }

    /// <summary>The name of the animation.</summary>
    [JsonPropertyName("animationName")] public string? AnimationName { get; init; }

    /// <summary>The total length of the animation, in seconds.</summary>
    [JsonPropertyName("animationLength")] public double AnimationLength { get; init; }

    /// <summary>True when the animation is an idle animation.</summary>
    [JsonPropertyName("isIdleAnimation")] public bool IsIdleAnimation { get; init; }

    /// <summary>The id of the model the animation belongs to.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The name of the model the animation belongs to.</summary>
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }

    /// <summary>True when the animation belongs to a Live2D item rather than the main model.</summary>
    [JsonPropertyName("isLive2DItem")] public bool IsLive2DItem { get; init; }
}

/// <summary>Payload of an <c>ItemEvent</c>: fired for item-related changes.</summary>
public sealed record ItemEventPayload : IVTubeStudioEvent<ItemEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.Item;

    /// <inheritdoc/>
    public static JsonTypeInfo<ItemEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ItemEventPayload;

    /// <summary>The type of item event (for example added, removed, dropped, or pinned).</summary>
    [JsonPropertyName("itemEventType")] public string? ItemEventType { get; init; }

    /// <summary>The instance id of the affected item.</summary>
    [JsonPropertyName("itemInstanceID")] public string? ItemInstanceId { get; init; }

    /// <summary>The file name of the affected item.</summary>
    [JsonPropertyName("itemFileName")] public string? ItemFileName { get; init; }

    /// <summary>The item's position, rotation, and size at the time of the event.</summary>
    [JsonPropertyName("itemPosition")] public ModelPosition? ItemPosition { get; init; }
}

/// <summary>Payload of a <c>ModelClickedEvent</c>: fired when the model area is clicked.</summary>
public sealed record ModelClickedEventPayload : IVTubeStudioEvent<ModelClickedEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.ModelClicked;

    /// <inheritdoc/>
    public static JsonTypeInfo<ModelClickedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelClickedEventPayload;

    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The id of the loaded model.</summary>
    [JsonPropertyName("loadedModelID")] public string? LoadedModelId { get; init; }

    /// <summary>The name of the loaded model.</summary>
    [JsonPropertyName("loadedModelName")] public string? LoadedModelName { get; init; }

    /// <summary>True when the click actually hit the model (not empty space).</summary>
    [JsonPropertyName("modelWasClicked")] public bool ModelWasClicked { get; init; }

    /// <summary>The mouse button that was used for the click.</summary>
    [JsonPropertyName("mouseButtonID")] public int MouseButtonId { get; init; }

    /// <summary>The click position within the window.</summary>
    [JsonPropertyName("clickPosition")] public ClickPosition? ClickPosition { get; init; }

    /// <summary>The size of the VTube Studio window at the time of the click.</summary>
    [JsonPropertyName("windowSize")] public WindowSize? WindowSize { get; init; }

    /// <summary>The number of ArtMeshes hit by the click.</summary>
    [JsonPropertyName("clickedArtMeshCount")] public int ClickedArtMeshCount { get; init; }

    /// <summary>The ids of the ArtMeshes hit by the click.</summary>
    [JsonPropertyName("artMeshHits")] public IReadOnlyList<string> ArtMeshHits { get; init; } = [];
}

/// <summary>A 2D click position within the VTube Studio window.</summary>
public sealed record ClickPosition
{
    /// <summary>X coordinate of the click.</summary>
    [JsonPropertyName("x")] public double X { get; init; }

    /// <summary>Y coordinate of the click.</summary>
    [JsonPropertyName("y")] public double Y { get; init; }
}

/// <summary>The pixel dimensions of the VTube Studio window.</summary>
public sealed record WindowSize
{
    /// <summary>Window width in pixels.</summary>
    [JsonPropertyName("x")] public int X { get; init; }

    /// <summary>Window height in pixels.</summary>
    [JsonPropertyName("y")] public int Y { get; init; }
}

/// <summary>Payload of a <c>PostProcessingEvent</c>: fired when post-processing is toggled or its preset changes.</summary>
public sealed record PostProcessingEventPayload : IVTubeStudioEvent<PostProcessingEventPayload>
{
    /// <inheritdoc/>
    public static string EventName => VTubeStudioEventNames.PostProcessing;

    /// <inheritdoc/>
    public static JsonTypeInfo<PostProcessingEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.PostProcessingEventPayload;

    /// <summary>True when post-processing is currently switched on.</summary>
    [JsonPropertyName("currentOnState")] public bool CurrentOnState { get; init; }

    /// <summary>The name of the currently selected post-processing preset.</summary>
    [JsonPropertyName("currentPreset")] public string? CurrentPreset { get; init; }
}
