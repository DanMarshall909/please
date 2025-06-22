using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application;
using Please.Domain.Interfaces;

namespace Please.TestUtilities;

public static class TestSystem
{
    public static IServiceProvider Create()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<FakeScriptGenerator>();
        services.AddSingleton<FakeScriptRepository>();
        services.AddSingleton<FakeContextService>();
        services.AddSingleton<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
        services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
        services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
        services.AddLogging(builder => builder.AddDebug());
        return services.BuildServiceProvider();
    }
}
