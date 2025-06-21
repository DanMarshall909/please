using NUnit.Framework;
using Please.TestUtilities;
using Please.Application.Commands.GenerateScript;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Exceptions;

namespace Please.Application.UnitTests.Commands;

[TestFixture]
public class GenerateScriptCommandHandlerTests
{
    private FakeScriptGenerator _scriptGenerator = null!;
    private FakeScriptRepository _scriptRepository = null!;
    private GenerateScriptCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _scriptGenerator = new FakeScriptGenerator();
        _scriptRepository = new FakeScriptRepository();
        _handler = new GenerateScriptCommandHandler(_scriptGenerator, _scriptRepository);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldGenerateAndSaveScript()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Deploy to production", ProviderType.OpenAI, "gpt-4");
        var expectedResponse = ScriptResponse.Create(
            "kubectl apply -f production.yaml",
            "Deploy to production",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash,
            RiskLevel.High
        );

        _scriptGenerator.NextResult = Result<ScriptResponse>.Success(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
        Assert.That(_scriptGenerator.LastRequest?.TaskDescription, Is.EqualTo(command.TaskDescription));
        Assert.That(_scriptGenerator.LastRequest?.Provider, Is.EqualTo(command.Provider));
        Assert.That(_scriptGenerator.LastRequest?.Model, Is.EqualTo(command.Model));
        Assert.That(_scriptRepository.Scripts, Has.Exactly(1).EqualTo(expectedResponse));
    }

    [Test]
    public async Task Handle_WithMinimalCommand_ShouldSetWorkingDirectoryToCurrentDirectory()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("List files");
        var expectedResponse = ScriptResponse.Create(
            "ls -la",
            "List files",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash
        );

        _scriptGenerator.NextResult = Result<ScriptResponse>.Success(expectedResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(_scriptGenerator.LastRequest?.WorkingDirectory, Is.EqualTo(Environment.CurrentDirectory));
    }

    [Test]
    public void Handle_WhenGenerationFails_ThrowsScriptGenerationException()
    {
        var command = GenerateScriptCommand.Create("fail");
        _scriptGenerator.NextResult = Result<ScriptResponse>.Failure("bad");

        Assert.That(async () => await _handler.Handle(command, CancellationToken.None),
            Throws.TypeOf<ScriptGenerationException>());
    }

}
