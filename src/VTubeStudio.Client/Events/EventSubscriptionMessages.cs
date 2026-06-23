using System.Text.Json;
using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Events;

/// <summary>Payload of an <c>EventSubscriptionRequest</c>: subscribes to or unsubscribes from a named event.</summary>
public sealed record EventSubscriptionRequest
{
    /// <summary>The event to (un)subscribe; one of the values in <see cref="VTubeStudioEventNames"/>.</summary>
    [JsonPropertyName("eventName")] public required string EventName { get; init; }

    /// <summary>True to subscribe, false to unsubscribe.</summary>
    [JsonPropertyName("subscribe")] public required bool Subscribe { get; init; }

    /// <summary>Optional per-event configuration (see the <c>*EventConfig</c> records); null for no config.</summary>
    [JsonPropertyName("config")] public JsonElement? Config { get; init; }
}

/// <summary>Payload of an <c>EventSubscriptionResponse</c>: the events the session is now subscribed to.</summary>
public sealed record EventSubscriptionResponse
{
    /// <summary>Number of events the session is currently subscribed to.</summary>
    [JsonPropertyName("subscribedEventCount")] public int SubscribedEventCount { get; init; }

    /// <summary>The names of the events the session is currently subscribed to.</summary>
    [JsonPropertyName("subscribedEvents")] public IReadOnlyList<string> SubscribedEvents { get; init; } = [];
}

/// <summary>Well-known event-name constants used with <see cref="EventSubscriptionRequest"/>.</summary>
public static class VTubeStudioEventNames
{
    /// <summary>Test event used to verify event subscription works.</summary>
    public const string Test = "TestEvent";

    /// <summary>Raised when a model finishes loading or is unloaded.</summary>
    public const string ModelLoaded = "ModelLoadedEvent";

    /// <summary>Raised when face/hand tracking is gained or lost.</summary>
    public const string TrackingStatusChanged = "TrackingStatusChangedEvent";

    /// <summary>Raised when the background changes.</summary>
    public const string BackgroundChanged = "BackgroundChangedEvent";

    /// <summary>Raised when the model configuration (for example its hotkeys) changes.</summary>
    public const string ModelConfigChanged = "ModelConfigChangedEvent";

    /// <summary>Raised when the model's position, rotation, or size changes.</summary>
    public const string ModelMoved = "ModelMovedEvent";

    /// <summary>Raised with the model outline data while a model-outline subscription is active.</summary>
    public const string ModelOutline = "ModelOutlineEvent";

    /// <summary>Raised when a hotkey is triggered.</summary>
    public const string HotkeyTriggered = "HotkeyTriggeredEvent";

    /// <summary>Raised during model animation playback (animation start/end and animation events).</summary>
    public const string ModelAnimation = "ModelAnimationEvent";

    /// <summary>Raised for item-related changes (added, removed, and similar).</summary>
    public const string Item = "ItemEvent";

    /// <summary>Raised when the model is clicked.</summary>
    public const string ModelClicked = "ModelClickedEvent";

    /// <summary>Raised when post-processing effects are toggled or their preset changes.</summary>
    public const string PostProcessing = "PostProcessingEvent";

    /// <summary>Raised when the Live2D Cubism editor connects to VTube Studio.</summary>
    public const string Live2DCubismEditorConnected = "Live2DCubismEditorConnectedEvent";
}
