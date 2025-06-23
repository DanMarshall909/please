using Xunit;
using Please.Application.Services;
using Please.TestUtilities;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Please.Console;
using Please.Domain.Interfaces;

namespace Please.Application.IntegrationTests;

public class CommandProcessorIntegrationTests
{
    [Fact]
    public async Task process_async_flows_through_context_and_generator()
    {
        var context = new FakeContextService();
        var generator = new FakeScriptGenerator
        {
            NextResult = Result<ScriptResponse>.Success(
                ScriptResponse.Create("ls", "list", ProviderType.OpenAi, "gpt-4", ScriptType.Bash))
        };

        var provider = PleaseHost.CreateServiceProvider(services =>
        {
            services.AddTransient<IContextService>(_ => context);
            services.AddTransient<IScriptGenerator>(_ => generator);
        });
        var processor = provider.GetRequiredService<CommandProcessor>();

        var result = await processor.ProcessAsync("list");

        Assert.True(result.IsSuccess);
        Assert.Equal("list", generator.LastRequest?.TaskDescription);
    }
}
