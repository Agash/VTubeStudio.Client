using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Tests for the completed protocol surface: serialization roundtrips for every
/// added record and metadata for every added event payload.
/// </summary>
[TestClass]
public sealed class ParityContractsTests
{
    private static T RoundTrip<T>(T value, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo)
    {
        string json = JsonSerializer.Serialize(value, typeInfo);
        return JsonSerializer.Deserialize(json, typeInfo)
            ?? throw new InvalidOperationException($"Roundtrip of {typeof(T).Name} produced null.");
    }

    [TestMethod]
    public void NewEventPayloads_ExposeWireNameAndTypeInfo()
    {
        Assert.AreEqual(VTubeStudioEventNames.Test, TestEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ModelOutline, ModelOutlineEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.Live2DCubismEditorConnected, Live2DCubismEditorConnectedEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ExpressionToggled, ExpressionToggledEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ArtMeshTracking, ArtMeshTrackingEventPayload.EventName);
        Assert.AreEqual(VTubeStudioEventNames.ArtMeshOutline, ArtMeshOutlineEventPayload.EventName);

        Assert.IsNotNull(TestEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ModelOutlineEventPayload.JsonTypeInfo);
        Assert.IsNotNull(Live2DCubismEditorConnectedEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ExpressionToggledEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ArtMeshTrackingEventPayload.JsonTypeInfo);
        Assert.IsNotNull(ArtMeshOutlineEventPayload.JsonTypeInfo);
    }

    [TestMethod]
    public void ModelClickedEventPayload_DeserializesObjectHits()
    {
        const string json = """
            {"modelLoaded":true,"loadedModelID":"m1","loadedModelName":"M","modelWasClicked":true,"mouseButtonID":0,"clickedArtMeshCount":1,
             "artMeshHits":[{"artMeshOrder":0,"isMasked":false,"hitInfo":{"modelID":"m1","artMeshID":"hair","angle":1.5,"size":1.0,"vertexID1":1,"vertexID2":2,"vertexID3":3,"vertexWeight1":0.5,"vertexWeight2":0.25,"vertexWeight3":0.25}}]}
            """;
        ModelClickedEventPayload? payload = JsonSerializer.Deserialize(json, VTubeStudioJsonContext.Default.ModelClickedEventPayload);

        Assert.IsNotNull(payload);
        Assert.AreEqual(1, payload!.ArtMeshHits.Count);
        Assert.AreEqual("hair", payload.ArtMeshHits[0].HitInfo?.ArtMeshId);
        Assert.AreEqual(2, payload.ArtMeshHits[0].HitInfo?.VertexId2);
    }

    [TestMethod]
    public void ParameterCreationDeletion_RoundTrip()
    {
        ParameterCreationRequest createReq = new() { ParameterName = "TestParam01", Explanation = "e", Min = -50, Max = 50, DefaultValue = 10 };
        ParameterCreationRequest createBack = RoundTrip(createReq, VTubeStudioJsonContext.Default.ParameterCreationRequest);
        Assert.AreEqual("TestParam01", createBack.ParameterName);
        Assert.AreEqual(10, createBack.DefaultValue);

        ParameterCreationResponse createResp = RoundTrip(new() { ParameterName = "TestParam01" }, VTubeStudioJsonContext.Default.ParameterCreationResponse);
        Assert.AreEqual("TestParam01", createResp.ParameterName);

        ParameterDeletionRequest deleteReq = RoundTrip(new() { ParameterName = "TestParam01" }, VTubeStudioJsonContext.Default.ParameterDeletionRequest);
        Assert.AreEqual("TestParam01", deleteReq.ParameterName);

        ParameterDeletionResponse deleteResp = RoundTrip(new() { ParameterName = "TestParam01" }, VTubeStudioJsonContext.Default.ParameterDeletionResponse);
        Assert.AreEqual("TestParam01", deleteResp.ParameterName);
    }

    [TestMethod]
    public void FolderAndPermissionRecords_RoundTrip()
    {
        VtsFolderInfoResponse folders = RoundTrip(
            new() { Models = "Live2DModels", Backgrounds = "Backgrounds", Items = "Items", Config = "Config", Logs = "Logs", Backup = "Backup" },
            VTubeStudioJsonContext.Default.VtsFolderInfoResponse);
        Assert.AreEqual("Live2DModels", folders.Models);
        Assert.AreEqual("Backup", folders.Backup);

        PermissionRequest permReq = RoundTrip(new() { RequestedPermission = "LoadCustomImagesAsItems" }, VTubeStudioJsonContext.Default.PermissionRequest);
        Assert.AreEqual("LoadCustomImagesAsItems", permReq.RequestedPermission);

        PermissionResponse permResp = RoundTrip(
            new() { GrantSuccess = true, RequestedPermission = "LoadCustomImagesAsItems", Permissions = [new PermissionInfo { Name = "LoadCustomImagesAsItems", Granted = true }] },
            VTubeStudioJsonContext.Default.PermissionResponse);
        Assert.IsTrue(permResp.GrantSuccess);
        Assert.AreEqual(1, permResp.Permissions.Count);
        Assert.IsTrue(permResp.Permissions[0].Granted);
    }

    [TestMethod]
    public void EventSubscriptionRequest_OmitsNullEventName()
    {
        string json = JsonSerializer.Serialize(new EventSubscriptionRequest { Subscribe = false }, VTubeStudioJsonContext.Default.EventSubscriptionRequest);

        StringAssert.DoesNotMatch(json, new System.Text.RegularExpressions.Regex("eventName"));
        StringAssert.Contains(json, "\"subscribe\":false");
    }

    [TestMethod]
    public void PhysicsRecords_RoundTrip()
    {
        GetCurrentModelPhysicsResponse physics = RoundTrip(
            new() { ModelLoaded = true, ModelHasPhysics = true, BaseStrength = 50, PhysicsGroups = [new PhysicsGroup { GroupId = "g1", GroupName = "Hair", StrengthMultiplier = 1.5, WindMultiplier = 0.3 }] },
            VTubeStudioJsonContext.Default.GetCurrentModelPhysicsResponse);
        Assert.AreEqual(50, physics.BaseStrength);
        Assert.AreEqual("g1", physics.PhysicsGroups[0].GroupId);

        SetCurrentModelPhysicsRequest set = RoundTrip(
            new() { StrengthOverrides = [new PhysicsOverride { Id = "g1", Value = 1.5, OverrideSeconds = 2 }] },
            VTubeStudioJsonContext.Default.SetCurrentModelPhysicsRequest);
        Assert.AreEqual("g1", set.StrengthOverrides[0].Id);
        Assert.AreEqual(0, set.WindOverrides.Count);
    }

    [TestMethod]
    public void NdiRecords_RoundTrip()
    {
        NdiConfigRequest req = RoundTrip(
            new() { SetNewConfig = true, NdiActive = true, UseNdi5 = true, UseCustomResolution = true, CustomWidthNdi = 1024, CustomHeightNdi = 512 },
            VTubeStudioJsonContext.Default.NdiConfigRequest);
        Assert.IsTrue(req.SetNewConfig);
        Assert.AreEqual(1024, req.CustomWidthNdi);

        NdiConfigResponse resp = RoundTrip(new() { NdiActive = true }, VTubeStudioJsonContext.Default.NdiConfigResponse);
        Assert.IsTrue(resp.NdiActive);
    }

    [TestMethod]
    public void PostProcessingRecords_RoundTrip()
    {
        PostProcessingListRequest listReq = RoundTrip(
            new() { FillPostProcessingEffectsArray = true, EffectIdFilter = ["ColorGrading"] },
            VTubeStudioJsonContext.Default.PostProcessingListRequest);
        Assert.AreEqual("ColorGrading", listReq.EffectIdFilter[0]);

        PostProcessingEffectConfig config = new() { EnumId = "ColorGrading_Strength", Type = "Float", ActivationConfig = true, FloatValue = 0.8 };
        PostProcessingListResponse listResp = RoundTrip(
            new() { PostProcessingActive = true, PostProcessingEffects = [new PostProcessingEffect { EnumId = "ColorGrading", ConfigEntries = [config] }] },
            VTubeStudioJsonContext.Default.PostProcessingListResponse);
        Assert.AreEqual(0.8, listResp.PostProcessingEffects[0].ConfigEntries[0].FloatValue);

        PostProcessingUpdateRequest updateReq = RoundTrip(
            new() { PostProcessingOn = true, SetPostProcessingValues = true, PostProcessingValues = [new PostProcessingValue { ConfigId = "Bloom_Strength", ConfigValue = "1.0" }] },
            VTubeStudioJsonContext.Default.PostProcessingUpdateRequest);
        Assert.AreEqual("Bloom_Strength", updateReq.PostProcessingValues[0].ConfigId);

        PostProcessingUpdateResponse updateResp = RoundTrip(new() { ActiveEffectCount = 2 }, VTubeStudioJsonContext.Default.PostProcessingUpdateResponse);
        Assert.AreEqual(2, updateResp.ActiveEffectCount);
    }

    [TestMethod]
    public void SceneOverlayRecords_RoundTrip()
    {
        SceneColorOverlayInfoResponse resp = RoundTrip(
            new() { Active = true, ColorOverlayR = 206, LeftCapturePart = new SceneColorCapturePart { Active = true, ColorR = 243 } },
            VTubeStudioJsonContext.Default.SceneColorOverlayInfoResponse);
        Assert.AreEqual(206, resp.ColorOverlayR);
        Assert.AreEqual(243, resp.LeftCapturePart?.ColorR);
    }

    [TestMethod]
    public void ItemControlRecords_RoundTrip()
    {
        ItemAnimationControlRequest animReq = RoundTrip(
            new() { ItemInstanceId = "i1", Framerate = 12, SetAnimationPlayState = true, AnimationPlayState = true },
            VTubeStudioJsonContext.Default.ItemAnimationControlRequest);
        Assert.AreEqual("i1", animReq.ItemInstanceId);
        Assert.AreEqual(12, animReq.Framerate);

        ItemAnimationControlResponse animResp = RoundTrip(new() { Frame = 3, AnimationPlaying = true }, VTubeStudioJsonContext.Default.ItemAnimationControlResponse);
        Assert.AreEqual(3, animResp.Frame);

        ItemMoveRequest moveReq = RoundTrip(
            new() { ItemsToMove = [new ItemMoveInstruction { ItemInstanceId = "i1", TimeInSeconds = 1, FadeMode = "easeOut", PositionX = 0.2 }] },
            VTubeStudioJsonContext.Default.ItemMoveRequest);
        Assert.AreEqual("easeOut", moveReq.ItemsToMove[0].FadeMode);

        ItemMoveResponse moveResp = RoundTrip(
            new() { MovedItems = [new ItemMoveResult { ItemInstanceId = "i1", Success = true, ErrorId = -1 }] },
            VTubeStudioJsonContext.Default.ItemMoveResponse);
        Assert.IsTrue(moveResp.MovedItems[0].Success);

        ItemSortRequest sortReq = RoundTrip(
            new() { ItemInstanceId = "i1", FrontOn = true, SetFrontOrder = "UseSpecialID", WithinModelOrderFront = "FullyInFront" },
            VTubeStudioJsonContext.Default.ItemSortRequest);
        Assert.AreEqual("FullyInFront", sortReq.WithinModelOrderFront);

        ItemSortResponse sortResp = RoundTrip(new() { ModelLoaded = true }, VTubeStudioJsonContext.Default.ItemSortResponse);
        Assert.IsTrue(sortResp.ModelLoaded);

        ItemPinRequest pinReq = RoundTrip(
            new() { Pin = true, ItemInstanceId = "i1", VertexPinType = "Random", PinInfo = new ItemPinInfo { Size = 0.33 } },
            VTubeStudioJsonContext.Default.ItemPinRequest);
        Assert.AreEqual("Random", pinReq.VertexPinType);
        Assert.AreEqual(0.33, pinReq.PinInfo?.Size);

        ItemPinResponse pinResp = RoundTrip(new() { IsPinned = true }, VTubeStudioJsonContext.Default.ItemPinResponse);
        Assert.IsTrue(pinResp.IsPinned);

        ItemUnloadResponse unloadResp = RoundTrip(
            new() { UnloadedItems = [new UnloadedItem { InstanceId = "i1", FileName = "a.png" }] },
            VTubeStudioJsonContext.Default.ItemUnloadResponse);
        Assert.AreEqual("a.png", unloadResp.UnloadedItems[0].FileName);
    }

    [TestMethod]
    public void ArtMeshRecords_RoundTrip()
    {
        ArtMeshListResponse list = RoundTrip(
            new() { ArtMeshGroups = [new ArtMeshGroup { GroupId = "g1", ArtMeshNames = ["a", "b"] }] },
            VTubeStudioJsonContext.Default.ArtMeshListResponse);
        Assert.AreEqual("g1", list.ArtMeshGroups[0].GroupId);
        Assert.AreEqual(2, list.ArtMeshGroups[0].ArtMeshNames.Count);

        ColorTintResponse tint = RoundTrip(new() { MatchedArtMeshes = 3 }, VTubeStudioJsonContext.Default.ColorTintResponse);
        Assert.AreEqual(3, tint.MatchedArtMeshes);

        ArtMeshSelectionRequest selReq = RoundTrip(
            new() { RequestedArtMeshCount = 2, ActiveArtMeshes = ["A"] },
            VTubeStudioJsonContext.Default.ArtMeshSelectionRequest);
        Assert.AreEqual(2, selReq.RequestedArtMeshCount);

        ArtMeshSelectionResponse selResp = RoundTrip(new() { Success = true }, VTubeStudioJsonContext.Default.ArtMeshSelectionResponse);
        Assert.IsTrue(selResp.Success);

        ArtMeshAtPositionRequest posReq = RoundTrip(
            new() { X = 0.3, Y = -0.67 },
            VTubeStudioJsonContext.Default.ArtMeshAtPositionRequest);
        Assert.AreEqual(0.3, posReq.X);

        ArtMeshAtPositionResponse posResp = RoundTrip(
            new() { ModelWasHit = true, ArtMeshHits = [new ArtMeshHit { ArtMeshOrder = 0, HitInfo = new ArtMeshHitInfo { ArtMeshId = "hair" } }] },
            VTubeStudioJsonContext.Default.ArtMeshAtPositionResponse);
        Assert.AreEqual("hair", posResp.ArtMeshHits[0].HitInfo?.ArtMeshId);
    }

    [TestMethod]
    public void CorrectedModels_RoundTrip()
    {
        CurrentModelResponse current = RoundTrip(
            new() { ModelLoaded = true, ModelPosition = new ModelPosition { PositionX = 0.1, Rotation = 9 } },
            VTubeStudioJsonContext.Default.CurrentModelResponse);
        Assert.AreEqual(0.1, current.ModelPosition?.PositionX);

        ItemListResponse items = RoundTrip(
            new() { CanLoadItemsRightNow = true, ItemInstancesInScene = [new ItemInstance { FileName = "a.png", InstanceId = "i1", PinnedToModel = true, FrameCount = 7 }] },
            VTubeStudioJsonContext.Default.ItemListResponse);
        Assert.IsTrue(items.CanLoadItemsRightNow);
        Assert.AreEqual(7, items.ItemInstancesInScene[0].FrameCount);

        ItemLoadRequest loadReq = RoundTrip(new() { FileName = "a.png" }, VTubeStudioJsonContext.Default.ItemLoadRequest);
        Assert.IsNull(loadReq.CustomDataBase64);

        ExpressionStateResponse expressions = RoundTrip(
            new() { Expressions = [new ExpressionInfo { Name = "e", File = "e.exp3.json", SecondsSinceLastActive = 1.5, UsedInHotkeys = [new ExpressionHotkey { Name = "h" }], Parameters = [new ExpressionParameter { Name = "p", Value = 1 }] }] },
            VTubeStudioJsonContext.Default.ExpressionStateResponse);
        Assert.AreEqual("h", expressions.Expressions[0].UsedInHotkeys[0].Name);
        Assert.AreEqual(1, expressions.Expressions[0].Parameters[0].Value);
        Assert.AreEqual(1.5, expressions.Expressions[0].SecondsSinceLastActive);
    }

    [TestMethod]
    public void BetaConfigAndOutline_RoundTrip()
    {
        ArtMeshTrackingEventConfig trackingConfig = RoundTrip(
            new()
            {
                Frequency = 30,
                TrackingPoints =
                [
                    new ArtMeshTrackingPoint
                    {
                        TrackingPointId = "p",
                        ArtMeshCoords = new ArtMeshTrackingCoords { ModelId = "m", ArtMeshId = "a", VertexId1 = 1, VertexWeight1 = 1.0 },
                    },
                ],
            },
            VTubeStudioJsonContext.Default.ArtMeshTrackingEventConfig);
        Assert.AreEqual("p", trackingConfig.TrackingPoints[0].TrackingPointId);
        Assert.AreEqual("a", trackingConfig.TrackingPoints[0].ArtMeshCoords?.ArtMeshId);

        ArtMeshOutlineEventConfig outlineConfig = RoundTrip(
            new() { Frequency = 15, ArtMeshes = [new ArtMeshOutlineEntry { ModelId = "m", ArtMeshId = "a" }] },
            VTubeStudioJsonContext.Default.ArtMeshOutlineEventConfig);
        Assert.AreEqual("a", outlineConfig.ArtMeshes[0].ArtMeshId);

        ModelOutlineEventPayload outline = RoundTrip(
            new() { ConvexHull = [new ClickPosition { X = 0.1, Y = 0.2 }] },
            VTubeStudioJsonContext.Default.ModelOutlineEventPayload);
        Assert.AreEqual(0.1, outline.ConvexHull[0].X);
    }

    [TestMethod]
    public void BetaEventPayloads_DeserializeDocumentedShapes()
    {
        const string toggled = """{"modelID":"m1","modelName":"M","isLive2DItem":false,"itemInstanceID":"","justLoaded":false,"expressionFile":"e.exp3.json","expressionName":"e","active":true}""";
        ExpressionToggledEventPayload? toggledPayload = JsonSerializer.Deserialize(toggled, VTubeStudioJsonContext.Default.ExpressionToggledEventPayload);
        Assert.IsNotNull(toggledPayload);
        Assert.IsTrue(toggledPayload!.Active);

        const string tracking = """{"modelLoaded":true,"modelID":"m1","subscribedPointsCount":1,"foundPointsCount":1,"eventCounter":5,"trackingPoints":[{"trackingPointID":"p","artMeshVisible":true,"position":{"x":0.1,"y":0.2},"rotation":90.0,"size":0.05}]}""";
        ArtMeshTrackingEventPayload? trackingPayload = JsonSerializer.Deserialize(tracking, VTubeStudioJsonContext.Default.ArtMeshTrackingEventPayload);
        Assert.IsNotNull(trackingPayload);
        Assert.AreEqual("p", trackingPayload!.TrackingPoints[0].TrackingPointId);
        Assert.AreEqual(0.1, trackingPayload.TrackingPoints[0].Position?.X);

        const string outline = """{"modelLoaded":true,"modelID":"m1","subscribedArtMeshCount":1,"foundArtMeshCount":1,"eventCounter":9,"artMeshOutlines":[{"artMeshID":"a","artMeshVisible":true,"outlineCount":1,"outlineArea":0.08,"outlinePoints":[{"points":[0.1,0.2]}]}]}""";
        ArtMeshOutlineEventPayload? outlinePayload = JsonSerializer.Deserialize(outline, VTubeStudioJsonContext.Default.ArtMeshOutlineEventPayload);
        Assert.IsNotNull(outlinePayload);
        Assert.AreEqual(0.1, outlinePayload!.ArtMeshOutlines[0].OutlinePoints[0].Points[0]);
    }

    [TestMethod]
    public async Task NewMethods_RequireConnection()
    {
        await using VTubeStudioClient client = new(new VTubeStudioClientOptions
        {
            PluginName = "Plugin",
            PluginDeveloper = "Dev",
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.GetVtsFolderInfoAsync());
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.RequestPermissionAsync());
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.CreateParameterAsync(new ParameterCreationRequest { ParameterName = "P" }));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.DeleteParameterAsync(new ParameterDeletionRequest { ParameterName = "P" }));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.GetSceneColorOverlayInfoAsync());
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.GetCurrentModelPhysicsAsync());
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.MoveItemsAsync(new ItemMoveRequest { ItemsToMove = [] }));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.UnsubscribeFromAllEventsAsync());
    }
}
