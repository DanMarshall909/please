using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Domain.Interfaces;
using Please.ConsoleHost;

namespace Please.TestUtilities;

public static class TestSystem
{
    public static IServiceProvider Create(Action<IServiceCollection>? configure = null)
    {
        return PleaseHost.CreateServiceProvider(services =>
        {
            services.AddTransient<FakeScriptGenerator>();
            services.AddTransient<FakeScriptRepository>();
            services.AddTransient<FakeContextService>();
            services.AddTransient<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
            services.AddTransient<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
            services.AddTransient<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
            services.AddLogging(builder => builder.AddDebug());
            configure?.Invoke(services);
        });
    }
}
