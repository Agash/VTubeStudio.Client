# Tests

## Unit tests (default)

```powershell
# From the repository root. Runs everything except the Integration category.
dotnet test tests/VTubeStudio.Client.Tests/VTubeStudio.Client.Tests.csproj
```

## Live integration tests (category `Integration`)

These tests run `VTubeStudio.Client` against a real VTube Studio instance.

Requirements:

- VTube Studio running on this machine with **Allow Plugin API access** enabled
  (main config page in VTube Studio).
- At least **2 models** available for the model-swap test
  (`ModelLoadedEvent_TypedAndRaw_FireOnProgrammaticSwap` restores the previously
  loaded model afterwards, so reruns are deterministic).

Run:

```powershell
# First run pops an approval dialog in VTube Studio; VTS remembers the plugin
# ("VTubeStudio.Client LiveTests" by "Agash"), so later runs are silent when you
# reuse the token.
$env:VTS_LIVE_TESTS = "1"
dotnet test tests/VTubeStudio.Client.Tests/VTubeStudio.Client.Tests.csproj -- --filter "TestCategory=Integration"
```

Optional configuration (no secrets are ever committed):

| Variable         | Default                  | Purpose                                           |
| ---------------- | ------------------------ | ------------------------------------------------- |
| `VTS_LIVE_TESTS` | (unset = skip)           | Set to `1` to actually run these tests.           |
| `VTS_ENDPOINT`   | `ws://localhost:8001`    | VTube Studio Public API WebSocket endpoint.       |
| `VTS_TOKEN`      | (requests a fresh token) | Previously approved auth token for silent reruns. |

Without `VTS_LIVE_TESTS=1` every integration test reports **Inconclusive
(skipped)**. CI additionally passes `--filter "TestCategory!=Integration"`, so
it never executes them and stays green with no VTube Studio installed.
