# Contributing

Thanks for taking the time to contribute! A few things to keep the workflow smooth:

## Before you start

- Open an issue first if you're planning a non-trivial change. A short discussion saves rework.
- Read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Building

```pwsh
dotnet restore VTubeStudio.Client.slnx
dotnet build VTubeStudio.Client.slnx
dotnet test tests/VTubeStudio.Client.Tests/VTubeStudio.Client.Tests.csproj
```

The sample is published with `PublishAot=true` + `IsTrimmable=true` in CI as a smoke test that the library stays AOT- and trim-clean. If you add a new message type, register it in `VTubeStudioJsonContext` or the AOT publish step will fail.

## Code style

- File-scoped namespaces, `nullable enable`, latest C# language level.
- `TreatWarningsAsErrors=true`; `CS1591` is escalated to error on packable projects (so every public symbol needs an XML doc comment).
- One concept per file. Bundle small related records in a single file (e.g. all messages for a single API surface).
- `System.Text.Json` with source-generated `JsonTypeInfo`; no `Newtonsoft.Json`.

## Adding a new request/response

1. Add a typed record per request and response under `src/VTubeStudio.Client/Messages/`.
2. Register both types in `VTubeStudioJsonContext`.
3. Add the `messageType` constant to `VTubeStudioMessageTypes`.
4. Add a typed method on `VTubeStudioClient` that uses `SendAsync` / `SendEmptyRequestAsync` / `SendAndDiscardAsync`.
5. Add a test that round-trips the wire format against a captured response sample.

## Adding a new event payload

1. Add the payload record under `src/VTubeStudio.Client/Events/EventPayloads.cs`.
2. Implement `IVTubeStudioEvent<TSelf>` on it — supply `EventName` (from `VTubeStudioEventNames`) and `JsonTypeInfo` (from the context).
3. Register in `VTubeStudioJsonContext`.
4. Add the event-name constant to `VTubeStudioEventNames`.
5. Demonstrate it in the sample's `On<TPayload>` registrations.

## Pull request flow

1. Branch from `main`.
2. Keep commits small and focused. Subject line ≤ 50 characters, imperative mood.
3. Update or add tests when behaviour changes.
4. Update `README.md` if the public surface changes.
5. The CI workflow builds, tests, AOT-publishes the sample, and packs both NuGet packages. All four must be green to merge.

## Releasing

Releases are tagged `vX.Y.Z` on the `main` branch. The published GitHub release triggers the `publish_nuget` job, which pushes both packages via NuGet's OIDC trusted-publisher flow. No API keys are stored in the repo.
