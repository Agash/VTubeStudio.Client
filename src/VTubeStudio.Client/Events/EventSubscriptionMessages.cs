using System.Text.Json;
using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Events;

public sealed record EventSubscriptionRequest
{
    [JsonPropertyName("eventName")] public required string EventName { get; init; }
    [JsonPropertyName("subscribe")] public required bool Subscribe { get; init; }
    [JsonPropertyName("config")] public JsonElement? Config { get; init; }
}

public sealed record EventSubscriptionResponse
{
    [JsonPropertyName("subscribedEventCount")] public int SubscribedEventCount { get; init; }
    [JsonPropertyName("subscribedEvents")] public IReadOnlyList<string> SubscribedEvents { get; init; } = [];
}

/// <summary>Well-known event-name constants used with <see cref="EventSubscriptionRequest"/>.</summary>
public static class VTubeStudioEventNames
{
    public const string Test = "TestEvent";
    public const string ModelLoaded = "ModelLoadedEvent";
    public const string TrackingStatusChanged = "TrackingStatusChangedEvent";
    public const string BackgroundChanged = "BackgroundChangedEvent";
    public const string ModelConfigChanged = "ModelConfigChangedEvent";
    public const string ModelMoved = "ModelMovedEvent";
    public const string ModelOutline = "ModelOutlineEvent";
    public const string HotkeyTriggered = "HotkeyTriggeredEvent";
    public const string ModelAnimation = "ModelAnimationEvent";
    public const string Item = "ItemEvent";
    public const string ModelClicked = "ModelClickedEvent";
    public const string PostProcessing = "PostProcessingEvent";
    public const string Live2DCubismEditorConnected = "Live2DCubismEditorConnectedEvent";
}
