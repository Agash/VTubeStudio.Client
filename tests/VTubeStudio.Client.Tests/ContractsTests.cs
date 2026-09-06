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
        VTubeStudioApiException ex = new(VTubeStudioErrorId.TokenRequestDenied, 50, "Denied");

        Assert.AreEqual(VTubeStudioErrorId.TokenRequestDenied, ex.ErrorId);
        Assert.AreEqual(50, ex.ErrorIdRaw);
        Assert.AreEqual("Denied", ex.ApiMessage);
        Assert.AreEqual("[50] Denied", ex.Message);
    }

    [TestMethod]
    [DataRow(VTubeStudioErrorId.Unknown, -1)]
    [DataRow(VTubeStudioErrorId.InternalServerError, 0)]
    [DataRow(VTubeStudioErrorId.RequestTypeUnknown, 7)]
    [DataRow(VTubeStudioErrorId.RequestRequiresAuthentication, 8)]
    [DataRow(VTubeStudioErrorId.RequestRequiresPermission, 9)]
    [DataRow(VTubeStudioErrorId.TokenRequestDenied, 50)]
    [DataRow(VTubeStudioErrorId.TokenRequestPluginIconInvalid, 54)]
    [DataRow(VTubeStudioErrorId.AuthenticationTokenMissing, 100)]
    [DataRow(VTubeStudioErrorId.AuthenticationPluginDeveloperMissing, 102)]
    [DataRow(VTubeStudioErrorId.ModelIdMissing, 150)]
    [DataRow(VTubeStudioErrorId.CannotCurrentlyChangeModel, 154)]
    [DataRow(VTubeStudioErrorId.HotkeyQueueFull, 200)]
    [DataRow(VTubeStudioErrorId.HotkeyExecutionFailedBecauseLive2DItemsDoNotSupportThisHotkeyType, 208)]
    [DataRow(VTubeStudioErrorId.ColorTintRequestNoModelLoaded, 250)]
    [DataRow(VTubeStudioErrorId.ColorTintRequestInvalidColorValue, 252)]
    [DataRow(VTubeStudioErrorId.MoveModelRequestNoModelLoaded, 300)]
    [DataRow(VTubeStudioErrorId.MoveModelRequestValuesOutOfRange, 302)]
    [DataRow(VTubeStudioErrorId.CustomParamNameInvalid, 350)]
    [DataRow(VTubeStudioErrorId.CustomParamLimitTotalExceeded, 356)]
    [DataRow(VTubeStudioErrorId.CustomParamDeletionNameInvalid, 400)]
    [DataRow(VTubeStudioErrorId.CustomParamDeletionCannotDeleteDefaultParam, 403)]
    [DataRow(VTubeStudioErrorId.InjectDataNoDataProvided, 450)]
    [DataRow(VTubeStudioErrorId.InjectDataModeUnknown, 455)]
    [DataRow(VTubeStudioErrorId.ParameterValueRequestParameterNotFound, 500)]
    [DataRow(VTubeStudioErrorId.NdiConfigCooldownNotOver, 550)]
    [DataRow(VTubeStudioErrorId.NdiConfigResolutionInvalid, 551)]
    [DataRow(VTubeStudioErrorId.ExpressionStateRequestInvalidFilename, 600)]
    [DataRow(VTubeStudioErrorId.ExpressionStateRequestFileNotFound, 601)]
    [DataRow(VTubeStudioErrorId.ExpressionActivationRequestInvalidFilename, 650)]
    [DataRow(VTubeStudioErrorId.ExpressionActivationRequestNoModelLoaded, 652)]
    [DataRow(VTubeStudioErrorId.SetCurrentModelPhysicsRequestNoModelLoaded, 700)]
    [DataRow(VTubeStudioErrorId.SetCurrentModelPhysicsRequestDuplicatePhysicsGroupId, 706)]
    [DataRow(VTubeStudioErrorId.ItemFileNameMissing, 750)]
    [DataRow(VTubeStudioErrorId.ItemLoadLoadCooldownNotOver, 752)]
    [DataRow(VTubeStudioErrorId.ItemCustomDataLoadRequestRejectedByUser, 760)]
    [DataRow(VTubeStudioErrorId.CannotCurrentlyUnloadItem, 800)]
    [DataRow(VTubeStudioErrorId.ItemAnimationControlInstanceIdNotFound, 850)]
    [DataRow(VTubeStudioErrorId.ItemAnimationControlSimpleImageDoesNotSupportAnim, 854)]
    [DataRow(VTubeStudioErrorId.ItemMoveRequestInstanceIdNotFound, 900)]
    [DataRow(VTubeStudioErrorId.ItemMoveRequestCannotCurrentlyChangeOrder, 903)]
    [DataRow(VTubeStudioErrorId.EventSubscriptionRequestEventTypeUnknown, 950)]
    [DataRow(VTubeStudioErrorId.ArtMeshSelectionRequestNoModelLoaded, 1000)]
    [DataRow(VTubeStudioErrorId.ArtMeshSelectionRequestArtMeshIdListError, 1003)]
    [DataRow(VTubeStudioErrorId.ItemPinRequestGivenItemNotLoaded, 1050)]
    [DataRow(VTubeStudioErrorId.ItemPinRequestPinPositionInvalid, 1054)]
    [DataRow(VTubeStudioErrorId.PermissionRequestUnknownPermission, 1100)]
    [DataRow(VTubeStudioErrorId.PermissionRequestFileProblem, 1102)]
    [DataRow(VTubeStudioErrorId.PostProcessingListRequestInvalidFilter, 1150)]
    [DataRow(VTubeStudioErrorId.PostProcessingUpdateRequestCannotUpdateRightNow, 1200)]
    [DataRow(VTubeStudioErrorId.PostProcessingUpdateRequestTriedToLoadRestrictedEffect, 1206)]
    [DataRow(VTubeStudioErrorId.ItemSortRequestInstanceIdNotFound, 1250)]
    [DataRow(VTubeStudioErrorId.ItemSortRequestItemConfigWindowOpen, 1255)]
    [DataRow(VTubeStudioErrorId.EventTestEventTestMessageTooLong, 100000)]
    [DataRow(VTubeStudioErrorId.EventModelLoadedEventModelIdInvalid, 100050)]
    [DataRow(VTubeStudioErrorId.EventHotkeyTriggeredEventHotkeyActionInvalid, 100100)]
    [DataRow(VTubeStudioErrorId.EventArtMeshTrackingEventTrackingPointsInvalid, 100150)]
    [DataRow(VTubeStudioErrorId.EventArtMeshTrackingEventFrequencyInvalid, 100151)]
    [DataRow(VTubeStudioErrorId.EventArtMeshOutlineEventArtMeshesInvalid, 100200)]
    [DataRow(VTubeStudioErrorId.EventArtMeshOutlineEventFrequencyInvalid, 100201)]
    public void ErrorId_MatchesUpstreamValue(VTubeStudioErrorId id, int raw)
    {
        Assert.AreEqual(raw, (int)id);
        Assert.IsTrue(Enum.IsDefined(typeof(VTubeStudioErrorId), raw));
    }

    [TestMethod]
    public void ErrorId_CoversFullUpstreamSet()
    {
        Assert.AreEqual(126, Enum.GetValues<VTubeStudioErrorId>().Length);
    }

    [TestMethod]
    public void UnrecognizedError_PreservesRawId()
    {
        const int raw = 99999;
        Assert.IsFalse(Enum.IsDefined(typeof(VTubeStudioErrorId), raw));

        VTubeStudioErrorId mapped = Enum.IsDefined(typeof(VTubeStudioErrorId), raw)
            ? (VTubeStudioErrorId)raw
            : VTubeStudioErrorId.Unknown;
        VTubeStudioApiException ex = new(mapped, raw, "Custom");

        Assert.AreEqual(VTubeStudioErrorId.Unknown, ex.ErrorId);
        Assert.AreEqual(raw, ex.ErrorIdRaw);
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

        await client.DisconnectAsync();
        await client.DisposeAsync();
        await client.DisposeAsync();
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => client.ConnectAsync());
    }

    [TestMethod]
    public void ClientOptions_HaveDocumentedDefaults()
    {
        VTubeStudioClientOptions options = new()
        {
            PluginName = "Plugin",
            PluginDeveloper = "Dev",
        };

        Assert.AreEqual(VTubeStudioApi.DefaultEndpoint, options.Endpoint);
        Assert.AreEqual(TimeSpan.FromSeconds(10), options.RequestTimeout);
        Assert.AreEqual(TimeSpan.FromMinutes(2), options.AuthApprovalTimeout);
        Assert.AreEqual(16 * 1024, options.ReceiveBufferSize);
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
