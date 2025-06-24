using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application;

namespace Please.Console;

/// <summary>
/// Creates a service provider configured the same way as production.
/// Allows tests to override registrations via the configure callback.
/// </summary>
public static class PleaseHost
{
    /// <summary>
    /// Creates a service provider with application services registered.
    /// </summary>
    /// <param name="configure">Optional callback to configure additional services.</param>
    /// <returns>A configured ServiceProvider.</returns>
    public static ServiceProvider CreateServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddApplication();
        configure?.Invoke(services);

        // Use AOT-friendly options for the service provider
        var options = new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        };

        return services.BuildServiceProvider(options);
    }
}
