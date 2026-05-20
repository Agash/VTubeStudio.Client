namespace VTubeStudio.Client;

/// <summary>Wire-format constants for the VTube Studio Public API.</summary>
public static class VTubeStudioApi
{
    /// <summary>The <c>apiName</c> value sent in every request and present on every response.</summary>
    public const string ApiName = "VTubeStudioPublicAPI";

    /// <summary>The <c>apiVersion</c> value sent in every request.</summary>
    public const string ApiVersion = "1.0";

    /// <summary>Default WebSocket endpoint (<c>ws://localhost:8001</c>).</summary>
    public static readonly Uri DefaultEndpoint = new("ws://localhost:8001");
}
