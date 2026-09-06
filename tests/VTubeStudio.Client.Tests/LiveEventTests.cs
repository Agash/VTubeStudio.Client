using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Errors;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Live tests for events. See README.md.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public sealed class LiveEventTests : LiveTestBase
{
    [TestMethod]
    public async Task ApiState_ReportsVersion()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        (VTubeStudioClient client, _) = await ConnectAndAuthenticateAsync(TestContext, cts.Token);
        await using (client)
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
        (VTubeStudioClient client, _) = await ConnectAndAuthenticateAsync(TestContext, cts.Token);
        await using (client)
        {
            ConcurrentQueue<VTubeStudioEventArgs> seen = new();
            client.EventReceived += (_, e) =>
            {
                if (e.EventName == VTubeStudioEventNames.Test)
                {
                    seen.Enqueue(e);
                }
            };

            EventSubscriptionResponse sub = await client.SubscribeAsync(VTubeStudioEventNames.Test, subscribe: true, cts.Token);
            TestContext.WriteLine($"Subscribed: [{string.Join(",", sub.SubscribedEvents)}]");
            Assert.IsTrue(sub.SubscribedEvents.Contains(VTubeStudioEventNames.Test));

            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);

            Assert.IsTrue(seen.Count >= 2,
                $"Expected at least 2 TestEvent ticks in 5s, got {seen.Count}. Subscription ack: [{string.Join(",", sub.SubscribedEvents)}].");
            foreach (VTubeStudioEventArgs e in seen)
            {
                TestContext.WriteLine($"tick: data={e.RawData.GetRawText()} at={e.ReceivedAtUtc:O}");
            }

            EventSubscriptionResponse unsub = await client.SubscribeAsync(VTubeStudioEventNames.Test, subscribe: false, cts.Token);
            Assert.IsFalse(unsub.SubscribedEvents.Contains(VTubeStudioEventNames.Test));
        }
    }

    [TestMethod]
    public async Task ModelLoadedEvent_TypedAndRaw_FireOnProgrammaticSwap()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(3));
        (VTubeStudioClient client, _) = await ConnectAndAuthenticateAsync(TestContext, cts.Token);
        await using (client)
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
        (VTubeStudioClient client, _) = await ConnectAndAuthenticateAsync(TestContext, cts.Token);
        await using (client)
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

            await client.MoveModelAsync(new MoveModelRequest
            {
                TimeInSeconds = 0,
                ValuesAreRelativeToModel = true,
            }, cts.Token);

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

    [TestMethod]
    public async Task RequestResponse_SanityChecks_StillWorkAlongsideEvents()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
        (VTubeStudioClient client, _) = await ConnectAndAuthenticateAsync(TestContext, cts.Token);
        await using (client)
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
