using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Console;
using Please.Domain.Interfaces;

namespace Please.TestUtilities;

public static class TestSystem
{
    public static IServiceProvider Create(Action<IServiceCollection>? configure = null)
    {
        return PleaseHost.CreateServiceProvider(services =>
        {
            // Register test doubles with explicit interface implementations for AOT compatibility
            services.AddSingleton<FakeScriptGenerator>();
            services.AddSingleton<FakeScriptRepository>();
            services.AddSingleton<FakeContextService>();

            // Use direct registration instead of factory methods for AOT compatibility
            services.AddSingleton<IScriptGenerator, FakeScriptGenerator>();
            services.AddSingleton<IScriptRepository, FakeScriptRepository>();
            services.AddSingleton<IContextService, FakeContextService>();

            services.AddLogging(builder => builder.AddDebug());
            configure?.Invoke(services);
        });
    }
}
