using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of an <c>ArtMeshListResponse</c>: the ArtMesh names and tags in the current model.</summary>
public sealed record ArtMeshListResponse
{
    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>Total number of ArtMesh names in the model.</summary>
    [JsonPropertyName("numberOfArtMeshNames")] public int NumberOfArtMeshNames { get; init; }

    /// <summary>Total number of distinct ArtMesh tags in the model.</summary>
    [JsonPropertyName("numberOfArtMeshTags")] public int NumberOfArtMeshTags { get; init; }

    /// <summary>The ArtMesh identifiers (names) in the model.</summary>
    [JsonPropertyName("artMeshNames")] public IReadOnlyList<string> ArtMeshNames { get; init; } = [];

    /// <summary>The distinct tags applied to ArtMeshes in the model.</summary>
    [JsonPropertyName("artMeshTags")] public IReadOnlyList<string> ArtMeshTags { get; init; } = [];

    /// <summary>Total number of ArtMesh groups in the model.</summary>
    [JsonPropertyName("numberOfArtMeshGroups")] public int NumberOfArtMeshGroups { get; init; }

    /// <summary>The ArtMesh groups set up for the model.</summary>
    [JsonPropertyName("artMeshGroups")] public IReadOnlyList<ArtMeshGroup> ArtMeshGroups { get; init; } = [];
}

/// <summary>An ArtMesh group in <see cref="ArtMeshListResponse.ArtMeshGroups"/>.</summary>
public sealed record ArtMeshGroup
{
    /// <summary>The group id, unique per model.</summary>
    [JsonPropertyName("groupID")] public required string GroupId { get; init; }

    /// <summary>The group name; names are not guaranteed unique.</summary>
    [JsonPropertyName("groupName")] public string? GroupName { get; init; }

    /// <summary>Number of ArtMeshes in the group.</summary>
    [JsonPropertyName("numberOfArtMeshesInGroup")] public int NumberOfArtMeshesInGroup { get; init; }

    /// <summary>The ArtMesh names in the group.</summary>
    [JsonPropertyName("artMeshNames")] public IReadOnlyList<string> ArtMeshNames { get; init; } = [];
}

/// <summary>Payload of a <c>ColorTintRequest</c>: applies a color tint to the ArtMeshes selected by a matcher.</summary>
public sealed record ColorTintRequest
{
    /// <summary>The color and blend settings to apply.</summary>
    [JsonPropertyName("colorTint")] public required ColorTint ColorTint { get; init; }

    /// <summary>Selects which ArtMeshes the tint is applied to.</summary>
    [JsonPropertyName("artMeshMatcher")] public required ArtMeshMatcher ArtMeshMatcher { get; init; }
}

/// <summary>The color and blend settings of a <see cref="ColorTintRequest"/>.</summary>
public sealed record ColorTint
{
    /// <summary>Red channel (0-255).</summary>
    [JsonPropertyName("colorR")] public required int ColorR { get; init; }

    /// <summary>Green channel (0-255).</summary>
    [JsonPropertyName("colorG")] public required int ColorG { get; init; }

    /// <summary>Blue channel (0-255).</summary>
    [JsonPropertyName("colorB")] public required int ColorB { get; init; }

    /// <summary>Alpha/opacity (0-255); defaults to fully opaque.</summary>
    [JsonPropertyName("colorA")] public int ColorA { get; init; } = 255;

    /// <summary>How strongly the tint mixes with the scene lighting color (0-1); defaults to 1.</summary>
    [JsonPropertyName("mixWithSceneLightingColor")] public double MixWithSceneLightingColor { get; init; } = 1d;
}

/// <summary>Payload of a <c>ColorTintResponse</c>: how many ArtMeshes were tinted.</summary>
public sealed record ColorTintResponse
{
    /// <summary>Number of ArtMeshes the tint was applied to.</summary>
    [JsonPropertyName("matchedArtMeshes")] public int MatchedArtMeshes { get; init; }
}

/// <summary>One ArtMesh hit: where a position check struck an ArtMesh.</summary>
public sealed record ArtMeshHit
{
    /// <summary>Order in the ArtMesh stack at the hit position; 0 is topmost.</summary>
    [JsonPropertyName("artMeshOrder")] public int ArtMeshOrder { get; init; }

    /// <summary>True when the hit ArtMesh is masked.</summary>
    [JsonPropertyName("isMasked")] public bool IsMasked { get; init; }

    /// <summary>Exact hit position within the ArtMesh.</summary>
    [JsonPropertyName("hitInfo")] public ArtMeshHitInfo? HitInfo { get; init; }
}

/// <summary>Exact hit position within an ArtMesh, as barycentric coordinates.</summary>
public sealed record ArtMeshHitInfo
{
    /// <summary>The id of the model the ArtMesh belongs to.</summary>
    [JsonPropertyName("modelID")] public string? ModelId { get; init; }

    /// <summary>The id of the hit ArtMesh.</summary>
    [JsonPropertyName("artMeshID")] public string? ArtMeshId { get; init; }

    /// <summary>Base angle for the hit position.</summary>
    [JsonPropertyName("angle")] public double Angle { get; init; }

    /// <summary>Base size value for the hit position.</summary>
    [JsonPropertyName("size")] public double Size { get; init; }

    /// <summary>First vertex id of the hit triangle.</summary>
    [JsonPropertyName("vertexID1")] public int VertexId1 { get; init; }

    /// <summary>Second vertex id of the hit triangle.</summary>
    [JsonPropertyName("vertexID2")] public int VertexId2 { get; init; }

    /// <summary>Third vertex id of the hit triangle.</summary>
    [JsonPropertyName("vertexID3")] public int VertexId3 { get; init; }

    /// <summary>Barycentric weight for the first vertex.</summary>
    [JsonPropertyName("vertexWeight1")] public double VertexWeight1 { get; init; }

    /// <summary>Barycentric weight for the second vertex.</summary>
    [JsonPropertyName("vertexWeight2")] public double VertexWeight2 { get; init; }

    /// <summary>Barycentric weight for the third vertex.</summary>
    [JsonPropertyName("vertexWeight3")] public double VertexWeight3 { get; init; }
}

/// <summary>Payload of an <c>ArtMeshSelectionRequest</c>: asks the user to select ArtMeshes.</summary>
public sealed record ArtMeshSelectionRequest
{
    /// <summary>Override for the text shown above the selection list.</summary>
    [JsonPropertyName("textOverride")] public string? TextOverride { get; init; }

    /// <summary>Override for the help text shown from the selection list.</summary>
    [JsonPropertyName("helpOverride")] public string? HelpOverride { get; init; }

    /// <summary>How many ArtMeshes the user must select; 0 or less means any number.</summary>
    [JsonPropertyName("requestedArtMeshCount")] public int RequestedArtMeshCount { get; init; }

    /// <summary>ArtMesh ids to pre-activate in the list.</summary>
    [JsonPropertyName("activeArtMeshes")] public IReadOnlyList<string> ActiveArtMeshes { get; init; } = [];
}

/// <summary>Payload of an <c>ArtMeshSelectionResponse</c>: the user's ArtMesh selection.</summary>
public sealed record ArtMeshSelectionResponse
{
    /// <summary>True when the user confirmed with OK; false on Cancel.</summary>
    [JsonPropertyName("success")] public bool Success { get; init; }

    /// <summary>The activated ArtMesh ids.</summary>
    [JsonPropertyName("activeArtMeshes")] public IReadOnlyList<string> ActiveArtMeshes { get; init; } = [];

    /// <summary>The deactivated ArtMesh ids.</summary>
    [JsonPropertyName("inactiveArtMeshes")] public IReadOnlyList<string> InactiveArtMeshes { get; init; } = [];
}

/// <summary>Payload of an <c>ArtMeshAtPositionRequest</c>: lists ArtMeshes at a position.</summary>
public sealed record ArtMeshAtPositionRequest
{
    /// <summary>X position in VTube Studio coordinates.</summary>
    [JsonPropertyName("x")] public required double X { get; init; }

    /// <summary>Y position in VTube Studio coordinates.</summary>
    [JsonPropertyName("y")] public required double Y { get; init; }

    /// <summary>Shows a position dot for this many seconds; 0 disables it.</summary>
    [JsonPropertyName("visualize")] public double Visualize { get; init; }
}

/// <summary>Payload of an <c>ArtMeshAtPositionResponse</c>: the ArtMeshes at the checked position.</summary>
public sealed record ArtMeshAtPositionResponse
{
    /// <summary>True when a model is loaded.</summary>
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }

    /// <summary>The id of the loaded model.</summary>
    [JsonPropertyName("loadedModelID")] public string? LoadedModelId { get; init; }

    /// <summary>The name of the loaded model.</summary>
    [JsonPropertyName("loadedModelName")] public string? LoadedModelName { get; init; }

    /// <summary>True when the position hit the model.</summary>
    [JsonPropertyName("modelWasHit")] public bool ModelWasHit { get; init; }

    /// <summary>The checked position.</summary>
    [JsonPropertyName("checkedPosition")] public ArtMeshPosition? CheckedPosition { get; init; }

    /// <summary>The VTube Studio window size in pixels.</summary>
    [JsonPropertyName("windowSize")] public ArtMeshWindowSize? WindowSize { get; init; }

    /// <summary>Number of ArtMeshes at the checked position.</summary>
    [JsonPropertyName("artMeshHitCount")] public int ArtMeshHitCount { get; init; }

    /// <summary>The ArtMeshes at the checked position, topmost first.</summary>
    [JsonPropertyName("artMeshHits")] public IReadOnlyList<ArtMeshHit> ArtMeshHits { get; init; } = [];
}

/// <summary>A 2D position in VTube Studio coordinates.</summary>
public sealed record ArtMeshPosition
{
    /// <summary>X coordinate.</summary>
    [JsonPropertyName("x")] public double X { get; init; }

    /// <summary>Y coordinate.</summary>
    [JsonPropertyName("y")] public double Y { get; init; }
}

/// <summary>A window size in pixels.</summary>
public sealed record ArtMeshWindowSize
{
    /// <summary>Width in pixels.</summary>
    [JsonPropertyName("x")] public int X { get; init; }

    /// <summary>Height in pixels.</summary>
    [JsonPropertyName("y")] public int Y { get; init; }
}
/// <summary>Selects which ArtMeshes a <see cref="ColorTintRequest"/> applies to. Selections are combined (OR).</summary>
public sealed record ArtMeshMatcher
{
    /// <summary>When true, applies to every ArtMesh in the model and ignores the other selectors.</summary>
    [JsonPropertyName("tintAll")] public bool TintAll { get; init; }

    /// <summary>Selects ArtMeshes by their index (1-based ArtMesh number).</summary>
    [JsonPropertyName("artMeshNumber")] public IReadOnlyList<int> ArtMeshNumber { get; init; } = [];

    /// <summary>Selects ArtMeshes whose name exactly matches one of these values.</summary>
    [JsonPropertyName("nameExact")] public IReadOnlyList<string> NameExact { get; init; } = [];

    /// <summary>Selects ArtMeshes whose name contains one of these substrings.</summary>
    [JsonPropertyName("nameContains")] public IReadOnlyList<string> NameContains { get; init; } = [];

    /// <summary>Selects ArtMeshes whose tag exactly matches one of these values.</summary>
    [JsonPropertyName("tagExact")] public IReadOnlyList<string> TagExact { get; init; } = [];

    /// <summary>Selects ArtMeshes that have a tag containing one of these substrings.</summary>
    [JsonPropertyName("tagContains")] public IReadOnlyList<string> TagContains { get; init; } = [];

    /// <summary>Selects ArtMeshes in one or more ArtMesh groups by group id.</summary>
    [JsonPropertyName("artMeshGroupIDExact")] public IReadOnlyList<string> ArtMeshGroupIdExact { get; init; } = [];
}
