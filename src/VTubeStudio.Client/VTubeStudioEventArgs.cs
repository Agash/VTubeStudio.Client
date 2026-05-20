using System.Text.Json;

namespace VTubeStudio.Client;

/// <summary>
/// Raised by <see cref="VTubeStudioClient.EventReceived"/> for every subscribed event the
/// VTube Studio server pushes. The typed payload is delivered via the strongly-typed
/// <see cref="VTubeStudioClient.OnEvent{TPayload}"/> hook; this raw form is provided as an
/// escape hatch for callers that want to handle unknown event names.
/// </summary>
public sealed class VTubeStudioEventArgs : EventArgs
{
    public required string EventName { get; init; }

    public required JsonElement RawData { get; init; }

    public required DateTimeOffset ReceivedAtUtc { get; init; }
}
