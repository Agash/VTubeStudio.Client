using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Events;

public sealed record ModelLoadedEventPayload : IVTubeStudioEvent<ModelLoadedEventPayload>
{
    public static string EventName => VTubeStudioEventNames.ModelLoaded;
    public static JsonTypeInfo<ModelLoadedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelLoadedEventPayload;

    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
}

public sealed record TrackingStatusChangedEventPayload : IVTubeStudioEvent<TrackingStatusChangedEventPayload>
{
    public static string EventName => VTubeStudioEventNames.TrackingStatusChanged;
    public static JsonTypeInfo<TrackingStatusChangedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.TrackingStatusChangedEventPayload;

    [JsonPropertyName("faceFound")] public bool FaceFound { get; init; }
    [JsonPropertyName("leftHandFound")] public bool LeftHandFound { get; init; }
    [JsonPropertyName("rightHandFound")] public bool RightHandFound { get; init; }
}

public sealed record BackgroundChangedEventPayload : IVTubeStudioEvent<BackgroundChangedEventPayload>
{
    public static string EventName => VTubeStudioEventNames.BackgroundChanged;
    public static JsonTypeInfo<BackgroundChangedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.BackgroundChangedEventPayload;

    [JsonPropertyName("backgroundName")] public required string BackgroundName { get; init; }
}

public sealed record ModelConfigChangedEventPayload : IVTubeStudioEvent<ModelConfigChangedEventPayload>
{
    public static string EventName => VTubeStudioEventNames.ModelConfigChanged;
    public static JsonTypeInfo<ModelConfigChangedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelConfigChangedEventPayload;

    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("hotkeyConfigChanged")] public bool HotkeyConfigChanged { get; init; }
}

public sealed record ModelMovedEventPayload : IVTubeStudioEvent<ModelMovedEventPayload>
{
    public static string EventName => VTubeStudioEventNames.ModelMoved;
    public static JsonTypeInfo<ModelMovedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelMovedEventPayload;

    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("modelPosition")] public ModelPosition? ModelPosition { get; init; }
}

public sealed record ModelPosition
{
    [JsonPropertyName("positionX")] public double PositionX { get; init; }
    [JsonPropertyName("positionY")] public double PositionY { get; init; }
    [JsonPropertyName("size")] public double Size { get; init; }
    [JsonPropertyName("rotation")] public double Rotation { get; init; }
}

public sealed record HotkeyTriggeredEventPayload : IVTubeStudioEvent<HotkeyTriggeredEventPayload>
{
    public static string EventName => VTubeStudioEventNames.HotkeyTriggered;
    public static JsonTypeInfo<HotkeyTriggeredEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.HotkeyTriggeredEventPayload;

    [JsonPropertyName("hotkeyID")] public required string HotkeyId { get; init; }
    [JsonPropertyName("hotkeyName")] public required string HotkeyName { get; init; }
    [JsonPropertyName("hotkeyAction")] public string? HotkeyAction { get; init; }
    [JsonPropertyName("hotkeyFile")] public string? HotkeyFile { get; init; }
    [JsonPropertyName("hotkeyTriggeredByAPI")] public bool HotkeyTriggeredByApi { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("isLive2DItem")] public bool IsLive2DItem { get; init; }
}

public sealed record ModelAnimationEventPayload : IVTubeStudioEvent<ModelAnimationEventPayload>
{
    public static string EventName => VTubeStudioEventNames.ModelAnimation;
    public static JsonTypeInfo<ModelAnimationEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelAnimationEventPayload;

    [JsonPropertyName("animationEventType")] public string? AnimationEventType { get; init; }
    [JsonPropertyName("animationEventTime")] public double AnimationEventTime { get; init; }
    [JsonPropertyName("animationEventData")] public string? AnimationEventData { get; init; }
    [JsonPropertyName("animationName")] public string? AnimationName { get; init; }
    [JsonPropertyName("animationLength")] public double AnimationLength { get; init; }
    [JsonPropertyName("isIdleAnimation")] public bool IsIdleAnimation { get; init; }
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }
    [JsonPropertyName("modelName")] public string? ModelName { get; init; }
    [JsonPropertyName("isLive2DItem")] public bool IsLive2DItem { get; init; }
}

public sealed record ItemEventPayload : IVTubeStudioEvent<ItemEventPayload>
{
    public static string EventName => VTubeStudioEventNames.Item;
    public static JsonTypeInfo<ItemEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ItemEventPayload;

    [JsonPropertyName("itemEventType")] public string? ItemEventType { get; init; }
    [JsonPropertyName("itemInstanceID")] public string? ItemInstanceId { get; init; }
    [JsonPropertyName("itemFileName")] public string? ItemFileName { get; init; }
    [JsonPropertyName("itemPosition")] public ModelPosition? ItemPosition { get; init; }
}

public sealed record ModelClickedEventPayload : IVTubeStudioEvent<ModelClickedEventPayload>
{
    public static string EventName => VTubeStudioEventNames.ModelClicked;
    public static JsonTypeInfo<ModelClickedEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.ModelClickedEventPayload;

    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("loadedModelID")] public string? LoadedModelId { get; init; }
    [JsonPropertyName("loadedModelName")] public string? LoadedModelName { get; init; }
    [JsonPropertyName("modelWasClicked")] public bool ModelWasClicked { get; init; }
    [JsonPropertyName("mouseButtonID")] public int MouseButtonId { get; init; }
    [JsonPropertyName("clickPosition")] public ClickPosition? ClickPosition { get; init; }
    [JsonPropertyName("windowSize")] public WindowSize? WindowSize { get; init; }
    [JsonPropertyName("clickedArtMeshCount")] public int ClickedArtMeshCount { get; init; }
    [JsonPropertyName("artMeshHits")] public IReadOnlyList<string> ArtMeshHits { get; init; } = [];
}

public sealed record ClickPosition
{
    [JsonPropertyName("x")] public double X { get; init; }
    [JsonPropertyName("y")] public double Y { get; init; }
}

public sealed record WindowSize
{
    [JsonPropertyName("x")] public int X { get; init; }
    [JsonPropertyName("y")] public int Y { get; init; }
}

public sealed record PostProcessingEventPayload : IVTubeStudioEvent<PostProcessingEventPayload>
{
    public static string EventName => VTubeStudioEventNames.PostProcessing;
    public static JsonTypeInfo<PostProcessingEventPayload> JsonTypeInfo => VTubeStudioJsonContext.Default.PostProcessingEventPayload;

    [JsonPropertyName("currentOnState")] public bool CurrentOnState { get; init; }
    [JsonPropertyName("currentPreset")] public string? CurrentPreset { get; init; }
}
