using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of an <c>APIStateResponse</c>: whether the API is active and the session authenticated.</summary>
public sealed record ApiStateResponse
{
    /// <summary>True when the VTube Studio API is currently running.</summary>
    [JsonPropertyName("active")] public required bool Active { get; init; }

    /// <summary>The VTube Studio version string.</summary>
    [JsonPropertyName("vTubeStudioVersion")] public required string VTubeStudioVersion { get; init; }

    /// <summary>True when this session has already authenticated.</summary>
    [JsonPropertyName("currentSessionAuthenticated")] public required bool CurrentSessionAuthenticated { get; init; }
}

/// <summary>Payload of a <c>StatisticsResponse</c>: VTube Studio runtime statistics.</summary>
public sealed record StatisticsResponse
{
    /// <summary>Milliseconds elapsed since VTube Studio started.</summary>
    [JsonPropertyName("uptime")] public long Uptime { get; init; }

    /// <summary>The current render framerate (frames per second).</summary>
    [JsonPropertyName("framerate")] public int Framerate { get; init; }

    /// <summary>The VTube Studio version string.</summary>
    [JsonPropertyName("vTubeStudioVersion")] public string? VTubeStudioVersion { get; init; }

    /// <summary>Number of plugins the user has authorised.</summary>
    [JsonPropertyName("allowedPlugins")] public int AllowedPlugins { get; init; }

    /// <summary>Number of plugins currently connected.</summary>
    [JsonPropertyName("connectedPlugins")] public int ConnectedPlugins { get; init; }

    /// <summary>True when VTube Studio was launched via Steam.</summary>
    [JsonPropertyName("startedWithSteam")] public bool StartedWithSteam { get; init; }

    /// <summary>VTube Studio window width in pixels.</summary>
    [JsonPropertyName("windowWidth")] public int WindowWidth { get; init; }

    /// <summary>VTube Studio window height in pixels.</summary>
    [JsonPropertyName("windowHeight")] public int WindowHeight { get; init; }

    /// <summary>True when the VTube Studio window is fullscreen.</summary>
    [JsonPropertyName("windowIsFullscreen")] public bool WindowIsFullscreen { get; init; }
}

/// <summary>Payload of a <c>FaceFoundResponse</c>: whether a face is currently being tracked.</summary>
public sealed record FaceFoundResponse
{
    /// <summary>True when a face is currently detected by tracking.</summary>
    [JsonPropertyName("found")] public required bool Found { get; init; }
}
