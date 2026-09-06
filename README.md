# VTubeStudio.Client

[![NuGet](https://img.shields.io/nuget/v/VTubeStudio.Client.svg)](https://www.nuget.org/packages/VTubeStudio.Client/)
[![NuGet downloads](https://img.shields.io/nuget/dt/VTubeStudio.Client.svg)](https://www.nuget.org/packages/VTubeStudio.Client/)
[![Build](https://github.com/Agash/VTubeStudio.Client/actions/workflows/build.yml/badge.svg)](https://github.com/Agash/VTubeStudio.Client/actions/workflows/build.yml)
[![License](https://img.shields.io/github/license/Agash/VTubeStudio.Client.svg)](LICENSE.txt)

Modern .NET 10 / C# 14 client library for the [VTube Studio Public API](https://github.com/DenchiSoft/VTubeStudio). AOT-friendly, `System.Text.Json` source-generated, fully typed.

## Install

```bash
dotnet add package VTubeStudio.Client
dotnet add package VTubeStudio.Client.DependencyInjection   # optional, only if you use Microsoft.Extensions.DependencyInjection
```

## Packages

| Package | Purpose |
|---|---|
| `VTubeStudio.Client` | Core WebSocket client, typed message records, typed event hub |
| `VTubeStudio.Client.DependencyInjection` | `AddVTubeStudioClient(...)` for `Microsoft.Extensions.DependencyInjection` |

## Quick start

```csharp
using VTubeStudio.Client;
using VTubeStudio.Client.Events;
using VTubeStudio.Client.Messages;

await using var client = new VTubeStudioClient(new VTubeStudioClientOptions
{
    PluginName = "MyPlugin",
    PluginDeveloper = "Me",
});

await client.ConnectAsync();

// First run: VTube Studio prompts the user to approve. On approval the token
// is returned; persist it so subsequent runs re-authenticate silently. If the
// stored token is later invalidated, the client re-requests automatically.
string token = await client.RequestAndAuthenticateAsync(existingToken: null);
// persist `token` to your secret store

// Resource discovery
HotkeysInCurrentModelResponse hotkeys = await client.GetHotkeysAsync();
foreach (AvailableHotkey hk in hotkeys.AvailableHotkeys)
{
    Console.WriteLine($"{hk.HotkeyId}  {hk.Name}  ({hk.Type})");
}

// Trigger by id
await client.TriggerHotkeyAsync(new HotkeyTriggerRequest { HotkeyId = hotkeys.AvailableHotkeys[0].HotkeyId });

// Subscribe to typed events. No JsonTypeInfo or event-name string needed;
// every payload type carries both via IVTubeStudioEvent<TSelf>.
client.Events.On<HotkeyTriggeredEventPayload>(
    e => Console.WriteLine($"hotkey {e.HotkeyName} triggered (by API: {e.HotkeyTriggeredByApi})"));
await client.SubscribeAsync<HotkeyTriggeredEventPayload>();

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

- Session / state: `APIStateRequest`, `StatisticsRequest`, `FaceFoundRequest`, `VTSFolderInfoRequest`
- Authentication: full token-request + session-authenticate two-step flow handled by one method, plus permission query/request
- Models: current model, available models, load, move, physics get/set
- Hotkeys: list, trigger by id (with item-instance scoping)
- Expressions: state (with details), activate / deactivate
- Parameters: input + Live2D parameter lists, value query, custom-parameter create/delete, custom-parameter injection
- ArtMesh: list (with groups), color tint, at-position query, user selection
- Scene: lighting overlay info, NDI config, post-processing list/update
- Items: list, load (incl. custom image data), unload, animation control, move, sort, pin
- Events: subscribe / unsubscribe (+all) with typed config records, typed event hub via `IVTubeStudioEvent<TSelf>`

## Sample

[`samples/VTubeStudio.Client.Sample/`](samples/VTubeStudio.Client.Sample) is an interactive Spectre.Console app that exercises the public surface: connect, authenticate, model swap, hotkey trigger, expression cycle, item load with move/pin/unload, ArtMesh tint cycle and user selection, model orbit, custom parameter lifecycle (create, feed, delete), custom parameter injection, permissions, physics and post-processing reads, test-event ticks, live event tailing.

```bash
cd samples/VTubeStudio.Client.Sample
dotnet run
```

## Contributing

See [CONTRIBUTING.md](.github/CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](.github/CODE_OF_CONDUCT.md).

## License

MIT, see [LICENSE.txt](LICENSE.txt).
