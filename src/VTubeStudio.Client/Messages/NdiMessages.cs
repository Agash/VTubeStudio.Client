using System.Text.Json.Serialization;

namespace VTubeStudio.Client.Messages;

/// <summary>Payload of an <c>NDIConfigRequest</c>: reads or changes the NDI configuration.</summary>
public sealed record NdiConfigRequest
{
    /// <summary>False reads the current config; true applies the remaining fields.</summary>
    [JsonPropertyName("setNewConfig")] public bool SetNewConfig { get; init; }

    /// <summary>True turns the NDI stream on.</summary>
    [JsonPropertyName("ndiActive")] public bool NdiActive { get; init; }

    /// <summary>True uses NDI 5 instead of NDI 4.</summary>
    [JsonPropertyName("useNDI5")] public bool UseNdi5 { get; init; }

    /// <summary>True uses a custom resolution instead of the window resolution.</summary>
    [JsonPropertyName("useCustomResolution")] public bool UseCustomResolution { get; init; }

    /// <summary>Custom width (256-8192, multiple of 16); -1 leaves it unchanged.</summary>
    [JsonPropertyName("customWidthNDI")] public int CustomWidthNdi { get; init; }

    /// <summary>Custom height (256-8192, multiple of 8); -1 leaves it unchanged.</summary>
    [JsonPropertyName("customHeightNDI")] public int CustomHeightNdi { get; init; }
}

/// <summary>Payload of an <c>NDIConfigResponse</c>: the current NDI configuration.</summary>
public sealed record NdiConfigResponse
{
    /// <summary>True turns the NDI stream on.</summary>
    [JsonPropertyName("ndiActive")] public bool NdiActive { get; init; }

    /// <summary>True uses NDI 5 instead of NDI 4.</summary>
    [JsonPropertyName("useNDI5")] public bool UseNdi5 { get; init; }

    /// <summary>True uses a custom resolution instead of the window resolution.</summary>
    [JsonPropertyName("useCustomResolution")] public bool UseCustomResolution { get; init; }

    /// <summary>Custom width in pixels.</summary>
    [JsonPropertyName("customWidthNDI")] public int CustomWidthNdi { get; init; }

    /// <summary>Custom height in pixels.</summary>
    [JsonPropertyName("customHeightNDI")] public int CustomHeightNdi { get; init; }
}
