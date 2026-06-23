using System.Text.Json;

namespace VTubeStudio.Client;

/// <summary>
/// Raised by <see cref="VTubeStudioClient.EventReceived"/> for every subscribed event the
/// VTube Studio server pushes. The typed payload is delivered via the strongly-typed
/// event hub (<see cref="Events.VTubeStudioEventHub.On{TPayload}(Action{TPayload})"/>); this
/// raw form is provided as an escape hatch for callers that want to handle unknown event names.
/// </summary>
public sealed class VTubeStudioEventArgs : EventArgs
{
    /// <summary>The wire-format event name (the frame's <c>messageType</c>, e.g. <c>"HotkeyTriggeredEvent"</c>).</summary>
    public required string EventName { get; init; }

    /// <summary>The raw <c>data</c> payload of the event frame, for callers that parse it themselves.</summary>
    public required JsonElement RawData { get; init; }

    /// <summary>The UTC time at which this client received the event frame.</summary>
    public required DateTimeOffset ReceivedAtUtc { get; init; }
}
