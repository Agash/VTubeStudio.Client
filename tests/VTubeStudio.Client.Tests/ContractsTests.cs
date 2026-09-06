using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Errors;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Tests for message record defaults, event payload metadata, errors and
/// client validation. Covers the paths that need no live connection.
/// </summary>
[TestClass]
public sealed class ContractsTests
{
    [TestMethod]
    public void ResponseRecords_DefaultToEmptyCollections()
    {
        Assert.AreEqual(0, new ArtMeshListResponse().ArtMeshNames.Count);
        Assert.AreEqual(0, new ArtMeshListResponse().ArtMeshTags.Count);
        Assert.AreEqual(0, new ExpressionStateResponse().Expressions.Count);
        Assert.AreEqual(0, new HotkeysInCurrentModelResponse().AvailableHotkeys.Count);
        Assert.AreEqual(0, new ItemListResponse().AvailableSpots.Count);
        Assert.AreEqual(0, new ItemListResponse().ItemInstancesInScene.Count);
        Assert.AreEqual(0, new ItemListResponse().AvailableItemFiles.Count);
        Assert.AreEqual(0, new ItemUnloadResponse().UnloadedItems.Count);
        Assert.AreEqual(0, new AvailableModelsResponse().AvailableModels.Count);
        Assert.AreEqual(0, new InputParameterListResponse().CustomParameters.Count);
        Assert.AreEqual(0, new InputParameterListResponse().DefaultParameters.Count);
        Assert.AreEqual(0, new Live2DParameterListResponse().Parameters.Count);
        Assert.AreEqual(0, new EventSubscriptionResponse().SubscribedEvents.Count);
        Assert.AreEqual(0, new EventSubscriptionResponse().SubscribedEventCount);
        Assert.AreEqual(0, new ModelClickedEventPayload().ArtMeshHits.Count);
    }

    [TestMethod]
    public void RequestRecords_HaveDocumentedDefaults()
    {
        Assert.IsTrue(new ItemListRequest().IncludeItemInstancesInScene);
        Assert.AreEqual(0, new ItemUnloadRequest().InstanceIds.Count);
        Assert.AreEqual(0, new ItemUnloadRequest().FileNames.Count);
        Assert.AreEqual("set", new InjectParameterDataRequest { ParameterValues = [] }.Mode);
        Assert.AreEqual(0, new AvailableHotkey { Name = "n", Type = "t", HotkeyId = "id" }.KeyCombination.Count);
    }

    [TestMethod]
    public void EventPayloads_ExposeWireNameAndTypeInfo()
    {
        Assert.AreEqual(VTubeStudioEventNames.ModelLoaded, ModelLoadedEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.TrackingStatusChanged, TrackingStatusChangedEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.BackgroundChanged, BackgroundChangedEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ModelConfigChanged, ModelConfigChangedEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ModelMoved, ModelMovedEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.HotkeyTriggered, HotkeyTriggeredEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ModelAnimation, ModelAnimationEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.Item, ItemEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ModelClicked, ModelClickedEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.PostProcessing, PostProcessingEventPayload.EventName);

        Assert.IsNotNull(ModelLoadedEventPayload.JsonTypeInfo);
        Assert.IsNotNull(TrackingStatusChangedEventPayload.JsonTypeInfo);
        Assert.IsNotNull(BackgroundChangedEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ModelConfigChangedEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ModelMovedEventPayload.JsonTypeInfo);
        Assert.IsNotNull(HotkeyTriggeredEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ModelAnimationEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ItemEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ModelClickedEventPayload.JsonTypeInfo);
        Assert.IsNotNull(PostProcessingEventPayload.JsonTypeInfo);
    }

    [TestMethod]
    public void ApiException_PreservesIdsAndMessage()
    {
        VTubeStudioApiException ex = new(VTubeStudioErrorId.AuthenticationTokenInvalid, 50, "Denied");

        Assert.AreEqual(VTubeStudioErrorId.AuthenticationTokenInvalid, ex.ErrorId);
        Assert.AreEqual(50, ex.ErrorIdRaw);
        Assert.AreEqual("Denied", ex.ApiMessage);
        Assert.AreEqual("[50] Denied", ex.Message);
    }

    [TestMethod]
    public void Client_RejectsInvalidOptions()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new VTubeStudioClient(null!));
        Assert.ThrowsExactly<ArgumentException>(() => _ = new VTubeStudioClient(new VTubeStudioClientOptions
        {
            PluginName = string.Empty,
            PluginDeveloper = "Dev",
        }));
        Assert.ThrowsExactly<ArgumentException>(() => _ = new VTubeStudioClient(new VTubeStudioClientOptions
        {
            PluginName = "Plugin",
            PluginDeveloper = "  ",
        }));
    }

    [TestMethod]
    public async Task Client_StartsDisconnectedAndDisposesIdempotently()
    {
        VTubeStudioClient client = new(new VTubeStudioClientOptions
        {
            PluginName = "Plugin",
            PluginDeveloper = "Dev",
        });
        Assert.IsFalse(client.IsConnected);

        await client.DisposeAsync();
        await client.DisposeAsync();
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => client.ConnectAsync());
    }

    [TestMethod]
    public async Task Client_RequiresConnectionForRequests()
    {
        await using VTubeStudioClient client = new(new VTubeStudioClientOptions
        {
            PluginName = "Plugin",
            PluginDeveloper = "Dev",
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.GetApiStateAsync());
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.SubscribeAsync("TestEvent"));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            client.GetParameterValueAsync(new ParameterValueRequest { Name = "FaceAngleX" }));
    }

    [TestMethod]
    public async Task Client_ValidatesArgumentsBeforeConnecting()
    {
        await using VTubeStudioClient client = new(new VTubeStudioClientOptions
        {
            PluginName = "Plugin",
            PluginDeveloper = "Dev",
        });

        Assert.ThrowsExactly<ArgumentException>(() => { _ = client.AuthenticateAsync(string.Empty); });
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            _ = client.SubscribeWithConfigAsync<TestEventConfig>("TestEvent", null!, VTubeStudioJsonContext.Default.TestEventConfig);
        });
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            _ = client.SubscribeWithConfigAsync("TestEvent", new TestEventConfig(), null!);
        });
    }
}
