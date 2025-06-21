using NUnit.Framework;
using Moq;
using Please.Application.Services;
using Please.Domain.Commands;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Services;

[TestFixture]
public class CommandProcessorTests
{
    private Mock<IContextService> _context = null!;
    private Mock<IScriptGenerator> _generator = null!;
    private CommandProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new Mock<IContextService>();
        _generator = new Mock<IScriptGenerator>();
        _processor = new CommandProcessor(_context.Object, _generator.Object);
    }

    [Test]
    public async Task process_async_returns_failure_when_context_service_fails()
    {
        _context.Setup(c => c.GetContextAsync(It.IsAny<CommandIntent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CommandContext>.Failure("no context"));

        var result = await _processor.ProcessAsync("list files");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("no context"));
    }

    [Test]
    public async Task process_async_invokes_generator_when_context_available()
    {
        _context.Setup(c => c.GetContextAsync(It.IsAny<CommandIntent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CommandContext>.Success(new CommandContext("/tmp")));
        var expected = ScriptResponse.Create("ls", "list", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.Setup(g => g.GenerateScriptAsync(It.IsAny<ScriptRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScriptResponse>.Success(expected));

        var result = await _processor.ProcessAsync("list");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected));
    }
}

