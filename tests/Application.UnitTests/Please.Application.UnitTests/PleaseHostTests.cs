using Xunit;
using Please.Application.Services;
using Please.Domain.Interfaces;
using Please.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Please.Console;

namespace Please.Application.UnitTests;

public class PleaseHostTests
{
    [Fact]
    public void create_service_provider_resolves_script_service()
    {
        // Register all required dependencies for AOT compatibility
        var provider = PleaseHost.CreateServiceProvider(services =>
        {
            // Register test doubles
            services.AddSingleton<FakeScriptGenerator>();
            services.AddSingleton<FakeScriptRepository>();
            services.AddSingleton<FakeContextService>();

            // Register interfaces
            services.AddSingleton<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
            services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
            services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
        });

        object? service = provider.GetService(typeof(IScriptService));
        Assert.NotNull(service);
    }

    [Fact]
    public void overrides_can_replace_registered_services()
    {
        var fake = new FakeScriptGenerator();
        var provider = PleaseHost.CreateServiceProvider(services =>
        {
            // Register the fake generator
            services.AddSingleton<IScriptGenerator>(fake);

            // Register other required dependencies
            services.AddSingleton<FakeScriptRepository>();
            services.AddSingleton<FakeContextService>();
            services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
            services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
        });

        var resolved = provider.GetRequiredService<IScriptGenerator>();
        Assert.Same(fake, resolved);
    }
}
