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

/// <summary>Config for the expression-toggled event.</summary>
public sealed record ExpressionToggledEventConfig
{
    /// <summary>When true, all expression states are sent once on subscribe.</summary>
    [JsonPropertyName("sendAllActiveStatesOnSubscription")] public bool SendAllActiveStatesOnSubscription { get; init; }

    /// <summary>When true, Live2D item expressions are not reported.</summary>
    [JsonPropertyName("ignoreLive2DItems")] public bool IgnoreLive2DItems { get; init; }
}

/// <summary>Config for the ArtMesh tracking event.</summary>
public sealed record ArtMeshTrackingEventConfig
{
    /// <summary>Events per second (1-60).</summary>
    [JsonPropertyName("frequency")] public int Frequency { get; init; }

    /// <summary>The points to track; at most 500 per session.</summary>
    [JsonPropertyName("trackingPoints")] public IReadOnlyList<ArtMeshTrackingPoint> TrackingPoints { get; init; } = [];
}

/// <summary>One point in <see cref="ArtMeshTrackingEventConfig.TrackingPoints"/>.</summary>
public sealed record ArtMeshTrackingPoint
{
    /// <summary>Caller-assigned point id, unique per subscription.</summary>
    [JsonPropertyName("trackingPointID")] public required string TrackingPointId { get; init; }

    /// <summary>Barycentric coordinates of the point.</summary>
    [JsonPropertyName("artMeshCoords")] public ArtMeshTrackingCoords? ArtMeshCoords { get; init; }

    /// <summary>When true, a visualizer circle is shown at the point.</summary>
    [JsonPropertyName("visualize")] public bool Visualize { get; init; }
}

/// <summary>Barycentric coordinates of an <see cref="ArtMeshTrackingPoint"/>.</summary>
public sealed record ArtMeshTrackingCoords
{
    /// <summary>The id of the model.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The id of the ArtMesh.</summary>
    [JsonPropertyName("artMeshID")] public string? ArtMeshId { get; init; }

    /// <summary>First vertex id.</summary>
    [JsonPropertyName("vertexID1")] public int VertexId1 { get; init; }

    /// <summary>Second vertex id.</summary>
    [JsonPropertyName("vertexID2")] public int VertexId2 { get; init; }

    /// <summary>Third vertex id.</summary>
    [JsonPropertyName("vertexID3")] public int VertexId3 { get; init; }

    /// <summary>Barycentric weight for the first vertex.</summary>
    [JsonPropertyName("vertexWeight1")] public double VertexWeight1 { get; init; }

    /// <summary>Barycentric weight for the second vertex.</summary>
    [JsonPropertyName("vertexWeight2")] public double VertexWeight2 { get; init; }

    /// <summary>Barycentric weight for the third vertex.</summary>
    [JsonPropertyName("vertexWeight3")] public double VertexWeight3 { get; init; }

    /// <summary>Base angle.</summary>
    [JsonPropertyName("angle")] public double Angle { get; init; }

    /// <summary>Base size value.</summary>
    [JsonPropertyName("size")] public double Size { get; init; }
}

/// <summary>Config for the ArtMesh outline event.</summary>
public sealed record ArtMeshOutlineEventConfig
{
    /// <summary>Events per second (1-30).</summary>
    [JsonPropertyName("frequency")] public int Frequency { get; init; }

    /// <summary>The ArtMeshes to track; at most 100 per session.</summary>
    [JsonPropertyName("artMeshes")] public IReadOnlyList<ArtMeshOutlineEntry> ArtMeshes { get; init; } = [];
}

/// <summary>One entry in <see cref="ArtMeshOutlineEventConfig.ArtMeshes"/>.</summary>
public sealed record ArtMeshOutlineEntry
{
    /// <summary>The id of the model.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The id of the ArtMesh.</summary>
    [JsonPropertyName("artMeshID")] public string? ArtMeshId { get; init; }
}
