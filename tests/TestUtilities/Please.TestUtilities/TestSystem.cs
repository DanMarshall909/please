using Microsoft.Extensions.DependencyInjection;
using Please.Application;
using Please.Domain.Interfaces;

namespace Please.TestUtilities;

public static class TestSystem
{
    public static IServiceProvider Create()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddTransient<FakeScriptGenerator>();
        services.AddTransient<FakeScriptRepository>();
        services.AddTransient<FakeContextService>();
        services.AddTransient<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
        services.AddTransient<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
        services.AddTransient<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
        services.AddLogging(builder => builder.AddDebug());
        return services.BuildServiceProvider();
    }
}
