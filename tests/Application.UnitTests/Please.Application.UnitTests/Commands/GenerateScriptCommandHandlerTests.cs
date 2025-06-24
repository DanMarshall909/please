using Xunit;
using Please.TestUtilities;
using Please.TestUtilities.Builders;
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

    [Theory]
    [InlineData("Deploy to production", ProviderType.OpenAi, "gpt-4", RiskLevel.High)]
    [InlineData("Create backup", ProviderType.Anthropic, "claude-3", RiskLevel.Medium)]
    [InlineData("List files", ProviderType.Ollama, "llama2", RiskLevel.Low)]
    public async Task valid_script_request_generates_and_saves_script(string task, ProviderType provider, string model, RiskLevel riskLevel)
    {
        // Arrange
        var command = GenerateScriptCommandBuilder.Create()
            .WithTask(task)
            .WithProvider(provider)
            .WithModel(model)
            .Build();

        var expectedResponse = ScriptResponseBuilder.Create()
            .WithTask(task)
            .WithProvider(provider)
            .WithModel(model)
            .WithRiskLevel(riskLevel)
            .Build();

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
        var command = GenerateScriptCommandBuilder.Create()
            .WithTask("List files")
            .Build();

        var expectedResponse = ScriptResponseBuilder.Create()
            .WithTask("List files")
            .Build();

        _scriptGenerator.NextResult = Result<ScriptResponse>.Success(expectedResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Environment.CurrentDirectory, _scriptGenerator.LastRequest?.WorkingDirectory);
    }

    [Theory]
    [InlineData("Generation failed")]
    [InlineData("Network timeout")]
    [InlineData("Invalid API key")]
    public async Task failed_script_generation_throws_descriptive_exception(string errorMessage)
    {
        // Arrange
        var command = GenerateScriptCommandBuilder.Create()
            .WithTask("Test task")
            .Build();

        _scriptGenerator.NextResult = Result<ScriptResponse>.Failure(errorMessage);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ScriptGenerationException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        Assert.Contains(errorMessage, exception.Message);
    }

    [Theory]
    [InlineData(ScriptType.Bash, "/home/user")]
    [InlineData(ScriptType.PowerShell, "C:\\Projects")]
    [InlineData(ScriptType.Python, "/var/lib")]
    public async Task script_request_preserves_script_type_and_working_directory(ScriptType scriptType, string workingDir)
    {
        // Arrange
        var command = GenerateScriptCommandBuilder.Create()
            .WithTask("Custom task")
            .WithScriptType(scriptType)
            .WithWorkingDirectory(workingDir)
            .Build();

        var expectedResponse = ScriptResponseBuilder.Create()
            .WithTask("Custom task")
            .WithScriptType(scriptType)
            .Build();

        _scriptGenerator.NextResult = Result<ScriptResponse>.Success(expectedResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(scriptType, _scriptGenerator.LastRequest?.ScriptType);
        Assert.Equal(workingDir, _scriptGenerator.LastRequest?.WorkingDirectory);
    }
}
