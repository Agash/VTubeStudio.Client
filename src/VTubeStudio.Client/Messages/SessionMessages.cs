using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

public sealed record ApiStateResponse
{
    [JsonPropertyName("active")] public required bool Active { get; init; }
    [JsonPropertyName("vTubeStudioVersion")] public required string VTubeStudioVersion { get; init; }
    [JsonPropertyName("currentSessionAuthenticated")] public required bool CurrentSessionAuthenticated { get; init; }
}

public sealed record StatisticsResponse
{
    [JsonPropertyName("uptime")] public long Uptime { get; init; }
    [JsonPropertyName("framerate")] public int Framerate { get; init; }
    [JsonPropertyName("vTubeStudioVersion")] public string? VTubeStudioVersion { get; init; }
    [JsonPropertyName("allowedPlugins")] public int AllowedPlugins { get; init; }
    [JsonPropertyName("connectedPlugins")] public int ConnectedPlugins { get; init; }
    [JsonPropertyName("startedWithSteam")] public bool StartedWithSteam { get; init; }
    [JsonPropertyName("windowWidth")] public int WindowWidth { get; init; }
    [JsonPropertyName("windowHeight")] public int WindowHeight { get; init; }
    [JsonPropertyName("windowIsFullscreen")] public bool WindowIsFullscreen { get; init; }
}

public sealed record FaceFoundResponse
{
    [JsonPropertyName("found")] public required bool Found { get; init; }
}
