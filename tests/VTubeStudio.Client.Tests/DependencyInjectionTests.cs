using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTubeStudio.Client.DependencyInjection;

namespace VTubeStudio.Client.Tests;

/// <summary>
/// Tests for the dependency injection hookup.
/// </summary>
[TestClass]
public sealed class DependencyInjectionTests
{
    [TestMethod]
    public void AddClient_ValidatesArguments()
    {
        ServiceCollection services = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => VTubeStudioServiceCollectionExtensions.AddVTubeStudioClient(null!, _ => { }));
        Assert.ThrowsExactly<ArgumentNullException>(() => services.AddVTubeStudioClient(null!));
    }

    [TestMethod]
    public async Task AddClient_RegistersSingletonClient()
    {
        ServiceCollection services = new();
        _ = services.AddVTubeStudioClient(options =>
        {
            options.PluginName = "Plugin";
            options.PluginDeveloper = "Dev";
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        VTubeStudioClient first = provider.GetRequiredService<VTubeStudioClient>();
        VTubeStudioClient second = provider.GetRequiredService<VTubeStudioClient>();

        Assert.AreSame(first, second);
        Assert.IsFalse(first.IsConnected);
        await first.DisposeAsync();
    }
}
