<!-- Thanks for the PR! A few quick checks before review. -->

## Summary

<!-- What does this change do, and why? -->

## Checklist

- [ ] Built locally with `dotnet build VTubeStudio.Client.slnx`.
- [ ] Tests added/updated and `dotnet test tests/VTubeStudio.Client.Tests/VTubeStudio.Client.Tests.csproj` passes.
- [ ] If new message types or event payloads were added, they're registered in `VTubeStudioJsonContext` (and event payloads implement `IVTubeStudioEvent<TSelf>`).
- [ ] Public XML doc comments added on every new public symbol (otherwise CS1591 will fail the build on packable projects).
- [ ] Sample updated if the public surface changed.
- [ ] No `Newtonsoft.Json` references introduced.

## Related issues

<!-- Closes #..., refs #... -->
