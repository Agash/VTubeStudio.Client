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
}
