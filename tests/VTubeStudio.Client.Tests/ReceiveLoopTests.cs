using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Tests for receive path classification of event frames versus responses.
/// </summary>
[TestClass]
public sealed class ReceiveLoopTests
{
    private static VTubeStudioClient CreateClient() => new(new VTubeStudioClientOptions
    {
        PluginName = "ReceiveLoopTests",
        PluginDeveloper = "Tests",
    });

    private static List<VTubeStudioEventArgs> CaptureRaw(VTubeStudioClient client)
    {
        List<VTubeStudioEventArgs> seen = [];
        client.EventReceived += (_, e) =>
        {
            lock (seen) seen.Add(e);
        };
        return seen;
    }

    // Event frames carry a requestID.
    private static string ModelLoadedFrame(string? requestId)
    {
        string requestIdField = requestId is null ? string.Empty : "\"requestID\":\"" + requestId + "\",";
        return "{\"apiName\":\"VTubeStudioPublicAPI\",\"apiVersion\":\"1.0\",\"timestamp\":1788683673093,\"messageType\":\"ModelLoadedEvent\"," + requestIdField + "\"data\":{\"modelLoaded\":true,\"modelName\":\"akari\",\"modelID\":\"8e015e806a144842845b8b98fe3b7ec9\"}}";
    }

    private static TaskCompletionSource<VTubeStudioEnvelope> AddPending(VTubeStudioClient client, string requestId)
    {
        TaskCompletionSource<VTubeStudioEnvelope> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.IsTrue(client._pending.TryAdd(requestId, tcs));
        return tcs;
    }

    [TestMethod]
    public async Task ModelLoadedEvent_WithNoRequestId_IsDispatched()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);
        ModelLoadedEventPayload? typed = null;
        using IDisposable _ = client.Events.On<ModelLoadedEventPayload>(p => typed = p);

        client.DispatchMessage(ModelLoadedFrame(requestId: null));

        Assert.AreEqual(1, raw.Count);
        Assert.AreEqual("ModelLoadedEvent", raw[0].EventName);
        Assert.IsNotNull(typed);
        Assert.AreEqual("akari", typed!.ModelName);
        Assert.IsTrue(typed.ModelLoaded);
    }

    [TestMethod]
    public async Task ModelLoadedEvent_WithServerGeneratedRequestId_IsDispatched()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);
        ModelLoadedEventPayload? typed = null;
        using IDisposable _ = client.Events.On<ModelLoadedEventPayload>(p => typed = p);

        client.DispatchMessage(ModelLoadedFrame(requestId: "88c38fae970c4c1998c976259097a6f3"));

        Assert.AreEqual(1, raw.Count);
        Assert.AreEqual("ModelLoadedEvent", raw[0].EventName);
        Assert.AreEqual("akari", raw[0].RawData.GetProperty("modelName").GetString());
        Assert.IsNotNull(typed);
        Assert.AreEqual("8e015e806a144842845b8b98fe3b7ec9", typed!.ModelId);
    }

    [TestMethod]
    public async Task TestEvent_WithRequestId_IsDispatchedAsRawEvent()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);

        client.DispatchMessage(
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683670457,"messageType":"TestEvent","requestID":"7d0cecae33d94900bc0a32758988dc02","data":{"yourTestMessage":"diag123","counter":1360}}""");

        Assert.AreEqual(1, raw.Count);
        Assert.AreEqual("TestEvent", raw[0].EventName);
        Assert.AreEqual(1360, raw[0].RawData.GetProperty("counter").GetInt32());
    }

    [TestMethod]
    public async Task Event_WithMatchingPendingRequestId_IsStillDispatchedAsEvent()
    {
        // Events never resolve pending requests.
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);
        TaskCompletionSource<VTubeStudioEnvelope> tcs = AddPending(client, "collision-id");

        client.DispatchMessage(ModelLoadedFrame(requestId: "collision-id"));

        Assert.AreEqual(1, raw.Count);
        Assert.AreEqual("ModelLoadedEvent", raw[0].EventName);
        Assert.IsFalse(tcs.Task.IsCompleted, "Event frame must not complete a pending request.");
    }

    [TestMethod]
    public async Task NormalResponse_WithMatchingRequestId_CompletesPendingRequest()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);
        TaskCompletionSource<VTubeStudioEnvelope> tcs = AddPending(client, "42");

        client.DispatchMessage(
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683667733,"messageType":"StatisticsResponse","requestID":"42","data":{"uptime":1439384,"framerate":73}}""");

        VTubeStudioEnvelope response = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual("StatisticsResponse", response.MessageType);
        Assert.AreEqual(0, raw.Count, "A correlated response must not surface as an event.");
    }

    [TestMethod]
    public async Task NormalResponse_WithUnknownRequestId_IsDroppedSilently()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);

        client.DispatchMessage(
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683667733,"messageType":"StatisticsResponse","requestID":"no-such-request","data":{}}""");

        Assert.AreEqual(0, raw.Count);
        Assert.AreEqual(0, client._pending.Count);
    }

    [TestMethod]
    public async Task LateResponse_AfterPendingRemoved_IsNotSurfacedAsEvent()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);
        TaskCompletionSource<VTubeStudioEnvelope> tcs = AddPending(client, "43");
        Assert.IsTrue(client._pending.TryRemove("43", out _));

        client.DispatchMessage(
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683667733,"messageType":"StatisticsResponse","requestID":"43","data":{}}""");

        Assert.AreEqual(0, raw.Count);
        Assert.IsFalse(tcs.Task.IsCompleted);
    }

    [TestMethod]
    public async Task EventSubscriptionResponse_CompletesPendingSubscriptionRequest()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);
        TaskCompletionSource<VTubeStudioEnvelope> tcs = AddPending(client, "7");

        client.DispatchMessage(
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683669792,"messageType":"EventSubscriptionResponse","requestID":"7","data":{"subscribedEventCount":2,"subscribedEvents":["TestEvent","ModelLoadedEvent"]}}""");

        VTubeStudioEnvelope response = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual("EventSubscriptionResponse", response.MessageType);
        Assert.AreEqual(0, raw.Count, "The subscription ack must not surface as an event.");
    }

    [TestMethod]
    public async Task ApiError_WithMatchingRequestId_CompletesPendingRequest()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);
        TaskCompletionSource<VTubeStudioEnvelope> tcs = AddPending(client, "9");

        client.DispatchMessage(
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683669759,"messageType":"APIError","requestID":"9","data":{"errorID":50,"message":"User has denied API access for your plugin."}}""");

        VTubeStudioEnvelope response = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual("APIError", response.MessageType);
        Assert.AreEqual(0, raw.Count);
    }

    [TestMethod]
    public async Task UnregisteredEvent_StillRaisesRawEvent_DoesNotThrow()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);

        client.DispatchMessage(ModelLoadedFrame(requestId: "some-uuid-without-handler"));

        Assert.AreEqual(1, raw.Count);
        Assert.AreEqual("ModelLoadedEvent", raw[0].EventName);
    }

    [TestMethod]
    public async Task TypedModelLoadedSubscription_ReceivesCorrectPayload()
    {
        await using VTubeStudioClient client = CreateClient();
        ModelLoadedEventPayload? received = null;
        using IDisposable _ = client.Events.On<ModelLoadedEventPayload>(p => received = p);

        client.DispatchMessage(
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683672660,"messageType":"ModelLoadedEvent","requestID":"550e8400e29b41d4a716446655440000","data":{"modelLoaded":false,"modelName":"hiyori","modelID":"03b8d2c93593474b8ee0a50e3c43316d"}}""");

        Assert.IsNotNull(received);
        Assert.IsFalse(received!.ModelLoaded);
        Assert.AreEqual("hiyori", received.ModelName);
        Assert.AreEqual("03b8d2c93593474b8ee0a50e3c43316d", received.ModelId);
    }

    [TestMethod]
    public async Task MalformedFrame_IsIgnoredWithoutThrowing()
    {
        await using VTubeStudioClient client = CreateClient();
        List<VTubeStudioEventArgs> raw = CaptureRaw(client);

        client.DispatchMessage("{not json");
        client.DispatchMessage("null");

        Assert.AreEqual(0, raw.Count);
    }

    [TestMethod]
    public void Envelope_DeserializesLiveEventFrame()
    {
        const string json =
            """{"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0","timestamp":1788683673093,"messageType":"ModelLoadedEvent","requestID":"8afcc7c8f496475d8243158a51516b21","data":{"modelLoaded":true,"modelName":"akari","modelID":"8e015e806a144842845b8b98fe3b7ec9"}}""";
        VTubeStudioEnvelope? env = JsonSerializer.Deserialize(
            json, Serialization.VTubeStudioJsonContext.Default.VTubeStudioEnvelope);

        Assert.IsNotNull(env);
        Assert.AreEqual("ModelLoadedEvent", env!.MessageType);
        Assert.AreEqual("8afcc7c8f496475d8243158a51516b21", env.RequestId);
        Assert.IsTrue(env.Data.GetProperty("modelLoaded").GetBoolean());
    }
}
