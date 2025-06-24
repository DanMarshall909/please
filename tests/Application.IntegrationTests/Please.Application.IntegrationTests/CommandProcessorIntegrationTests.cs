using Xunit;
using Please.Application.Services;
using Please.TestUtilities;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Please.Console;
using Please.Domain.Interfaces;
using Please.Domain.Commands;

namespace Please.Application.IntegrationTests;

public class CommandProcessorIntegrationTests
{
    [Fact]
    public async Task process_async_flows_through_context_and_generator()
    {
        var context = new FakeContextService
        {
            ContextResult = Result<CommandContext>.Success(new CommandContext("/"))
        };
        var generator = new FakeScriptGenerator
        {
            NextResult = Result<ScriptResponse>.Success(
                ScriptResponse.Create("ls", "list", ProviderType.OpenAi, "gpt-4", ScriptType.Bash))
        };

        var provider = PleaseHost.CreateServiceProvider(services =>
        {
            // Register test doubles with explicit interface implementations for AOT compatibility
            services.AddSingleton<IContextService>(_ => context);
            services.AddSingleton<IScriptGenerator>(_ => generator);

            // Register required repository for AOT compatibility
            services.AddSingleton<FakeScriptRepository>();
            services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
        });
        var processor = provider.GetRequiredService<CommandProcessor>();

        var result = await processor.ProcessAsync("list");

        Assert.True(result.IsSuccess);
        Assert.Equal("list", generator.LastRequest?.TaskDescription);
    }
}
