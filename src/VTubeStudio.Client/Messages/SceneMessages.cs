using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of a <c>SceneColorOverlayInfoResponse</c>: the scene lighting overlay state.</summary>
public sealed record SceneColorOverlayInfoResponse
{
    /// <summary>True when the lighting overlay is on.</summary>
    [JsonPropertyName("active")] public bool Active { get; init; }

    /// <summary>True when items are affected by the overlay.</summary>
    [JsonPropertyName("itemsIncluded")] public bool ItemsIncluded { get; init; }

    /// <summary>True captures a window; false captures a screen.</summary>
    [JsonPropertyName("isWindowCapture")] public bool IsWindowCapture { get; init; }

    /// <summary>Base brightness (0-100).</summary>
    [JsonPropertyName("baseBrightness")] public int BaseBrightness { get; init; }

    /// <summary>Color boost (0-100).</summary>
    [JsonPropertyName("colorBoost")] public int ColorBoost { get; init; }

    /// <summary>Smoothing (0-60).</summary>
    [JsonPropertyName("smoothing")] public int Smoothing { get; init; }

    /// <summary>Overlay red channel.</summary>
    [JsonPropertyName("colorOverlayR")] public int ColorOverlayR { get; init; }

    /// <summary>Overlay green channel.</summary>
    [JsonPropertyName("colorOverlayG")] public int ColorOverlayG { get; init; }

    /// <summary>Overlay blue channel.</summary>
    [JsonPropertyName("colorOverlayB")] public int ColorOverlayB { get; init; }

    /// <summary>Average red channel.</summary>
    [JsonPropertyName("colorAvgR")] public int ColorAvgR { get; init; }

    /// <summary>Average green channel.</summary>
    [JsonPropertyName("colorAvgG")] public int ColorAvgG { get; init; }

    /// <summary>Average blue channel.</summary>
    [JsonPropertyName("colorAvgB")] public int ColorAvgB { get; init; }

    /// <summary>Average color of the left capture part.</summary>
    [JsonPropertyName("leftCapturePart")] public SceneColorCapturePart? LeftCapturePart { get; init; }

    /// <summary>Average color of the middle capture part.</summary>
    [JsonPropertyName("middleCapturePart")] public SceneColorCapturePart? MiddleCapturePart { get; init; }

    /// <summary>Average color of the right capture part.</summary>
    [JsonPropertyName("rightCapturePart")] public SceneColorCapturePart? RightCapturePart { get; init; }
}

/// <summary>One capture part in <see cref="SceneColorOverlayInfoResponse"/>.</summary>
public sealed record SceneColorCapturePart
{
    /// <summary>True when the part is activated by the user.</summary>
    [JsonPropertyName("active")] public bool Active { get; init; }

    /// <summary>Red channel.</summary>
    [JsonPropertyName("colorR")] public int ColorR { get; init; }

    /// <summary>Green channel.</summary>
    [JsonPropertyName("colorG")] public int ColorG { get; init; }

    /// <summary>Blue channel.</summary>
    [JsonPropertyName("colorB")] public int ColorB { get; init; }
}
