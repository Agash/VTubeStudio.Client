using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Messages;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Shared setup for live integration tests. See README.md in this folder.
///
/// The client connects and authenticates once per test class: parallel token
/// requests collide server-side, so tests share one session. Requires the
/// same plugin identity on every run; VTube Studio remembers the approval.
/// </summary>
[TestCategory("Integration")]
public abstract class LiveTestBase
{
    private static VTubeStudioClient? _sharedClient;
    private static string? _sharedToken;

    public TestContext TestContext { get; set; } = null!;

    protected static VTubeStudioClient Client =>
        _sharedClient ?? throw new InvalidOperationException("Live client is not initialized.");

    protected static string LiveToken() =>
        _sharedToken ?? throw new InvalidOperationException("Live token is not initialized.");

    protected static Uri Endpoint =>
        Uri.TryCreate(Environment.GetEnvironmentVariable("VTS_ENDPOINT"), UriKind.Absolute, out Uri? uri)
            ? uri
            : VTubeStudioApi.DefaultEndpoint;

    protected static void EnsureInitialized(TestContext context)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("VTS_LIVE_TESTS"), "1", StringComparison.Ordinal))
        {
            Assert.Inconclusive("Live VTube Studio tests are disabled. Set VTS_LIVE_TESTS=1 with VTube Studio running (see tests README.md).");
        }

        if (_sharedClient is null)
        {
            (_sharedClient, _sharedToken) = ConnectAndAuthenticateAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    protected static void TeardownShared()
    {
        VTubeStudioClient? client = Interlocked.Exchange(ref _sharedClient, null);
        _sharedToken = null;
        client?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [TestInitialize]
    public void RequireLiveGate()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("VTS_LIVE_TESTS"), "1", StringComparison.Ordinal))
        {
            return;
        }

        Assert.Inconclusive(
            "Live VTube Studio tests are disabled. Set VTS_LIVE_TESTS=1 with VTube Studio running (see tests README.md).");
    }

    /// <summary>
    /// Connects and authenticates. Uses VTS_TOKEN when set, else requests a token.
    /// </summary>
    protected static async Task<(VTubeStudioClient Client, string Token)> ConnectAndAuthenticateAsync(
        TestContext context,
        CancellationToken ct)
    {
        VTubeStudioClient client = new(new VTubeStudioClientOptions
        {
            Endpoint = Endpoint,
            PluginName = "VTubeStudio.Client LiveTests",
            PluginDeveloper = "Agash",
        });

        try
        {
            using (CancellationTokenSource connectCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                connectCts.CancelAfter(TimeSpan.FromSeconds(10));
                await client.ConnectAsync(connectCts.Token);
            }

            string? token = Environment.GetEnvironmentVariable("VTS_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
            {
                context.WriteLine("No VTS_TOKEN set. Requesting a token. Approve the popup in VTube Studio.");
                using (CancellationTokenSource tokenCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    tokenCts.CancelAfter(TimeSpan.FromSeconds(90));
                    try
                    {
                        token = (await client.RequestAuthenticationTokenAsync(tokenCts.Token)).AuthenticationToken;
                    }
                    catch (OperationCanceledException)
                    {
                        Assert.Inconclusive("Token request timed out with no approval. Rerun while present to approve, or set VTS_TOKEN.");
                        throw new InvalidOperationException("Unreachable.");
                    }
                }

                context.WriteLine("Fresh token granted. Export it as VTS_TOKEN for silent reruns (do NOT commit it).");
            }

            AuthenticationResponse auth;
            using (CancellationTokenSource authCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                authCts.CancelAfter(TimeSpan.FromSeconds(15));
                auth = await client.AuthenticateAsync(token, authCts.Token);
            }

            Assert.IsTrue(auth.Authenticated, $"VTube Studio rejected authentication: {auth.Reason}");

            return (client, token);
        }
        catch
        {
            await client.DisposeAsync();
            throw;
        }
    }
}
