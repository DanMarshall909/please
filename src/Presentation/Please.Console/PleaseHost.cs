using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application;

namespace Please.ConsoleHost;

/// <summary>
/// Creates a service provider configured the same way as production.
/// Allows tests to override registrations via the configure callback.
/// </summary>
public static class PleaseHost
{
    public static ServiceProvider CreateServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddApplication();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }
}
