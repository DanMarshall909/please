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
        var provider = PleaseHost.CreateServiceProvider();
        object? service = provider.GetService(typeof(IScriptService));
        Assert.NotNull(service);
    }

    [Fact]
    public void overrides_can_replace_registered_services()
    {
        var fake = new FakeScriptGenerator();
        var provider = PleaseHost.CreateServiceProvider(services => { services.AddSingleton<IScriptGenerator>(fake); });

        var resolved = provider.GetRequiredService<IScriptGenerator>();
        Assert.Same(fake, resolved);
    }
}
