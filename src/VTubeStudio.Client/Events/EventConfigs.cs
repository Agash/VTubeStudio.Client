using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Events;

/// <summary>Per-event optional configuration passed via <see cref="EventSubscriptionRequest.Config"/>.</summary>
/// <remarks>Each event that takes config has its own record; the generic event-subscription path accepts any of them.</remarks>
public sealed record TestEventConfig
{
    [JsonPropertyName("testMessageForEvent")] public string? TestMessageForEvent { get; init; }
}

public sealed record ModelLoadedEventConfig
{
    /// <summary>When set, only events for the listed model IDs are delivered.</summary>
    [JsonPropertyName("modelID")] public IReadOnlyList<string>? ModelId { get; init; }
}

public sealed record ModelOutlineEventConfig
{
    [JsonPropertyName("draw")] public bool Draw { get; init; }
}

public sealed record HotkeyTriggeredEventConfig
{
    [JsonPropertyName("onlyForAction")] public string? OnlyForAction { get; init; }
    [JsonPropertyName("ignoreHotkeysTriggeredByAPI")] public bool IgnoreHotkeysTriggeredByApi { get; init; }
}

public sealed record ModelAnimationEventConfig
{
    [JsonPropertyName("ignoreLive2DItems")] public bool IgnoreLive2DItems { get; init; }
    [JsonPropertyName("ignoreIdleAnimations")] public bool IgnoreIdleAnimations { get; init; }
}

public sealed record ItemEventConfig
{
    [JsonPropertyName("itemInstanceIDs")] public IReadOnlyList<string>? ItemInstanceIds { get; init; }
    [JsonPropertyName("itemFileNames")] public IReadOnlyList<string>? ItemFileNames { get; init; }
}

public sealed record ModelClickedEventConfig
{
    [JsonPropertyName("onlyClicksOnModel")] public bool OnlyClicksOnModel { get; init; }
}
