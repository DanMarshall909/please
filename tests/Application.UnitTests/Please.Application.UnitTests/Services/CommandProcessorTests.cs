using Xunit;
using Please.TestUtilities;
using Please.Application.Services;
using Please.Domain.Commands;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Services;

public class CommandProcessorTests
{
    private readonly FakeContextService _context;
    private readonly FakeScriptGenerator _generator;
    private readonly CommandProcessor _processor;

    public CommandProcessorTests()
    {
        // Create a test service provider with explicit configuration for AOT compatibility
        var provider = TestSystem.Create(services =>
        {
            // Ensure the test doubles are properly configured
            services.AddSingleton<FakeScriptGenerator>();
            services.AddSingleton<FakeScriptRepository>();
            services.AddSingleton<FakeContextService>();

            // Register interfaces with their implementations
            services.AddSingleton<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
            services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
            services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
        });

        _context = provider.GetRequiredService<FakeContextService>();
        _generator = provider.GetRequiredService<FakeScriptGenerator>();
        _processor = provider.GetRequiredService<CommandProcessor>();
    }

    [Fact]
    public async Task process_async_returns_failure_when_context_service_fails()
    {
        _context.ContextResult = Result<CommandContext>.Failure("no context");

        var result = await _processor.ProcessAsync("list files");

        Assert.True(result.IsFailure);
        Assert.Equal("no context", result.Error);
    }

    [Fact]
    public async Task process_async_invokes_generator_when_context_available()
    {
        _context.ContextResult = Result<CommandContext>.Success(new CommandContext("/tmp"));
        var expected = ScriptResponse.Create("ls", "list", ProviderType.OpenAi, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(expected);

        var result = await _processor.ProcessAsync("list");

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }
}
