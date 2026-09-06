using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Errors;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Live tests for events. See README.md.
/// </summary>
[TestClass]
[TestCategory("Integration")]
[DoNotParallelize]
public sealed class LiveEventTests : LiveTestBase
{
    [ClassInitialize]
    public static void ClassSetup(TestContext context) => EnsureInitialized(context);

    [ClassCleanup]
    public static void ClassTeardown() => TeardownShared();

    [TestMethod]
    public async Task AuthenticationFlows_ReuseAndRefresh()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        VTubeStudioClient client = Client;
        {
            string reused = await client.RequestAndAuthenticateAsync(LiveToken(), cts.Token);
            Assert.AreEqual(LiveToken(), reused);

            string refreshed = await client.RequestAndAuthenticateAsync("invalid-token", cts.Token);
            Assert.AreNotEqual("invalid-token", refreshed);
            Assert.IsFalse(string.IsNullOrWhiteSpace(refreshed));

            string fresh = await client.RequestAndAuthenticateAsync(null, cts.Token);
            Assert.IsFalse(string.IsNullOrWhiteSpace(fresh));
        }
    }

    [TestMethod]
    public async Task ApiErrors_MapToTypedIds()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        VTubeStudioClient client = Client;
        {
            await ExpectApiErrorAsync(151, () => client.LoadModelAsync(new ModelLoadRequest { ModelId = "xyz" }, cts.Token));
            CurrentModelResponse current = await client.GetCurrentModelAsync(cts.Token);
            await ExpectApiErrorAsync(current.ModelLoaded ? 202 : 251, () => client.TriggerHotkeyAsync(new HotkeyTriggerRequest { HotkeyId = "no-such-hotkey" }, cts.Token));
            await ExpectApiErrorAsync(401, () => client.DeleteParameterAsync(new ParameterDeletionRequest { ParameterName = "Nope1234" }, cts.Token));
            await ExpectApiErrorAsync(950, () => client.SubscribeAsync("NoSuchEvent", ct: cts.Token));
        }
    }

    private static async Task ExpectApiErrorAsync(int raw, Func<Task> call)
    {
        try
        {
            await call();
            Assert.Fail($"Expected API error {raw}.");
        }
        catch (VTubeStudioApiException ex)
        {
            Assert.AreEqual(raw, ex.ErrorIdRaw);
            Assert.AreEqual((VTubeStudioErrorId)raw, ex.ErrorId);
        }
    }

    [TestMethod]
    public async Task RequestTimeout_TimesOutAndDisconnectsCleanly()
    {
        VTubeStudioClientOptions options = new()
        {
            PluginName = "VTubeStudio.Client LiveTests",
            PluginDeveloper = "Agash",
            RequestTimeout = TimeSpan.Zero,
        };
        await using VTubeStudioClient client = new(options);
        {
            await client.ConnectAsync();
            await client.ConnectAsync();
            Assert.IsTrue(client.IsConnected);

            bool timedOut = false;
            for (int i = 0; i < 5 && !timedOut; i++)
            {
                try
                {
                    _ = await client.GetApiStateAsync();
                }
                catch (OperationCanceledException)
                {
                    timedOut = true;
                }
            }

            Assert.IsTrue(timedOut, "Expected the zero-timeout request to time out.");

            bool disconnected = false;
            client.Disconnected += (_, _) => disconnected = true;
            await client.DisconnectAsync();
            Assert.IsTrue(disconnected);
            Assert.IsFalse(client.IsConnected);
        }
    }

    [TestMethod]
    public async Task ApiState_ReportsVersion()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        VTubeStudioClient client = Client;
        {
            ApiStateResponse state = await client.GetApiStateAsync(cts.Token);
            TestContext.WriteLine($"VTube Studio version: {state.VTubeStudioVersion}, active={state.Active}, authenticated={state.CurrentSessionAuthenticated}");
            Assert.IsTrue(state.Active, "VTube Studio API is not active.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(state.VTubeStudioVersion));
            Assert.IsTrue(state.CurrentSessionAuthenticated, "Session should be authenticated after AuthenticateAsync.");
        }
    }

    [TestMethod]
    public async Task TestEvent_RawSubscriber_ReceivesTicks()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        VTubeStudioClient client = Client;
        {
            ConcurrentQueue<VTubeStudioEventArgs> seen = new();
            client.EventReceived += (_, e) =>
            {
                if (e.EventName == VTubeStudioEventNames.Test)
                {
                    seen.Enqueue(e);
                }
            };

            EventSubscriptionResponse sub = await client.SubscribeWithConfigAsync(
                VTubeStudioEventNames.Test,
                new TestEventConfig { TestMessageForEvent = "cfg" },
                VTubeStudioJsonContext.Default.TestEventConfig,
                ct: cts.Token);
            TestContext.WriteLine($"Subscribed: [{string.Join(",", sub.SubscribedEvents)}]");
            Assert.IsTrue(sub.SubscribedEvents.Contains(VTubeStudioEventNames.Test));

            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);

            Assert.IsTrue(seen.Count >= 2,
                $"Expected at least 2 TestEvent ticks in 5s, got {seen.Count}. Subscription ack: [{string.Join(",", sub.SubscribedEvents)}].");
            foreach (VTubeStudioEventArgs e in seen)
            {
                TestContext.WriteLine($"tick: data={e.RawData.GetRawText()} at={e.ReceivedAtUtc:O}");
                Assert.AreEqual("cfg", e.RawData.GetProperty("yourTestMessage").GetString());
            }

            EventSubscriptionResponse unsub = await client.SubscribeAsync(VTubeStudioEventNames.Test, subscribe: false, cts.Token);
            Assert.IsFalse(unsub.SubscribedEvents.Contains(VTubeStudioEventNames.Test));
        }
    }

    [TestMethod]
    public async Task ModelLoadedEvent_TypedAndRaw_FireOnProgrammaticSwap()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        VTubeStudioClient client = Client;
        {
            CurrentModelResponse before = await client.GetCurrentModelAsync(cts.Token);
            AvailableModelsResponse available = await client.GetAvailableModelsAsync(cts.Token);
            TestContext.WriteLine($"Current: loaded={before.ModelLoaded} name={before.ModelName} id={before.ModelId}; available={available.AvailableModels.Count}");
            AvailableModel? other = available.AvailableModels.FirstOrDefault(m => m.ModelId != before.ModelId);
            Assert.IsNotNull(other, "Need at least 2 models in VTube Studio to exercise a programmatic model swap.");

            ConcurrentQueue<ModelLoadedEventPayload> typed = new();
            ConcurrentQueue<VTubeStudioEventArgs> raw = new();
            using IDisposable _ = client.Events.On<ModelLoadedEventPayload>(typed.Enqueue);
            void OnRaw(object? sender, VTubeStudioEventArgs e)
            {
                if (e.EventName == VTubeStudioEventNames.ModelLoaded)
                {
                    raw.Enqueue(e);
                }
            }

            client.EventReceived += OnRaw;
            try
            {
                EventSubscriptionResponse sub = await client.SubscribeAsync<ModelLoadedEventPayload>(ct: cts.Token);
                TestContext.WriteLine($"Subscribed: [{string.Join(",", sub.SubscribedEvents)}]");
                Assert.IsTrue(sub.SubscribedEvents.Contains(VTubeStudioEventNames.ModelLoaded), Diagnostic("subscribe-ack", sub, typed, raw));

                ModelLoadResponse load = await client.LoadModelAsync(new ModelLoadRequest { ModelId = other!.ModelId }, cts.Token);
                TestContext.WriteLine($"Load requested -> modelID={load.ModelId}");
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);

                Assert.IsTrue(typed.Count >= 2, Diagnostic("after-load typed", sub, typed, raw));
                Assert.IsTrue(raw.Count >= 2, Diagnostic("after-load raw", sub, typed, raw));
                ModelLoadedEventPayload[] payloads = [.. typed];
                Assert.IsTrue(payloads.Any(p => !p.ModelLoaded), $"Expected an unload event. Got: {Describe(payloads)}");
                Assert.IsTrue(payloads.Any(p => p.ModelLoaded && p.ModelId == other.ModelId),
                    $"Expected a load event for {other.ModelId}. Got: {Describe(payloads)}");
                foreach (VTubeStudioEventArgs e in raw)
                {
                    TestContext.WriteLine($"raw ModelLoadedEvent: data={e.RawData.GetRawText()}");
                }
            }
            finally
            {
                client.EventReceived -= OnRaw;
                if (before.ModelLoaded && before.ModelId is not null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), cts.Token); // Cooldown between model loads.
                    try
                    {
                        await client.LoadModelAsync(new ModelLoadRequest { ModelId = before.ModelId }, cts.Token);
                        TestContext.WriteLine($"Restored model {before.ModelName}.");
                    }
                    catch (Exception ex)
                    {
                        TestContext.WriteLine($"WARNING: failed to restore model {before.ModelName}: {ex.Message}");
                    }
                }
            }
        }

        static string Describe(IEnumerable<ModelLoadedEventPayload> payloads)
        {
            return string.Join(" | ", payloads.Select(p => $"loaded={p.ModelLoaded} name={p.ModelName} id={p.ModelId}"));
        }

        static string Diagnostic(
            string phase,
            EventSubscriptionResponse sub,
            ConcurrentQueue<ModelLoadedEventPayload> typed,
            ConcurrentQueue<VTubeStudioEventArgs> raw)
        {
            return $"[{phase}] subscribed=[{string.Join(",", sub.SubscribedEvents)}] "
                + $"typed={typed.Count} ({Describe(typed)}) "
                + $"raw={raw.Count} ({string.Join(" | ", raw.Select(e => e.RawData.GetRawText()))})";
        }
    }

    [TestMethod]
    public async Task ReversibleWrites_InjectMoveItemRoundtrips()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        VTubeStudioClient client = Client;
        {
            ParameterInfo faceAngle = await client.GetParameterValueAsync(
                new ParameterValueRequest { Name = "FaceAngleX" }, cts.Token);
            TestContext.WriteLine($"FaceAngleX value={faceAngle.Value} range=[{faceAngle.Min},{faceAngle.Max}]");
            await client.InjectParameterDataAsync(new InjectParameterDataRequest
            {
                FaceFound = true,
                Mode = "set",
                ParameterValues = [new ParameterValue { Id = "FaceAngleX", Value = faceAngle.Value, Weight = 1 }],
            }, cts.Token);

            CurrentModelResponse current = await client.GetCurrentModelAsync(cts.Token);
            if (current.ModelLoaded)
            {
                await client.MoveModelAsync(new MoveModelRequest
                {
                    TimeInSeconds = 0,
                    ValuesAreRelativeToModel = true,
                }, cts.Token);
            }
            else
            {
                TestContext.WriteLine("No model loaded; skipping move step.");
            }

            ItemListResponse files = await client.GetItemListAsync(
                new ItemListRequest { IncludeAvailableItemFiles = true, IncludeItemInstancesInScene = false }, cts.Token);
            if (files.AvailableItemFiles.Count == 0)
            {
                Assert.Inconclusive("No item files available; skipping item roundtrip.");
            }

            string fileName = files.AvailableItemFiles[0].FileName;
            ItemLoadResponse loaded = await client.LoadItemAsync(new ItemLoadRequest
            {
                FileName = fileName,
                Size = 0.2,
                FadeTime = 0.2,
                UnloadWhenPluginDisconnects = true,
            }, cts.Token);
            TestContext.WriteLine($"Loaded item instance {loaded.InstanceId}.");
            try
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(loaded.InstanceId));
            }
            finally
            {
                ItemUnloadResponse unloaded = await UnloadWithRetryAsync(client, loaded.InstanceId, cts.Token);
                TestContext.WriteLine($"Unloaded {unloaded.UnloadedItems.Count} item(s).");
            }
        }
    }

    private static async Task<ItemUnloadResponse> UnloadWithRetryAsync(VTubeStudioClient client, string instanceId, CancellationToken ct)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await client.UnloadItemAsync(new ItemUnloadRequest { InstanceIds = [instanceId] }, ct);
            }
            catch (VTubeStudioApiException) when (attempt < 4)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }

    private static async Task<ModelLoadResponse> LoadModelWithCooldownRetryAsync(VTubeStudioClient client, string modelId, CancellationToken ct)
    {
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                return await client.LoadModelAsync(new ModelLoadRequest { ModelId = modelId }, ct);
            }
            catch (VTubeStudioApiException ex) when (ex.ErrorId == VTubeStudioErrorId.ModelLoadCooldownNotOver && attempt < 4)
            {
                await Task.Delay(TimeSpan.FromSeconds(2.5), ct);
            }
        }
    }

    [TestMethod]
    public async Task StableReads_FoldersScenePhysicsPostProcessingPermissions()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        VTubeStudioClient client = Client;
        {
            VtsFolderInfoResponse folders = await client.GetVtsFolderInfoAsync(cts.Token);
            TestContext.WriteLine($"folders: models={folders.Models} items={folders.Items}");
            Assert.IsFalse(string.IsNullOrWhiteSpace(folders.Models));

            SceneColorOverlayInfoResponse overlay = await client.GetSceneColorOverlayInfoAsync(cts.Token);
            TestContext.WriteLine($"overlay: active={overlay.Active}");

            GetCurrentModelPhysicsResponse physics = await client.GetCurrentModelPhysicsAsync(cts.Token);
            TestContext.WriteLine($"physics: loaded={physics.ModelLoaded} hasPhysics={physics.ModelHasPhysics} groups={physics.PhysicsGroups.Count}");

            PostProcessingListResponse post = await client.GetPostProcessingAsync(new PostProcessingListRequest(), cts.Token);
            TestContext.WriteLine($"post: supported={post.PostProcessingSupported} active={post.PostProcessingActive}");

            PostProcessingUpdateResponse updated = await client.UpdatePostProcessingAsync(new PostProcessingUpdateRequest
            {
                PostProcessingOn = post.PostProcessingActive,
                PostProcessingFadeTime = 0,
            }, cts.Token);
            Assert.AreEqual(post.PostProcessingActive, updated.PostProcessingActive);

            PermissionResponse permissions = await client.RequestPermissionAsync(timeout: TimeSpan.FromSeconds(30), ct: cts.Token);
            TestContext.WriteLine($"permissions: [{string.Join(",", permissions.Permissions.Select(p => $"{p.Name}={p.Granted}"))}]");
            Assert.IsTrue(permissions.Permissions.Count > 0);
        }
    }

    [TestMethod]
    public async Task ParameterCreateInjectDelete_Roundtrip()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        VTubeStudioClient client = Client;
        {
            const string name = "VtsClientTest1";
            try
            {
                ParameterCreationResponse created = await client.CreateParameterAsync(new ParameterCreationRequest
                {
                    ParameterName = name,
                    Explanation = "Live test parameter.",
                    Min = -50,
                    Max = 50,
                    DefaultValue = 10,
                }, cts.Token);
                Assert.AreEqual(name, created.ParameterName);

                InputParameterListResponse list = await client.GetInputParametersAsync(cts.Token);
                Assert.IsTrue(list.CustomParameters.Any(p => p.Name == name), "Created parameter missing from input list.");

                await client.InjectParameterDataAsync(new InjectParameterDataRequest
                {
                    Mode = "set",
                    ParameterValues = [new ParameterValue { Id = name, Value = 5 }],
                }, cts.Token);
            }
            finally
            {
                try
                {
                    ParameterDeletionResponse deleted = await client.DeleteParameterAsync(new ParameterDeletionRequest { ParameterName = name }, cts.Token);
                    TestContext.WriteLine($"Deleted parameter {deleted.ParameterName}.");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"WARNING: failed to delete parameter {name}: {ex.Message}");
                }
            }

            InputParameterListResponse after = await client.GetInputParametersAsync(cts.Token);
            Assert.IsFalse(after.CustomParameters.Any(p => p.Name == name), "Deleted parameter still listed.");
        }
    }

    [TestMethod]
    public async Task ItemControl_MoveAnimatePinRoundtrip()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        VTubeStudioClient client = Client;
        {
            ItemListResponse files = await client.GetItemListAsync(
                new ItemListRequest { IncludeAvailableItemFiles = true, IncludeItemInstancesInScene = false }, cts.Token);
            AvailableItemFile? file = files.AvailableItemFiles.FirstOrDefault(f => f.Type != "Live2D");
            if (file is null)
            {
                Assert.Inconclusive("No non-Live2D item files available; skipping item control roundtrip.");
            }

            ItemLoadResponse loaded = await client.LoadItemAsync(new ItemLoadRequest
            {
                FileName = file.FileName,
                Size = 0.2,
                FadeTime = 0.2,
                UnloadWhenPluginDisconnects = true,
            }, cts.Token);
            try
            {
                ItemMoveResponse moved = await client.MoveItemsAsync(new ItemMoveRequest
                {
                    ItemsToMove = [new ItemMoveInstruction { ItemInstanceId = loaded.InstanceId, TimeInSeconds = 0, PositionX = 0, PositionY = 0 }],
                }, cts.Token);
                Assert.AreEqual(1, moved.MovedItems.Count);
                Assert.IsTrue(moved.MovedItems[0].Success, $"Move failed with error {moved.MovedItems[0].ErrorId}.");

                ItemAnimationControlResponse anim = await client.ControlItemAnimationAsync(new ItemAnimationControlRequest
                {
                    ItemInstanceId = loaded.InstanceId,
                    Brightness = 1,
                    Opacity = 1,
                }, cts.Token);
                TestContext.WriteLine($"anim: frame={anim.Frame} playing={anim.AnimationPlaying}");

                ItemPinResponse pin = await client.PinItemAsync(new ItemPinRequest
                {
                    Pin = true,
                    ItemInstanceId = loaded.InstanceId,
                    AngleRelativeTo = "RelativeToModel",
                    SizeRelativeTo = "RelativeToWorld",
                    VertexPinType = "Center",
                    PinInfo = new ItemPinInfo(),
                }, cts.Token);
                Assert.IsTrue(pin.IsPinned);
                ItemPinResponse unpin = await client.PinItemAsync(new ItemPinRequest
                {
                    Pin = false,
                    ItemInstanceId = loaded.InstanceId,
                }, cts.Token);
                Assert.IsFalse(unpin.IsPinned);

                ItemSortResponse sorted = await client.SortItemAsync(new ItemSortRequest
                {
                    ItemInstanceId = loaded.InstanceId,
                }, cts.Token);
                Assert.AreEqual(loaded.InstanceId, sorted.ItemInstanceId);
            }
            finally
            {
                await UnloadWithRetryAsync(client, loaded.InstanceId, cts.Token);
            }
        }
    }

    [TestMethod]
    public async Task BetaEndpoints_AttemptOrSkip()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        VTubeStudioClient client = Client;
        {
            List<string> unavailable = [];

            await AttemptOrSkipAsync("ArtMeshAtPosition", unavailable, TestContext, async () =>
            {
                ArtMeshAtPositionResponse at = await client.GetArtMeshesAtPositionAsync(
                    new ArtMeshAtPositionRequest { X = 0, Y = 0 }, cts.Token);
                TestContext.WriteLine($"at-position: hits={at.ArtMeshHitCount}");
            });

            await AttemptOrSkipAsync("ExpressionToggled", unavailable, TestContext, async () =>
            {
                EventSubscriptionResponse sub = await client.SubscribeWithConfigAsync(
                    VTubeStudioEventNames.ExpressionToggled,
                    new ExpressionToggledEventConfig(),
                    VTubeStudioJsonContext.Default.ExpressionToggledEventConfig,
                    ct: cts.Token);
                TestContext.WriteLine($"subscribed: [{string.Join(",", sub.SubscribedEvents)}]");
                _ = await client.SubscribeAsync(VTubeStudioEventNames.ExpressionToggled, subscribe: false, cts.Token);
            });

            await AttemptOrSkipAsync("NdiConfig", unavailable, TestContext, async () =>
            {
                NdiConfigResponse ndi = await client.GetNdiConfigAsync(cts.Token);
                TestContext.WriteLine($"ndi: active={ndi.NdiActive}");
                NdiConfigResponse applied = await client.SetNdiConfigAsync(new NdiConfigRequest
                {
                    SetNewConfig = true,
                    NdiActive = ndi.NdiActive,
                    UseNdi5 = ndi.UseNdi5,
                    UseCustomResolution = ndi.UseCustomResolution,
                    CustomWidthNdi = ndi.CustomWidthNdi,
                    CustomHeightNdi = ndi.CustomHeightNdi,
                }, cts.Token);
                Assert.AreEqual(ndi.NdiActive, applied.NdiActive);
            });

            if (unavailable.Count > 0)
            {
                Assert.Inconclusive($"Unavailable on this VTube Studio build: {string.Join(", ", unavailable)}.");
            }
        }
    }

    [TestMethod]
    public async Task ModelWrites_MovePhysicsExpressionTintRoundtrip()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(4));
        VTubeStudioClient client = Client;
        {
            CurrentModelResponse before = await client.GetCurrentModelAsync(cts.Token);
            bool loadedByTest = false;
            if (!before.ModelLoaded)
            {
                AvailableModelsResponse available = await client.GetAvailableModelsAsync(cts.Token);
                if (available.AvailableModels.Count == 0)
                {
                    Assert.Inconclusive("No models available.");
                }

                _ = await LoadModelWithCooldownRetryAsync(client, available.AvailableModels[0].ModelId, cts.Token);
                await Task.Delay(TimeSpan.FromSeconds(4), cts.Token);
                loadedByTest = true;
            }

            try
            {
                await client.MoveModelAsync(new MoveModelRequest
                {
                    TimeInSeconds = 0,
                    ValuesAreRelativeToModel = true,
                }, cts.Token);

                GetCurrentModelPhysicsResponse physics = await client.GetCurrentModelPhysicsAsync(cts.Token);
                if (physics is { ModelHasPhysics: true } && physics.PhysicsGroups.Count > 0)
                {
                    PhysicsGroup group = physics.PhysicsGroups[0];
                    await client.SetCurrentModelPhysicsAsync(new SetCurrentModelPhysicsRequest
                    {
                        StrengthOverrides = [new PhysicsOverride { Id = group.GroupId, Value = group.StrengthMultiplier, OverrideSeconds = 0.5 }],
                    }, cts.Token);
                }
                else
                {
                    TestContext.WriteLine("No physics groups; skipping override.");
                }

                ExpressionStateResponse expressions = await client.GetExpressionStateAsync(ct: cts.Token);
                if (expressions.Expressions.Count > 0)
                {
                    string file = expressions.Expressions[0].File;
                    await client.SetExpressionAsync(new ExpressionActivationRequest { ExpressionFile = file, Active = true }, cts.Token);
                    await client.SetExpressionAsync(new ExpressionActivationRequest { ExpressionFile = file, Active = false }, cts.Token);
                }
                else
                {
                    TestContext.WriteLine("No expressions; skipping blink.");
                }

                ArtMeshListResponse meshes = await client.GetArtMeshListAsync(cts.Token);
                if (meshes.NumberOfArtMeshNames > 0)
                {
                    ArtMeshMatcher matcher = new() { TintAll = true };
                    _ = await client.TintArtMeshAsync(new ColorTintRequest
                    {
                        ColorTint = new ColorTint { ColorR = 255, ColorG = 200, ColorB = 200, ColorA = 255 },
                        ArtMeshMatcher = matcher,
                    }, cts.Token);
                    ColorTintResponse reset = await client.TintArtMeshAsync(new ColorTintRequest
                    {
                        ColorTint = new ColorTint { ColorR = 255, ColorG = 255, ColorB = 255, ColorA = 255 },
                        ArtMeshMatcher = matcher,
                    }, cts.Token);
                    Assert.IsTrue(reset.MatchedArtMeshes > 0);
                }
                else
                {
                    TestContext.WriteLine("No artmeshes; skipping tint.");
                }
            }
            finally
            {
                if (loadedByTest)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
                    _ = await LoadModelWithCooldownRetryAsync(client, string.Empty, cts.Token);
                }
            }
        }
    }

    [TestMethod]
    public async Task ModelInventory_ArtMeshTintAcrossAllModels()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(10));
        VTubeStudioClient client = Client;
        {
            CurrentModelResponse before = await client.GetCurrentModelAsync(cts.Token);
            AvailableModelsResponse available = await client.GetAvailableModelsAsync(cts.Token);
            if (available.AvailableModels.Count == 0)
            {
                Assert.Inconclusive("No models available.");
            }

            try
            {
                foreach (AvailableModel model in available.AvailableModels)
                {
                    _ = await LoadModelWithCooldownRetryAsync(client, model.ModelId, cts.Token);
                    await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);

                    CurrentModelResponse current = await client.GetCurrentModelAsync(cts.Token);
                    ArtMeshListResponse meshes = await client.GetArtMeshListAsync(cts.Token);
                    ExpressionStateResponse expressions = await client.GetExpressionStateAsync(ct: cts.Token);
                    GetCurrentModelPhysicsResponse physics = await client.GetCurrentModelPhysicsAsync(cts.Token);
                    TestContext.WriteLine($"{current.ModelName}: params={current.NumberOfLive2DParameters} meshes={meshes.NumberOfArtMeshNames} tags={meshes.NumberOfArtMeshTags} groups={meshes.NumberOfArtMeshGroups} expressions={expressions.Expressions.Count} hasPhysics={physics.ModelHasPhysics} physicsGroups={physics.PhysicsGroups.Count}");
                    Assert.AreEqual(model.ModelId, current.ModelId);

                    if (meshes.NumberOfArtMeshNames > 0)
                    {
                        ArtMeshMatcher matcher = new() { TintAll = true };
                        ColorTintResponse tinted = await client.TintArtMeshAsync(new ColorTintRequest
                        {
                            ColorTint = new ColorTint { ColorR = 255, ColorG = 200, ColorB = 200, ColorA = 255 },
                            ArtMeshMatcher = matcher,
                        }, cts.Token);
                        Assert.IsTrue(tinted.MatchedArtMeshes > 0, $"Tint matched nothing on {current.ModelName}.");
                        ColorTintResponse reset = await client.TintArtMeshAsync(new ColorTintRequest
                        {
                            ColorTint = new ColorTint { ColorR = 255, ColorG = 255, ColorB = 255, ColorA = 255 },
                            ArtMeshMatcher = matcher,
                        }, cts.Token);
                        Assert.IsTrue(reset.MatchedArtMeshes > 0, $"White reset matched nothing on {current.ModelName}.");
                    }
                }
            }
            finally
            {
                string restoreId = before is { ModelLoaded: true, ModelId: not null } ? before.ModelId : string.Empty;
                _ = await LoadModelWithCooldownRetryAsync(client, restoreId, cts.Token);
            }
        }
    }

    private static async Task AttemptOrSkipAsync(string name, List<string> unavailable, TestContext context, Func<Task> attempt)
    {
        try
        {
            await attempt();
        }
        catch (VTubeStudioApiException ex)
        {
            context.WriteLine($"{name} unavailable (error {ex.ErrorIdRaw}: {ex.ApiMessage}).");
            unavailable.Add($"{name} (error {ex.ErrorIdRaw})");
        }
    }

    [TestMethod]
    public async Task ModelLoadCooldown_MapsToTypedError()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        VTubeStudioClient client = Client;
        {
            CurrentModelResponse current = await client.GetCurrentModelAsync(cts.Token);
            if (!current.ModelLoaded || current.ModelId is null)
            {
                Assert.Inconclusive("No model loaded; skipping cooldown check.");
            }

            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    _ = await client.LoadModelAsync(new ModelLoadRequest { ModelId = current.ModelId }, cts.Token);
                    break;
                }
                catch (VTubeStudioApiException ex) when (ex.ErrorId == VTubeStudioErrorId.ModelLoadCooldownNotOver && attempt < 3)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2.5), cts.Token);
                }
            }

            try
            {
                _ = await client.LoadModelAsync(new ModelLoadRequest { ModelId = current.ModelId }, cts.Token);
                Assert.Inconclusive("Server did not enforce the load cooldown; mapping not verified.");
            }
            catch (VTubeStudioApiException ex)
            {
                TestContext.WriteLine($"cooldown error: id={ex.ErrorIdRaw} mapped={ex.ErrorId}");
                Assert.AreEqual(153, ex.ErrorIdRaw);
                Assert.AreEqual(VTubeStudioErrorId.ModelLoadCooldownNotOver, ex.ErrorId);
            }
        }
    }

    [TestMethod]
    public async Task RequestResponse_SanityChecks_StillWorkAlongsideEvents()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        VTubeStudioClient client = Client;
        {
            int rawEvents = 0;
            client.EventReceived += (_, _) => Interlocked.Increment(ref rawEvents);

            _ = await client.SubscribeAsync(VTubeStudioEventNames.Test, subscribe: true, cts.Token);
            StatisticsResponse stats = await client.GetStatisticsAsync(cts.Token);
            CurrentModelResponse current = await client.GetCurrentModelAsync(cts.Token);
            AvailableModelsResponse available = await client.GetAvailableModelsAsync(cts.Token);
            TestContext.WriteLine($"stats: {stats.Framerate}fps uptime={stats.Uptime}; current={current.ModelName}; models={available.AvailableModels.Count}");

            Assert.IsTrue(stats.Framerate > 0);
            Assert.IsTrue(available.AvailableModels.Count > 0);

            HotkeysInCurrentModelResponse hotkeys = await client.GetHotkeysAsync(ct: cts.Token);
            ExpressionStateResponse expressions = await client.GetExpressionStateAsync(ct: cts.Token);
            HotkeysInCurrentModelResponse hotkeysExplicit = await client.GetHotkeysAsync(new HotkeysInCurrentModelRequest(), cts.Token);
            ExpressionStateResponse expressionsDetailed = await client.GetExpressionStateAsync(new ExpressionStateRequest { Details = true }, cts.Token);
            Assert.AreEqual(hotkeys.AvailableHotkeys.Count, hotkeysExplicit.AvailableHotkeys.Count);
            Assert.IsNotNull(expressionsDetailed.Expressions);
            ArtMeshListResponse meshes = await client.GetArtMeshListAsync(cts.Token);
            InputParameterListResponse inputParams = await client.GetInputParametersAsync(cts.Token);
            Live2DParameterListResponse live2DParams = await client.GetLive2DParametersAsync(cts.Token);
            ItemListResponse items = await client.GetItemListAsync(new ItemListRequest(), cts.Token);
            FaceFoundResponse face = await client.GetFaceFoundAsync(cts.Token);
            TestContext.WriteLine($"hotkeys={hotkeys.AvailableHotkeys.Count} expressions={expressions.Expressions.Count} meshes={meshes.NumberOfArtMeshNames} input={inputParams.DefaultParameters.Count} live2d={live2DParams.Parameters.Count} items={items.ItemInstancesInScene.Count} face={face.Found}");

            Assert.IsNotNull(hotkeys.AvailableHotkeys);
            Assert.IsNotNull(expressions.Expressions);
            Assert.IsNotNull(items.ItemInstancesInScene);

            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            Assert.IsTrue(rawEvents >= 1, "Expected background TestEvent ticks while doing request/response calls.");

            _ = await client.SubscribeAsync(VTubeStudioEventNames.Test, subscribe: false, cts.Token);
        }
    }
}
