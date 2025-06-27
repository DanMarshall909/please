using Please.Domain.Interfaces;

namespace Please.TestUtilities;

public static class TestModule
{
    public static IServiceCollection AddTestDoubles(this IServiceCollection services)
    {
        services.AddSingleton<FakeScriptGenerator>();
        services.AddSingleton<FakeScriptRepository>();
        services.AddSingleton<FakeContextService>();

        services.AddSingleton<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
        services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
        services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());

        return services;
    }
}
