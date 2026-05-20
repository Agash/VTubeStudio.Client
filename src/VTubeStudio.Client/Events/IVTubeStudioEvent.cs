using System.Text.Json.Serialization.Metadata;

namespace VTubeStudio.Client.Events;

/// <summary>
/// Marker interface implemented by every well-known VTube Studio event payload. Carries
/// the wire-format event name and the source-generated <see cref="JsonTypeInfo{T}"/> so
/// callers can subscribe and handle events generically:
/// <code>
/// client.Events.On&lt;HotkeyTriggeredEventPayload&gt;(e => ...);
/// await client.SubscribeAsync&lt;HotkeyTriggeredEventPayload&gt;();
/// </code>
/// </summary>
/// <typeparam name="TSelf">The implementing payload record. Use as <c>: IVTubeStudioEvent&lt;HotkeyTriggeredEventPayload&gt;</c>.</typeparam>
public interface IVTubeStudioEvent<TSelf>
    where TSelf : class, IVTubeStudioEvent<TSelf>
{
    /// <summary>The wire-format event name (matches values in <see cref="VTubeStudioEventNames"/>).</summary>
    static abstract string EventName { get; }

    /// <summary>Source-generated <see cref="JsonTypeInfo{T}"/> for this payload type.</summary>
    static abstract JsonTypeInfo<TSelf> JsonTypeInfo { get; }
}
