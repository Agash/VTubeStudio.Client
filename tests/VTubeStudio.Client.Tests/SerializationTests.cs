using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Tests;

[TestClass]
public sealed class SerializationTests
{
    [TestMethod]
    public void Envelope_RoundTrip_PreservesProtocolFields()
    {
        VTubeStudioEnvelope envelope = new()
        {
            MessageType = VTubeStudioMessageTypes.HotkeyTriggerRequest,
            RequestId = "abc-123",
            Data = JsonElement.Parse("""{"hotkeyID":"X"}"""),
        };

        string json = JsonSerializer.Serialize(envelope, VTubeStudioJsonContext.Default.VTubeStudioEnvelope);
        VTubeStudioEnvelope? back = JsonSerializer.Deserialize(json, VTubeStudioJsonContext.Default.VTubeStudioEnvelope);

        Assert.IsNotNull(back);
        Assert.AreEqual(VTubeStudioApi.ApiName, back!.ApiName);
        Assert.AreEqual(VTubeStudioApi.ApiVersion, back.ApiVersion);
        Assert.AreEqual("HotkeyTriggerRequest", back.MessageType);
        Assert.AreEqual("abc-123", back.RequestId);
        Assert.AreEqual("X", back.Data.GetProperty("hotkeyID").GetString());
    }

    [TestMethod]
    public void HotkeyTriggeredEventPayload_DeserializesAllFields()
    {
        const string json = """
            {
              "hotkeyID": "hk-1",
              "hotkeyName": "Wave",
              "hotkeyAction": "TriggerAnimation",
              "hotkeyFile": "wave.motion3.json",
              "hotkeyTriggeredByAPI": true,
              "modelID": "m1",
              "modelName": "Default Model",
              "isLive2DItem": false
            }
            """;
        HotkeyTriggeredEventPayload? p = JsonSerializer.Deserialize(json, VTubeStudioJsonContext.Default.HotkeyTriggeredEventPayload);
        Assert.IsNotNull(p);
        Assert.AreEqual("hk-1", p!.HotkeyId);
        Assert.IsTrue(p.HotkeyTriggeredByApi);
        Assert.AreEqual("TriggerAnimation", p.HotkeyAction);
    }

    [TestMethod]
    public void TrackingStatusChangedEventPayload_DeserializesAllFields()
    {
        const string json = """{"faceFound":true,"leftHandFound":false,"rightHandFound":true}""";
        TrackingStatusChangedEventPayload? p = JsonSerializer.Deserialize(json, VTubeStudioJsonContext.Default.TrackingStatusChangedEventPayload);
        Assert.IsNotNull(p);
        Assert.IsTrue(p!.FaceFound);
        Assert.IsFalse(p.LeftHandFound);
        Assert.IsTrue(p.RightHandFound);
    }

    [TestMethod]
    public void ColorTintRequest_SerializesWithMatcherSemantics()
    {
        ColorTintRequest req = new()
        {
            ColorTint = new ColorTint { ColorR = 255, ColorG = 128, ColorB = 0, ColorA = 200 },
            ArtMeshMatcher = new ArtMeshMatcher { NameContains = ["face"] },
        };
        string json = JsonSerializer.Serialize(req, VTubeStudioJsonContext.Default.ColorTintRequest);
        StringAssert.Contains(json, "\"colorR\":255");
        StringAssert.Contains(json, "\"nameContains\":[\"face\"]");
        StringAssert.Contains(json, "\"mixWithSceneLightingColor\":1");
    }

    [TestMethod]
    public void ApiErrorData_ParsesNumericErrorId()
    {
        const string json = """{"errorID":100,"message":"User denied"}""";
        ApiErrorData? err = JsonSerializer.Deserialize(json, VTubeStudioJsonContext.Default.ApiErrorData);
        Assert.IsNotNull(err);
        Assert.AreEqual(100, err!.ErrorId);
        Assert.AreEqual("User denied", err.Message);
    }

    [TestMethod]
    public void EventSubscriptionRequest_AcceptsTypedConfig()
    {
        HotkeyTriggeredEventConfig cfg = new() { OnlyForAction = "TriggerAnimation", IgnoreHotkeysTriggeredByApi = true };
        JsonElement el = JsonSerializer.SerializeToElement(cfg, VTubeStudioJsonContext.Default.HotkeyTriggeredEventConfig);
        EventSubscriptionRequest req = new()
        {
            EventName = VTubeStudioEventNames.HotkeyTriggered,
            Subscribe = true,
            Config = el,
        };
        string json = JsonSerializer.Serialize(req, VTubeStudioJsonContext.Default.EventSubscriptionRequest);
        StringAssert.Contains(json, "\"eventName\":\"HotkeyTriggeredEvent\"");
        StringAssert.Contains(json, "\"onlyForAction\":\"TriggerAnimation\"");
        StringAssert.Contains(json, "\"ignoreHotkeysTriggeredByAPI\":true");
    }
}
