using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace VTubeStudio.Client.Events;

/// <summary>
/// Strongly-typed event dispatcher: callers register handlers per typed payload and the hub
/// deserialises the raw frame against the matching <see cref="JsonTypeInfo"/> before invoking
/// them. Well-known payloads implement <see cref="IVTubeStudioEvent{TSelf}"/> so the hub knows
/// the event name and the JSON type info from the type parameter alone — no <c>JsonTypeInfo</c>
/// argument on the call site. Unknown event names still surface on
/// <see cref="VTubeStudioClient.EventReceived"/> for escape-hatch use.
/// </summary>
public sealed class VTubeStudioEventHub
{
    private readonly ConcurrentDictionary<string, List<TypedHandler>> _handlers = new(StringComparer.Ordinal);

    /// <summary>
    /// Subscribe a typed handler. Works with any payload type that implements
    /// <see cref="IVTubeStudioEvent{TSelf}"/> — i.e. every event payload the library ships.
    /// Returns an <see cref="IDisposable"/> that removes the handler on dispose.
    /// </summary>
    public IDisposable On<TPayload>(Action<TPayload> handler)
        where TPayload : class, IVTubeStudioEvent<TPayload>
    {
        ArgumentNullException.ThrowIfNull(handler);
        return OnCore(TPayload.EventName, handler, TPayload.JsonTypeInfo);
    }

    /// <summary>
    /// Subscribe a typed handler with explicit event-name and <see cref="JsonTypeInfo{T}"/>.
    /// Use this overload only for custom or unrecognised event payloads that don't implement
    /// <see cref="IVTubeStudioEvent{TSelf}"/>; for everything the library ships, prefer
    /// <see cref="On{TPayload}(Action{TPayload})"/>.
    /// </summary>
    public IDisposable On<TPayload>(string eventName, Action<TPayload> handler, JsonTypeInfo<TPayload> typeInfo)
        where TPayload : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(typeInfo);
        return OnCore(eventName, handler, typeInfo);
    }

    private Subscription OnCore<TPayload>(string eventName, Action<TPayload> handler, JsonTypeInfo<TPayload> typeInfo)
        where TPayload : class
    {
        TypedHandler typed = new(eventName, data =>
        {
            TPayload? payload = data.Deserialize(typeInfo);
            if (payload is not null) handler(payload);
        });

        List<TypedHandler> bucket = _handlers.GetOrAdd(eventName, _ => []);
        lock (bucket) bucket.Add(typed);
        return new Subscription(this, eventName, typed);
    }

    /// <summary>Called by <see cref="VTubeStudioClient"/> for every event frame received.</summary>
    public void Dispatch(string eventName, JsonElement data)
    {
        if (!_handlers.TryGetValue(eventName, out List<TypedHandler>? bucket))
        {
            return;
        }
        TypedHandler[] snapshot;
        lock (bucket) snapshot = [.. bucket];
        foreach (TypedHandler handler in snapshot)
        {
            handler.Invoke(data);
        }
    }

    private void Remove(string eventName, TypedHandler handler)
    {
        if (_handlers.TryGetValue(eventName, out List<TypedHandler>? bucket))
        {
            lock (bucket) _ = bucket.Remove(handler);
        }
    }

    private sealed record TypedHandler(string EventName, Action<JsonElement> Invoke);

    private sealed class Subscription(VTubeStudioEventHub hub, string eventName, TypedHandler handler) : IDisposable
    {
        public void Dispose() => hub.Remove(eventName, handler);
    }
}
