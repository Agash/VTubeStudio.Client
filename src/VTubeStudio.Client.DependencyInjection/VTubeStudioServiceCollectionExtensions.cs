using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VTubeStudio.Client.DependencyInjection;

/// <summary>DI extensions that register a singleton <see cref="VTubeStudioClient"/>.</summary>
public static class VTubeStudioServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="VTubeStudioClient"/> as a singleton, with <see cref="VTubeStudioClientOptions"/>
    /// configured by <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddVTubeStudioClient(
        this IServiceCollection services,
        Action<VTubeStudioClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        _ = services.AddOptions<VTubeStudioClientOptions>().Configure(configure);
        services.TryAddSingleton<VTubeStudioClient>(static sp =>
        {
            VTubeStudioClientOptions options = sp.GetRequiredService<IOptions<VTubeStudioClientOptions>>().Value;
            ILogger<VTubeStudioClient>? logger = sp.GetService<ILogger<VTubeStudioClient>>();
            return new VTubeStudioClient(options, logger);
        });
        return services;
    }
}
