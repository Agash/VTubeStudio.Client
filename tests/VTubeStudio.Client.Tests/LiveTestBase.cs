using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.Messages;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Shared setup for live integration tests. See README.md in this folder.
/// </summary>
[TestCategory("Integration")]
public abstract class LiveTestBase
{
    public TestContext TestContext { get; set; } = null!;

    protected static Uri Endpoint =>
        Uri.TryCreate(Environment.GetEnvironmentVariable("VTS_ENDPOINT"), UriKind.Absolute, out Uri? uri)
            ? uri
            : VTubeStudioApi.DefaultEndpoint;

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
                    token = (await client.RequestAuthenticationTokenAsync(tokenCts.Token)).AuthenticationToken;
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
