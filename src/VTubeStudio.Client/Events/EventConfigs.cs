using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Events;

/// <summary>Per-event optional configuration passed via <see cref="EventSubscriptionRequest.Config"/>.</summary>
/// <remarks>Each event that takes config has its own record; the generic event-subscription path accepts any of them.</remarks>
public sealed record TestEventConfig
{
    /// <summary>An arbitrary message that VTube Studio echoes back in every test event.</summary>
    [JsonPropertyName("testMessageForEvent")] public string? TestMessageForEvent { get; init; }
}

/// <summary>Config for the model-loaded event.</summary>
public sealed record ModelLoadedEventConfig
{
    /// <summary>When set, only events for the listed model IDs are delivered.</summary>
    [JsonPropertyName("modelID")] public IReadOnlyList<string>? ModelId { get; init; }
}

/// <summary>Config for the model-outline event.</summary>
public sealed record ModelOutlineEventConfig
{
    /// <summary>When true, VTube Studio draws the model outline on screen while the subscription is active.</summary>
    [JsonPropertyName("draw")] public bool Draw { get; init; }
}

/// <summary>Config for the hotkey-triggered event.</summary>
public sealed record HotkeyTriggeredEventConfig
{
    /// <summary>When set, only hotkeys of this action type raise the event.</summary>
    [JsonPropertyName("onlyForAction")] public string? OnlyForAction { get; init; }

    /// <summary>When true, hotkeys triggered via the API are not reported by the event.</summary>
    [JsonPropertyName("ignoreHotkeysTriggeredByAPI")] public bool IgnoreHotkeysTriggeredByApi { get; init; }
}

/// <summary>Config for the model-animation event.</summary>
public sealed record ModelAnimationEventConfig
{
    /// <summary>When true, animation events for Live2D items are not reported.</summary>
    [JsonPropertyName("ignoreLive2DItems")] public bool IgnoreLive2DItems { get; init; }

    /// <summary>When true, idle-animation events are not reported.</summary>
    [JsonPropertyName("ignoreIdleAnimations")] public bool IgnoreIdleAnimations { get; init; }
}

/// <summary>Config for the item event.</summary>
public sealed record ItemEventConfig
{
    /// <summary>When set, only events for the listed item instance ids are delivered.</summary>
    [JsonPropertyName("itemInstanceIDs")] public IReadOnlyList<string>? ItemInstanceIds { get; init; }

    /// <summary>When set, only events for items with the listed file names are delivered.</summary>
    [JsonPropertyName("itemFileNames")] public IReadOnlyList<string>? ItemFileNames { get; init; }
}

/// <summary>Config for the model-clicked event.</summary>
public sealed record ModelClickedEventConfig
{
    /// <summary>When true, only clicks that hit the model (not empty space) raise the event.</summary>
    [JsonPropertyName("onlyClicksOnModel")] public bool OnlyClicksOnModel { get; init; }
}
