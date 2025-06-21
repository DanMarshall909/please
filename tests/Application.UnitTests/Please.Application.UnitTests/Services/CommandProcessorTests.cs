using NUnit.Framework;
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
        _context = new FakeContextService();
        _generator = new FakeScriptGenerator();

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddTransient<IContextService>(_ => _context);
        services.AddTransient<IScriptGenerator>(_ => _generator);

        _provider = services.BuildServiceProvider();
        _processor = _provider.GetRequiredService<CommandProcessor>();
    }

    [Test]
    public async Task process_async_returns_failure_when_context_service_fails()
    {
        _context.ContextResult = Result<CommandContext>.Failure("no context");

        var result = await _processor.ProcessAsync("list files");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("no context"));
    }

    [Test]
    public async Task process_async_invokes_generator_when_context_available()
    {
        _context.ContextResult = Result<CommandContext>.Success(new CommandContext("/tmp"));
        var expected = ScriptResponse.Create("ls", "list", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(expected);

        var result = await _processor.ProcessAsync("list");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected));
    }
}

