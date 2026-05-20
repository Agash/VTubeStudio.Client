# VTubeStudio.Client

Modern .NET 10 / C# 14 client library for the [VTube Studio Public API](https://github.com/DenchiSoft/VTubeStudio). AOT-friendly, `System.Text.Json` source-generated, fully typed.

## Why another one?

The only existing C# library on NuGet (`VTS-Sharp`, last updated Dec 2023) targets .NET Standard 2.0 and depends on Newtonsoft.Json + WebSocketSharp + WebSocket4Net — reflection-heavy and not AOT/trim compatible. This library is built bottom-up for modern .NET: source-generated `JsonSerializerContext`, AOT-marked, no third-party serializer.

## Packages

| Package | Purpose |
|---|---|
| `VTubeStudio.Client` | Core WebSocket client + typed message records + event hub |
| `VTubeStudio.Client.DependencyInjection` | `AddVTubeStudioClient(...)` for `Microsoft.Extensions.DependencyInjection` |

## Quick start

```csharp
using VTubeStudio.Client;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;
using VTubeStudio.Client.Serialization;

await using var client = new VTubeStudioClient(new VTubeStudioClientOptions
{
    PluginName = "MyPlugin",
    PluginDeveloper = "Me",
});

await client.ConnectAsync();

// First run: user approves a popup in VTube Studio and a token is issued.
// Subsequent runs: pass the persisted token; the client re-authenticates and only
// re-requests if the stored token was invalidated.
string token = await client.RequestAndAuthenticateAsync(existingToken: null);
// → persist `token` to your secret store

// Resource discovery
HotkeysInCurrentModelResponse hotkeys = await client.GetHotkeysAsync();
foreach (AvailableHotkey hk in hotkeys.AvailableHotkeys)
{
    Console.WriteLine($"{hk.HotkeyId}  {hk.Name}  ({hk.Type})");
}

// Trigger by id
await client.TriggerHotkeyAsync(new HotkeyTriggerRequest { HotkeyId = hotkeys.AvailableHotkeys[0].HotkeyId });

// Subscribe to typed events
client.Events.On<HotkeyTriggeredEventPayload>(
    VTubeStudioEventNames.HotkeyTriggered,
    e => Console.WriteLine($"hotkey {e.HotkeyName} triggered (by API: {e.HotkeyTriggeredByApi})"),
    VTubeStudioJsonContext.Default.HotkeyTriggeredEventPayload);

await client.SubscribeAsync(VTubeStudioEventNames.HotkeyTriggered);

// Block until the user disconnects
await Task.Delay(Timeout.InfiniteTimeSpan);
```

## With dependency injection

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddVTubeStudioClient(opt =>
{
    opt.PluginName = "MyPlugin";
    opt.PluginDeveloper = "Me";
});
var host = builder.Build();

var vts = host.Services.GetRequiredService<VTubeStudioClient>();
await vts.ConnectAsync();
```

## What's covered

- Session/state: `APIStateRequest`, `StatisticsRequest`, `FaceFoundRequest`
- Authentication: full token request + session-authenticate two-step flow
- Models: current model, available models, load, move (with timed interpolation, native to VTS)
- Hotkeys: list, trigger by id (with item-instance scoping)
- Expressions: state, activate / deactivate
- Parameters: input + Live2D parameter lists, value query, custom-parameter injection
- ArtMesh: list, color tint with full `ArtMeshMatcher` semantics
- Items: list, load, unload (single / by ids / by filename / all-by-plugin)
- Events: subscribe / unsubscribe + typed config records, typed event hub

Every payload is a real record with `JsonPropertyName` attributes; everything is registered in a single source-generated `JsonSerializerContext` (`VTubeStudioJsonContext`). The library is `IsAotCompatible="true"` and `IsTrimmable="true"`.

## Sample

`samples/VTubeStudio.Client.Sample/` is a runnable Spectre.Console app that drives the full lifecycle: connect → authenticate → discover resources → trigger hotkeys → subscribe to events → tail event log. Run it against a live VTube Studio instance to see the API in action.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
