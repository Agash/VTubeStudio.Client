using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Tests;

[TestClass]
public sealed class EventHubTests
{
    [TestMethod]
    public void TypedHandler_ViaIVTubeStudioEvent_ReceivesDeserialisedPayload()
    {
        VTubeStudioEventHub hub = new();
        HotkeyTriggeredEventPayload? received = null;
        using IDisposable _ = hub.On<HotkeyTriggeredEventPayload>(p => received = p);

        JsonElement raw = JsonDocument.Parse("""{"hotkeyID":"x","hotkeyName":"W","hotkeyTriggeredByAPI":true}""").RootElement;
        hub.Dispatch(HotkeyTriggeredEventPayload.EventName, raw);

        Assert.IsNotNull(received);
        Assert.AreEqual("x", received!.HotkeyId);
        Assert.IsTrue(received.HotkeyTriggeredByApi);
    }

    [TestMethod]
    public void ExplicitOverload_StillWorksForCustomTypes()
    {
        VTubeStudioEventHub hub = new();
        ModelLoadedEventPayload? received = null;
        // Custom subscription path — useful for payloads the lib doesn't recognise.
        using IDisposable _ = hub.On<ModelLoadedEventPayload>(
            "CustomEvent",
            p => received = p,
            VTubeStudioJsonContext.Default.ModelLoadedEventPayload);

        JsonElement raw = JsonDocument.Parse("""{"modelLoaded":true,"modelName":"M","modelID":"id-1"}""").RootElement;
        hub.Dispatch("CustomEvent", raw);

        Assert.IsNotNull(received);
        Assert.AreEqual("M", received!.ModelName);
    }

    [TestMethod]
    public void Dispose_RemovesHandler()
    {
        VTubeStudioEventHub hub = new();
        int count = 0;
        IDisposable sub = hub.On<HotkeyTriggeredEventPayload>(_ => count++);

        JsonElement raw = JsonDocument.Parse("""{"hotkeyID":"x","hotkeyName":"W"}""").RootElement;
        hub.Dispatch(HotkeyTriggeredEventPayload.EventName, raw);
        Assert.AreEqual(1, count);

        sub.Dispose();
        hub.Dispatch(HotkeyTriggeredEventPayload.EventName, raw);
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void Dispatch_IgnoresUnregisteredEvents()
    {
        VTubeStudioEventHub hub = new();
        hub.Dispatch("UnknownEvent", JsonDocument.Parse("{}").RootElement);
        // No exception means pass.
    }

    [TestMethod]
    public void IVTubeStudioEvent_ReportsCorrectWireName()
    {
        Assert.AreEqual("HotkeyTriggeredEvent", HotkeyTriggeredEventPayload.EventName);
        Assert.AreEqual("ModelLoadedEvent", ModelLoadedEventPayload.EventName);
        Assert.AreEqual("TrackingStatusChangedEvent", TrackingStatusChangedEventPayload.EventName);
        Assert.AreEqual("ItemEvent", ItemEventPayload.EventName);
    }
}
