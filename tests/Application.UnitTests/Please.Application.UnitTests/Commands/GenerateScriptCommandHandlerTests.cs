using Xunit;
using Please.TestUtilities;
using Please.Application.Commands.GenerateScript;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Exceptions;

namespace Please.Application.UnitTests.Commands;

public class GenerateScriptCommandHandlerTests
{
    private readonly FakeScriptGenerator _scriptGenerator = new();
    private readonly FakeScriptRepository _scriptRepository = new();
    private readonly GenerateScriptCommandHandler _handler;

    public GenerateScriptCommandHandlerTests() =>
        _handler = new GenerateScriptCommandHandler(_scriptGenerator, _scriptRepository);

    [Fact]
    public async Task valid_script_request_generates_and_saves_script()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Deploy to production", ProviderType.OpenAi, "gpt-4");
        var expectedResponse = ScriptResponse.Create(
            "kubectl apply -f production.yaml",
            "Deploy to production",
            ProviderType.OpenAi,
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

    [Fact]
    public async Task script_request_without_working_directory_uses_current_directory()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("List files");
        var expectedResponse = ScriptResponse.Create(
            "ls -la",
            "List files",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash
        );

        _scriptGenerator.NextResult = Result<ScriptResponse>.Success(expectedResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Environment.CurrentDirectory, _scriptGenerator.LastRequest?.WorkingDirectory);
    }

    [Fact]
    public async Task failed_script_generation_throws_descriptive_exception()
    {
        var command = GenerateScriptCommand.Create("fail");
        _scriptGenerator.NextResult = Result<ScriptResponse>.Failure("bad");

        await Assert.ThrowsAsync<ScriptGenerationException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
    }
}
