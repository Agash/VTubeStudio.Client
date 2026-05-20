using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;
using VTubeStudio.Client;
using VTubeStudio.Client.DependencyInjection;
using VTubeStudio.Client.Errors;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

// Compose services via DI to mirror what a real plugin/app would do.
ServiceCollection services = new();
services.AddVTubeStudioClient(opt =>
{
    opt.PluginName = "VTubeStudio.Client Sample";
    opt.PluginDeveloper = "Agash";
});
ServiceProvider sp = services.BuildServiceProvider();
await using VTubeStudioClient client = sp.GetRequiredService<VTubeStudioClient>();

AnsiConsole.Write(new FigletText("VTS Client").Color(Color.Cyan1));
AnsiConsole.MarkupLine("[grey]Demonstrates the full VTube Studio plugin workflow.[/]");

// 1. Persist the token in a local file next to the sample. Real apps would use ISecretStore.
string tokenPath = Path.Combine(AppContext.BaseDirectory, "vts.token");
string? storedToken = File.Exists(tokenPath) ? await File.ReadAllTextAsync(tokenPath) : null;

await AnsiConsole.Status()
    .StartAsync("Connecting to VTube Studio…", async _ => await client.ConnectAsync());
AnsiConsole.MarkupLine("[green]✓[/] connected");

// 2. Auth
string token;
try
{
    AnsiConsole.MarkupLine(storedToken is null
        ? "[yellow]No stored token — VTube Studio will prompt you to approve this plugin.[/]"
        : "[grey]Re-authenticating with stored token…[/]");
    token = await client.RequestAndAuthenticateAsync(storedToken);
    if (storedToken != token)
    {
        await File.WriteAllTextAsync(tokenPath, token);
        AnsiConsole.MarkupLine($"[green]✓[/] new token persisted to [italic]{tokenPath}[/]");
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

// 3. Inspect the current state of VTS via a Spectre tree.
ApiStateResponse state = await client.GetApiStateAsync();
StatisticsResponse stats = await client.GetStatisticsAsync();
CurrentModelResponse current = await client.GetCurrentModelAsync();

Tree tree = new($"[bold]VTube Studio[/] [grey]v{state.VTubeStudioVersion}[/]");
tree.AddNode($"uptime [yellow]{TimeSpan.FromMilliseconds(stats.Uptime):g}[/]")
    .AddNode($"framerate [yellow]{stats.Framerate} fps[/]");
tree.AddNode($"plugins connected: [yellow]{stats.ConnectedPlugins}/{stats.AllowedPlugins}[/]");
tree.AddNode($"window: [yellow]{stats.WindowWidth}×{stats.WindowHeight}[/]{(stats.WindowIsFullscreen ? " (fullscreen)" : string.Empty)}");
TreeNode model = tree.AddNode("model");
if (current.ModelLoaded)
{
    model.AddNode($"name: [cyan]{current.ModelName}[/] [grey]({current.ModelId})[/]");
    model.AddNode($"live2d parameters: [yellow]{current.NumberOfLive2DParameters}[/]");
    model.AddNode($"art-meshes: [yellow]{current.NumberOfLive2DArtmeshes}[/]");
    model.AddNode($"textures: [yellow]{current.NumberOfTextures}[/] @ [yellow]{current.TextureResolution}px[/]");
}
else
{
    model.AddNode("[red]no model loaded[/]");
}
AnsiConsole.Write(tree);

if (!current.ModelLoaded)
{
    AnsiConsole.MarkupLine("[yellow]Load a model in VTube Studio and re-run to see the full demo.[/]");
    return 0;
}

// 4. Discover hotkeys + offer a trigger picker.
HotkeysInCurrentModelResponse hotkeys = await client.GetHotkeysAsync();
Table table = new Table().Border(TableBorder.Rounded).Title("[bold]hotkeys[/]");
_ = table.AddColumn("id").AddColumn("name").AddColumn("type").AddColumn("description");
foreach (AvailableHotkey hk in hotkeys.AvailableHotkeys)
{
    _ = table.AddRow(
        $"[grey]{hk.HotkeyId[..Math.Min(8, hk.HotkeyId.Length)]}…[/]",
        hk.Name,
        hk.Type,
        hk.Description ?? "[grey](none)[/]");
}
AnsiConsole.Write(table);

if (hotkeys.AvailableHotkeys.Count > 0 && AnsiConsole.Confirm("Trigger a hotkey?"))
{
    AvailableHotkey chosen = AnsiConsole.Prompt(
        new SelectionPrompt<AvailableHotkey>()
            .Title("which hotkey?")
            .UseConverter(static h => $"{h.Name} [grey]({h.Type})[/]")
            .AddChoices(hotkeys.AvailableHotkeys));

    HotkeyTriggerResponse resp = await client.TriggerHotkeyAsync(new HotkeyTriggerRequest { HotkeyId = chosen.HotkeyId });
    AnsiConsole.MarkupLine($"[green]✓[/] triggered [cyan]{chosen.Name}[/] (server echoed id [grey]{resp.HotkeyId[..8]}…[/])");
}

// 5. Subscribe to typed events.
AnsiConsole.MarkupLine("[bold]Subscribing to typed events. Press [yellow]Ctrl+C[/] to exit.[/]");

using IDisposable hotkeySub = client.Events.On<HotkeyTriggeredEventPayload>(
    VTubeStudioEventNames.HotkeyTriggered,
    e =>
    {
        string src = e.HotkeyTriggeredByApi ? "[blue]api[/]" : "[magenta]manual[/]";
        AnsiConsole.MarkupLine($"[grey]{DateTimeOffset.Now:HH:mm:ss}[/]  hotkey [cyan]{e.HotkeyName}[/] [grey]({e.HotkeyAction})[/] · {src}");
    },
    VTubeStudioJsonContext.Default.HotkeyTriggeredEventPayload);

using IDisposable trackSub = client.Events.On<TrackingStatusChangedEventPayload>(
    VTubeStudioEventNames.TrackingStatusChanged,
    e => AnsiConsole.MarkupLine($"[grey]{DateTimeOffset.Now:HH:mm:ss}[/]  tracking face={e.FaceFound} L={e.LeftHandFound} R={e.RightHandFound}"),
    VTubeStudioJsonContext.Default.TrackingStatusChangedEventPayload);

using IDisposable modelSub = client.Events.On<ModelLoadedEventPayload>(
    VTubeStudioEventNames.ModelLoaded,
    e => AnsiConsole.MarkupLine($"[grey]{DateTimeOffset.Now:HH:mm:ss}[/]  model [cyan]{e.ModelName}[/] {(e.ModelLoaded ? "loaded" : "unloaded")}"),
    VTubeStudioJsonContext.Default.ModelLoadedEventPayload);

_ = await client.SubscribeAsync(VTubeStudioEventNames.HotkeyTriggered);
_ = await client.SubscribeAsync(VTubeStudioEventNames.TrackingStatusChanged);
_ = await client.SubscribeAsync(VTubeStudioEventNames.ModelLoaded);

using CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
try { await Task.Delay(Timeout.InfiniteTimeSpan, cts.Token); } catch (OperationCanceledException) { }

AnsiConsole.MarkupLine("[grey]disconnecting…[/]");
return 0;
