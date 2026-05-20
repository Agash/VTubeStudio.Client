using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record ArtMeshListResponse
{
    [JsonPropertyName("modelLoaded")] public bool ModelLoaded { get; init; }
    [JsonPropertyName("numberOfArtMeshNames")] public int NumberOfArtMeshNames { get; init; }
    [JsonPropertyName("numberOfArtMeshTags")] public int NumberOfArtMeshTags { get; init; }
    [JsonPropertyName("artMeshNames")] public IReadOnlyList<string> ArtMeshNames { get; init; } = [];
    [JsonPropertyName("artMeshTags")] public IReadOnlyList<string> ArtMeshTags { get; init; } = [];
}

public sealed record ColorTintRequest
{
    [JsonPropertyName("colorTint")] public required ColorTint ColorTint { get; init; }
    [JsonPropertyName("artMeshMatcher")] public required ArtMeshMatcher ArtMeshMatcher { get; init; }
}

public sealed record ColorTint
{
    [JsonPropertyName("colorR")] public required int ColorR { get; init; }
    [JsonPropertyName("colorG")] public required int ColorG { get; init; }
    [JsonPropertyName("colorB")] public required int ColorB { get; init; }
    [JsonPropertyName("colorA")] public int ColorA { get; init; } = 255;
    [JsonPropertyName("mixWithSceneLightingColor")] public double MixWithSceneLightingColor { get; init; } = 1d;
}

public sealed record ArtMeshMatcher
{
    [JsonPropertyName("tintAll")] public bool TintAll { get; init; }
    [JsonPropertyName("artMeshNumber")] public IReadOnlyList<int> ArtMeshNumber { get; init; } = [];
    [JsonPropertyName("nameExact")] public IReadOnlyList<string> NameExact { get; init; } = [];
    [JsonPropertyName("nameContains")] public IReadOnlyList<string> NameContains { get; init; } = [];
    [JsonPropertyName("tagExact")] public IReadOnlyList<string> TagExact { get; init; } = [];
    [JsonPropertyName("tagContains")] public IReadOnlyList<string> TagContains { get; init; } = [];
}
