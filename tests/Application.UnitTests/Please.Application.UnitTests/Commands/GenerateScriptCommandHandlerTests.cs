using TUnit;
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
        Assert.Equal(expectedResponse, result);
        Assert.Equal(command.TaskDescription, _scriptGenerator.LastRequest?.TaskDescription);
        Assert.Equal(command.Provider, _scriptGenerator.LastRequest?.Provider);
        Assert.Equal(command.Model, _scriptGenerator.LastRequest?.Model);
        Assert.Equal(1, _scriptRepository.Scripts.Count(s => s == expectedResponse));
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
        Assert.Equal(Environment.CurrentDirectory, _scriptGenerator.LastRequest?.WorkingDirectory);
    }

    [Test]
    public void Handle_WhenGenerationFails_ThrowsScriptGenerationException()
    {
        var command = GenerateScriptCommand.Create("fail");
        _scriptGenerator.NextResult = Result<ScriptResponse>.Failure("bad");

        Assert.ThrowsAsync<ScriptGenerationException>(async () => await _handler.Handle(command, CancellationToken.None));
    }

}
