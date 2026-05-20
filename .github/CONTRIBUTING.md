# Contributing

Open an issue first for non-trivial changes so we can align on direction before code.

## Build and test

```pwsh
dotnet restore VTubeStudio.Client.slnx
dotnet build VTubeStudio.Client.slnx
dotnet test tests/VTubeStudio.Client.Tests/VTubeStudio.Client.Tests.csproj
```

## Releasing

Tag `vX.Y.Z` on `main`. Publishing a GitHub release triggers the workflow's NuGet push via OIDC trusted publishing. No API keys are stored in the repo.
