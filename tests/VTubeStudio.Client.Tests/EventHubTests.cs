using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Tests;

[TestClass]
public sealed class EventHubTests
{
    [TestMethod]
    public void TypedHandler_ReceivesDeserialisedPayload()
    {
        VTubeStudioEventHub hub = new();
        HotkeyTriggeredEventPayload? received = null;
        using IDisposable _ = hub.On<HotkeyTriggeredEventPayload>(
            VTubeStudioEventNames.HotkeyTriggered,
            p => received = p,
            VTubeStudioJsonContext.Default.HotkeyTriggeredEventPayload);

        JsonElement raw = JsonDocument.Parse("""{"hotkeyID":"x","hotkeyName":"W","hotkeyTriggeredByAPI":true}""").RootElement;
        hub.Dispatch(VTubeStudioEventNames.HotkeyTriggered, raw);

        Assert.IsNotNull(received);
        Assert.AreEqual("x", received!.HotkeyId);
        Assert.IsTrue(received.HotkeyTriggeredByApi);
    }

    [TestMethod]
    public void Dispose_RemovesHandler()
    {
        VTubeStudioEventHub hub = new();
        int count = 0;
        IDisposable sub = hub.On<HotkeyTriggeredEventPayload>(
            VTubeStudioEventNames.HotkeyTriggered,
            _ => count++,
            VTubeStudioJsonContext.Default.HotkeyTriggeredEventPayload);

        JsonElement raw = JsonDocument.Parse("""{"hotkeyID":"x","hotkeyName":"W"}""").RootElement;
        hub.Dispatch(VTubeStudioEventNames.HotkeyTriggered, raw);
        Assert.AreEqual(1, count);

        sub.Dispose();
        hub.Dispatch(VTubeStudioEventNames.HotkeyTriggered, raw);
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void Dispatch_IgnoresUnregisteredEvents()
    {
        VTubeStudioEventHub hub = new();
        hub.Dispatch("UnknownEvent", JsonDocument.Parse("{}").RootElement);
        // No exception means pass.
    }
}
