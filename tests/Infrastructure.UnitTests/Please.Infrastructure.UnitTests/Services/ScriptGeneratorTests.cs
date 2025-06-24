using FluentAssertions;
using Moq;
using NUnit.Framework;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Infrastructure.Services;
using Please.TestUtilities.Builders;

namespace Please.Infrastructure.UnitTests.Services;

[TestFixture]
public class ScriptGeneratorTests
{
    private ScriptGenerator _scriptGenerator = null!;

    [SetUp]
    public void Setup()
    {
        _scriptGenerator = new ScriptGenerator();
    }

    [Test]
    public async Task GenerateScriptAsync_with_valid_request_should_return_success()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithTask("list files in current directory")
            .WithProvider(ProviderType.OpenAi)
            .WithModel("gpt-4")
            .Build();

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Script.Should().NotBeEmpty();
        result.Value.TaskDescription.Should().Be("list files in current directory");
        result.Value.Provider.Should().Be(ProviderType.OpenAi);
        result.Value.Model.Should().Be("gpt-4");
    }

    [Test]
    public async Task GenerateScriptAsync_with_null_request_should_return_failure()
    {
        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("request");
    }

    [Test]
    public async Task GenerateScriptAsync_with_empty_task_description_should_return_failure()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithTask("")
            .Build();

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("task description");
    }

    [Test]
    public async Task IsProviderAvailableAsync_with_openai_provider_should_return_true()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithProvider(ProviderType.OpenAi)
            .Build();

        // Act
        var result = await _scriptGenerator.IsProviderAvailableAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public async Task IsProviderAvailableAsync_with_anthropic_provider_should_return_true()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithProvider(ProviderType.Anthropic)
            .Build();

        // Act
        var result = await _scriptGenerator.IsProviderAvailableAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public void GetFallbackModel_with_openai_provider_should_return_gpt_3_5_turbo()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithProvider(ProviderType.OpenAi)
            .WithModel("invalid-model")
            .Build();

        // Act
        var fallbackModel = _scriptGenerator.GetFallbackModel(request);

        // Assert
        fallbackModel.Should().Be("gpt-3.5-turbo");
    }

    [Test]
    public void GetFallbackModel_with_anthropic_provider_should_return_claude_3_haiku()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithProvider(ProviderType.Anthropic)
            .WithModel("invalid-model")
            .Build();

        // Act
        var fallbackModel = _scriptGenerator.GetFallbackModel(request);

        // Assert
        fallbackModel.Should().Be("claude-3-haiku-20240307");
    }

    [Test]
    public async Task GenerateScriptAsync_should_detect_powershell_script_type()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithTask("get current date using powershell")
            .Build();

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ScriptType.Should().Be(ScriptType.PowerShell);
    }

    [Test]
    public async Task GenerateScriptAsync_should_detect_bash_script_type()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithTask("list files using bash")
            .Build();

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ScriptType.Should().Be(ScriptType.Bash);
    }
}
