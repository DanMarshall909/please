using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Console;

namespace Please.TestUtilities;

public static class TestSystem
{
    public static IServiceProvider Create(Action<IServiceCollection>? configure = null)
    {
        return PleaseHost.CreateServiceProvider(services =>
        {
            services.AddTestDoubles();

            services.AddLogging(builder => builder.AddDebug());
            configure?.Invoke(services);
        });
    }
}
