// VTubeStudio.Client sample.
//
// A complete walk-through of every public API surface in the client, structured as an
// interactive Spectre.Console menu so you can see each capability live against a running
// VTube Studio instance.
//
// Comments throughout call out DX choices - places where the surface is intentionally
// verbose (because the underlying protocol is) vs. places where it could be terser if a
// future convenience overload were added.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using VTubeStudio.Client;
using VTubeStudio.Client.DependencyInjection;
using VTubeStudio.Client.Errors;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;

// ── Composition root ──────────────────────────────────────────────────────────
// Mirrors what a real plugin/app would do: register the client with DI, resolve
// it, and own its lifetime. The library is DI-friendly but also fully usable
// without it (just `new VTubeStudioClient(options)`).
ServiceCollection services = new();
services.AddVTubeStudioClient(opt =>
{
    opt.PluginName = "VTubeStudio.Client Sample";
    opt.PluginDeveloper = "Agash";
});
ServiceProvider sp = services.BuildServiceProvider();
await using VTubeStudioClient client = sp.GetRequiredService<VTubeStudioClient>();

AnsiConsole.Write(new FigletText("VTS Client").Color(Color.Cyan1));
AnsiConsole.MarkupLine("[grey]Demonstrates every public API in the library.[/]\n");

// ── Connect ───────────────────────────────────────────────────────────────────
await AnsiConsole.Status().StartAsync(
    "Connecting to VTube Studio...",
    async _ => await client.ConnectAsync());
AnsiConsole.MarkupLine($"[green]✓[/] connected to [cyan]{VTubeStudioApi.DefaultEndpoint}[/]");

// ── Authenticate ──────────────────────────────────────────────────────────────
// First run: the lib asks VTS for a fresh token; the user approves a popup in
// VTube Studio; the lib returns the token. Persist it (any secret store works -
// here we just write a file next to the binary). Subsequent runs pass the
// stored token; if it's been invalidated the lib re-requests automatically.
//
// DX note: this is one of the most painful flows in any VTS client (normally
// 30+ lines of correlation work). `RequestAndAuthenticateAsync` does it in one
// line and handles every edge case for you.
string tokenPath = Path.Combine(AppContext.BaseDirectory, "vts.token");
string? storedToken = File.Exists(tokenPath) ? await File.ReadAllTextAsync(tokenPath) : null;
string token;
try
{
    AnsiConsole.MarkupLine(storedToken is null
        ? "[yellow]No stored token - VTube Studio will prompt you to approve this plugin.[/]"
        : "[grey]Re-authenticating with stored token...[/]");
    token = await client.RequestAndAuthenticateAsync(storedToken);
    if (storedToken != token)
    {
        await File.WriteAllTextAsync(tokenPath, token);
        AnsiConsole.MarkupLine($"[green]✓[/] new token persisted ({tokenPath})");
    }
    else
    {
        AnsiConsole.MarkupLine("[green]✓[/] authenticated with stored token");
    }
}
catch (VTubeStudioApiException ex)
{
    AnsiConsole.MarkupLine($"[red]✗ authentication failed:[/] {ex.Message}");
    return 1;
}

await RenderOverviewAsync(client);

// ── Subscribe to every well-known event ───────────────────────────────────────
// DX win: with IVTubeStudioEvent<TSelf>, every typed payload carries its own
// event name AND its source-generated JsonTypeInfo. So `On<T>(handler)` and
// `SubscribeAsync<T>()` need no extra ceremony - the type parameter is enough.
List<IDisposable> eventSubs =
[
    client.Events.On<HotkeyTriggeredEventPayload>(e => Log($"[cyan]hotkey[/] {e.HotkeyName} [grey]({(e.HotkeyTriggeredByApi ? "by us" : "manual")})[/]")),
    client.Events.On<ModelLoadedEventPayload>(e => Log($"[magenta]model[/] {e.ModelName} {(e.ModelLoaded ? "loaded" : "unloaded")}")),
    client.Events.On<TrackingStatusChangedEventPayload>(e => Log($"[yellow]tracking[/] face={e.FaceFound} L={e.LeftHandFound} R={e.RightHandFound}")),
    client.Events.On<ItemEventPayload>(e => Log($"[green]item[/] {e.ItemEventType} {e.ItemFileName}")),
    client.Events.On<BackgroundChangedEventPayload>(e => Log($"[blue]background[/] {e.BackgroundName}")),
];

_ = await client.SubscribeAsync<HotkeyTriggeredEventPayload>();
_ = await client.SubscribeAsync<ModelLoadedEventPayload>();
_ = await client.SubscribeAsync<TrackingStatusChangedEventPayload>();
_ = await client.SubscribeAsync<ItemEventPayload>();
_ = await client.SubscribeAsync<BackgroundChangedEventPayload>();

// ── Interactive menu ──────────────────────────────────────────────────────────
using CancellationTokenSource shutdown = new();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; shutdown.Cancel(); };
try
{
    while (!shutdown.IsCancellationRequested)
    {
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]choose a demo[/] [grey](Ctrl+C to quit)[/]")
                .AddChoices(
                    "🎭  swap to a different model",
                    "🎬  trigger a random hotkey",
                    "😊  cycle through expressions",
                    "🎩  drop an item (auto-unloads after 5s)",
                    "🌈  color-tint the model (cycle hues on a region)",
                    "🌀  orbit the model around the canvas",
                    "📈  inject a sine-wave into a tracking parameter",
                    "📡  watch live events for 30s",
                    "🔁  refresh overview",
                    "🚪  quit"));

        try
        {
            switch (choice)
            {
                case var s when s.StartsWith("🎭", StringComparison.Ordinal): await SwapModelAsync(client, shutdown.Token); break;
                case var s when s.StartsWith("🎬", StringComparison.Ordinal): await TriggerRandomHotkeyAsync(client, shutdown.Token); break;
                case var s when s.StartsWith("😊", StringComparison.Ordinal): await CycleExpressionsAsync(client, shutdown.Token); break;
                case var s when s.StartsWith("🎩", StringComparison.Ordinal): await DropItemAsync(client, shutdown.Token); break;
                case var s when s.StartsWith("🌈", StringComparison.Ordinal): await ColorCycleAsync(client, shutdown.Token); break;
                case var s when s.StartsWith("🌀", StringComparison.Ordinal): await OrbitModelAsync(client, shutdown.Token); break;
                case var s when s.StartsWith("📈", StringComparison.Ordinal): await InjectSineAsync(client, shutdown.Token); break;
                case var s when s.StartsWith("📡", StringComparison.Ordinal): await WatchEventsAsync(shutdown.Token); break;
                case var s when s.StartsWith("🔁", StringComparison.Ordinal): await RenderOverviewAsync(client); break;
                case var s when s.StartsWith("🚪", StringComparison.Ordinal): shutdown.Cancel(); break;
            }
        }
        catch (VTubeStudioApiException ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ VTS error:[/] {ex.Message} [grey](errorId {ex.ErrorIdRaw})[/]");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            AnsiConsole.MarkupLine($"[red]✗ unexpected:[/] {ex.GetType().Name}: {ex.Message}");
        }
    }
}
catch (OperationCanceledException) { /* shutdown */ }

foreach (IDisposable sub in eventSubs) sub.Dispose();
AnsiConsole.MarkupLine("[grey]disconnecting...[/]");
return 0;

// ── Helpers ──────────────────────────────────────────────────────────────────

static void Log(string markup) => AnsiConsole.MarkupLine($"[grey]{DateTimeOffset.Now:HH:mm:ss}[/]  {markup}");

static async Task RenderOverviewAsync(VTubeStudioClient client)
{
    ApiStateResponse state = await client.GetApiStateAsync();
    StatisticsResponse stats = await client.GetStatisticsAsync();
    CurrentModelResponse current = await client.GetCurrentModelAsync();

    Tree tree = new($"[bold]VTube Studio[/] [grey]v{state.VTubeStudioVersion}[/]");
    _ = tree.AddNode($"uptime [yellow]{TimeSpan.FromMilliseconds(stats.Uptime):g}[/]");
    _ = tree.AddNode($"framerate [yellow]{stats.Framerate} fps[/]");
    _ = tree.AddNode($"plugins connected: [yellow]{stats.ConnectedPlugins}/{stats.AllowedPlugins}[/]");
    _ = tree.AddNode($"window: [yellow]{stats.WindowWidth}×{stats.WindowHeight}[/]{(stats.WindowIsFullscreen ? " (fullscreen)" : string.Empty)}");
    TreeNode model = tree.AddNode("model");
    if (current.ModelLoaded)
    {
        _ = model.AddNode($"name: [cyan]{current.ModelName}[/] [grey]({current.ModelId})[/]");
        _ = model.AddNode($"live2d parameters: [yellow]{current.NumberOfLive2DParameters}[/]");
        _ = model.AddNode($"art-meshes: [yellow]{current.NumberOfLive2DArtmeshes}[/]");
        _ = model.AddNode($"textures: [yellow]{current.NumberOfTextures}[/] @ [yellow]{current.TextureResolution}px[/]");
    }
    else
    {
        _ = model.AddNode("[red]no model loaded[/]");
    }
    AnsiConsole.Write(tree);
}

static async Task SwapModelAsync(VTubeStudioClient client, CancellationToken ct)
{
    AvailableModelsResponse models = await client.GetAvailableModelsAsync(ct);
    if (models.AvailableModels.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No models available in this VTS instance.[/]");
        return;
    }

    // Prefer to swap to a model that isn't currently loaded; fall back to any.
    AvailableModel pick = models.AvailableModels.FirstOrDefault(m => !m.ModelLoaded)
        ?? models.AvailableModels[0];

    AnsiConsole.MarkupLine($"[grey]loading[/] [cyan]{pick.ModelName}[/]...");
    // DX note: VTS has a 2-second cooldown between model loads. The lib surfaces
    // the protocol error verbatim if you exceed it (VTubeStudioApiException with
    // ErrorId == ModelLoadCooldownNotOver).
    _ = await client.LoadModelAsync(new ModelLoadRequest { ModelId = pick.ModelId }, ct);
    AnsiConsole.MarkupLine($"[green]✓[/] requested swap to [cyan]{pick.ModelName}[/]");
}

static async Task TriggerRandomHotkeyAsync(VTubeStudioClient client, CancellationToken ct)
{
    HotkeysInCurrentModelResponse hotkeys = await client.GetHotkeysAsync(ct: ct);
    if (hotkeys.AvailableHotkeys.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No hotkeys on the current model.[/]");
        return;
    }

    AvailableHotkey hk = hotkeys.AvailableHotkeys[Random.Shared.Next(hotkeys.AvailableHotkeys.Count)];
    AnsiConsole.MarkupLine($"[grey]triggering[/] [cyan]{hk.Name}[/] [grey]({hk.Type})[/]...");
    _ = await client.TriggerHotkeyAsync(new HotkeyTriggerRequest { HotkeyId = hk.HotkeyId }, ct);
    AnsiConsole.MarkupLine("[green]✓[/]");
}

static async Task CycleExpressionsAsync(VTubeStudioClient client, CancellationToken ct)
{
    ExpressionStateResponse expressions = await client.GetExpressionStateAsync(ct: ct);
    if (expressions.Expressions.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]The current model has no expressions.[/]");
        return;
    }

    foreach (ExpressionInfo exp in expressions.Expressions.Take(3))
    {
        AnsiConsole.MarkupLine($"[grey]activating[/] [cyan]{exp.Name}[/]");
        await client.SetExpressionAsync(new ExpressionActivationRequest { ExpressionFile = exp.File, Active = true }, ct);
        await Task.Delay(TimeSpan.FromMilliseconds(800), ct);
        await client.SetExpressionAsync(new ExpressionActivationRequest { ExpressionFile = exp.File, Active = false }, ct);
        await Task.Delay(TimeSpan.FromMilliseconds(200), ct);
    }
    AnsiConsole.MarkupLine("[green]✓[/] cycle complete");
}

static async Task DropItemAsync(VTubeStudioClient client, CancellationToken ct)
{
    ItemListResponse items = await client.GetItemListAsync(
        new ItemListRequest { IncludeAvailableItemFiles = true, IncludeItemInstancesInScene = false },
        ct);

    AvailableItemFile? file = items.AvailableItemFiles.Count > 0 ? items.AvailableItemFiles[0] : null;
    if (file is null)
    {
        AnsiConsole.MarkupLine("[yellow]No item files available in this VTS instance.[/]");
        return;
    }

    AnsiConsole.MarkupLine($"[grey]loading item[/] [cyan]{file.FileName}[/]...");
    ItemLoadResponse loaded = await client.LoadItemAsync(new ItemLoadRequest
    {
        FileName = file.FileName,
        PositionX = 0,
        PositionY = 0,
        Size = 0.5,
        Rotation = 0,
        FadeTime = 0.5,
        UnloadWhenPluginDisconnects = true,
    }, ct);
    AnsiConsole.MarkupLine($"[green]✓[/] loaded as instance [grey]{loaded.InstanceId}[/], unloading in 5s...");
    await Task.Delay(TimeSpan.FromSeconds(5), ct);

    _ = await client.UnloadItemAsync(new ItemUnloadRequest { InstanceIds = [loaded.InstanceId] }, ct);
    AnsiConsole.MarkupLine("[green]✓[/] unloaded");
}

static async Task ColorCycleAsync(VTubeStudioClient client, CancellationToken ct)
{
    ArtMeshListResponse meshes = await client.GetArtMeshListAsync(ct);
    if (meshes.NumberOfArtMeshNames == 0)
    {
        AnsiConsole.MarkupLine("[yellow]The current model exposes no ArtMesh names - can't tint.[/]");
        return;
    }

    // Pick a tag or name fragment likely to match the face region. If the model
    // doesn't have those, fall back to tinting everything (TintAll = true).
    //
    // DX note: ArtMeshMatcher is intentionally verbose - VTS's matcher API has
    // six independent dimensions (numbers / names exact / names contains / tags
    // exact / tags contains / tint-all). A "tint by single name" convenience would
    // be a leaky simplification.
    bool hasFaceTag = meshes.ArtMeshTags.Any(t => t.Contains("face", StringComparison.OrdinalIgnoreCase));
    bool hasFaceName = meshes.ArtMeshNames.Any(n => n.Contains("face", StringComparison.OrdinalIgnoreCase));

    ArtMeshMatcher matcher = (hasFaceTag, hasFaceName) switch
    {
        (true, _) => new ArtMeshMatcher { TagContains = ["face"] },
        (_, true) => new ArtMeshMatcher { NameContains = ["face"] },
        _ => new ArtMeshMatcher { TintAll = true },
    };

    AnsiConsole.MarkupLine("[grey]cycling tints...[/]");
    (byte, byte, byte)[] palette =
    [
        (255, 100, 100), // red
        (255, 200, 100), // orange
        (255, 255, 100), // yellow
        (100, 255, 100), // green
        (100, 200, 255), // blue
        (200, 100, 255), // violet
        (255, 255, 255), // reset to white
    ];

    foreach ((byte r, byte g, byte b) in palette)
    {
        await client.TintArtMeshAsync(new ColorTintRequest
        {
            ColorTint = new ColorTint { ColorR = r, ColorG = g, ColorB = b, ColorA = 255, MixWithSceneLightingColor = 1 },
            ArtMeshMatcher = matcher,
        }, ct);
        await Task.Delay(TimeSpan.FromMilliseconds(350), ct);
    }
    AnsiConsole.MarkupLine("[green]✓[/] reset to white");
}

static async Task OrbitModelAsync(VTubeStudioClient client, CancellationToken ct)
{
    AnsiConsole.MarkupLine("[grey]orbiting model around (0,0) over 4 seconds...[/]");
    Stopwatch sw = Stopwatch.StartNew();
    int steps = 24;
    double radius = 0.3;
    for (int i = 0; i <= steps; i++)
    {
        double angle = i / (double)steps * Math.Tau;
        double x = Math.Cos(angle) * radius;
        double y = Math.Sin(angle) * radius;
        // Each leg is interpolated server-side by VTS - TimeInSeconds is the
        // tween duration for this single hop. The lib doesn't smooth client-side;
        // that decision lives with the caller.
        await client.MoveModelAsync(new MoveModelRequest
        {
            TimeInSeconds = 4d / steps,
            ValuesAreRelativeToModel = false,
            PositionX = x,
            PositionY = y,
        }, ct);
        await Task.Delay(TimeSpan.FromMilliseconds(150), ct);
    }
    await client.MoveModelAsync(new MoveModelRequest
    {
        TimeInSeconds = 0.5,
        ValuesAreRelativeToModel = false,
        PositionX = 0,
        PositionY = 0,
    }, ct);
    AnsiConsole.MarkupLine($"[green]✓[/] orbit complete in {sw.ElapsedMilliseconds} ms");
}

static async Task InjectSineAsync(VTubeStudioClient client, CancellationToken ct)
{
    // FaceAngleX is a built-in VTS tracking parameter present on every model.
    // VTS requires injected parameters to be re-sent ≥ 1×/s to retain ownership;
    // we send at 30 Hz for 6 seconds.
    const string paramName = "FaceAngleX";
    AnsiConsole.MarkupLine($"[grey]injecting a sine wave into[/] [cyan]{paramName}[/] [grey]for 6s...[/]");
    DateTime start = DateTime.UtcNow;
    while ((DateTime.UtcNow - start).TotalSeconds < 6)
    {
        double t = (DateTime.UtcNow - start).TotalSeconds;
        double value = Math.Sin(t * Math.Tau / 2d) * 30d;   // ±30° over a 2-second period
        await client.InjectParameterDataAsync(new InjectParameterDataRequest
        {
            FaceFound = true,
            Mode = "set",
            ParameterValues = [new ParameterValue { Id = paramName, Value = value, Weight = 0.8 }],
        }, ct);
        await Task.Delay(TimeSpan.FromMilliseconds(33), ct);
    }
    AnsiConsole.MarkupLine("[green]✓[/] sine complete");
}

static async Task WatchEventsAsync(CancellationToken ct)
{
    AnsiConsole.MarkupLine("[grey]watching events for 30s (subscriptions already live)...[/]");
    using CancellationTokenSource limited = CancellationTokenSource.CreateLinkedTokenSource(ct);
    limited.CancelAfter(TimeSpan.FromSeconds(30));
    try { await Task.Delay(Timeout.InfiniteTimeSpan, limited.Token); }
    catch (OperationCanceledException) { /* expected */ }
    AnsiConsole.MarkupLine("[green]✓[/] watch ended");
}
