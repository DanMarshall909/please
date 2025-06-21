using TUnit;
using Please.TestUtilities;
using Please.Application.Services;
using Please.Domain.Commands;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Please.Application;

namespace Please.Application.UnitTests.Services;

[TestFixture]
public class CommandProcessorTests
{
    private FakeContextService _context = null!;
    private FakeScriptGenerator _generator = null!;
    private IServiceProvider _provider = null!;
    private CommandProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = TestSystem.Create();
        _context = _provider.GetRequiredService<FakeContextService>();
        _generator = _provider.GetRequiredService<FakeScriptGenerator>();
        _processor = _provider.GetRequiredService<CommandProcessor>();
    }

    [Test]
    public async Task process_async_returns_failure_when_context_service_fails()
    {
        _context.ContextResult = Result<CommandContext>.Failure("no context");

        var result = await _processor.ProcessAsync("list files");

        Assert.True(result.IsFailure);
        Assert.Equal("no context", result.Error);
    }

    [Test]
    public async Task process_async_invokes_generator_when_context_available()
    {
        _context.ContextResult = Result<CommandContext>.Success(new CommandContext("/tmp"));
        var expected = ScriptResponse.Create("ls", "list", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(expected);

        var result = await _processor.ProcessAsync("list");

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }
}

