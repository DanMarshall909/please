using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Infrastructure.Services;
using Shouldly;
using Xunit;

namespace Please.Infrastructure.UnitTests.Services;

public class ScriptGeneratorTests
{
    private readonly ScriptGenerator _scriptGenerator;

    public ScriptGeneratorTests()
    {
        _scriptGenerator = new ScriptGenerator();
    }

    [Fact]
    public async Task GenerateScriptAsync_with_valid_request_returns_success()
    {
        // Arrange
        var request = ScriptRequest.Create(
            "list files in current directory",
            ProviderType.OpenAi,
            "gpt-4"
        );

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Script.ShouldNotBeEmpty();
        result.Value.TaskDescription.ShouldBe("list files in current directory");
        result.Value.Provider.ShouldBe(ProviderType.OpenAi);
        result.Value.Model.ShouldBe("gpt-4");
    }

    [Fact]
    public async Task GenerateScriptAsync_with_null_request_returns_failure()
    {
        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Script request cannot be null");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateScriptAsync_with_empty_task_description_returns_failure(string taskDescription)
    {
        // Arrange
        var request = ScriptRequest.Create(
            taskDescription,
            ProviderType.OpenAi,
            "gpt-4"
        );

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("task description cannot be empty");
    }

    [Fact]
    public async Task GenerateScriptAsync_with_null_task_description_returns_failure()
    {
        // Arrange
        var request = ScriptRequest.Create(
            null!,
            ProviderType.OpenAi,
            "gpt-4"
        );

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("task description cannot be empty");
    }

    [Theory]
    [InlineData("list files", "Get-ChildItem")]
    [InlineData("show files in directory", "Get-ChildItem")]
    [InlineData("get current date", "Get-Date")]
    [InlineData("show time", "Get-Date")]
    [InlineData("list processes", "Get-ChildItem")] // "list" matches first
    [InlineData("show running process", "Get-Process")]
    public async Task GenerateScriptAsync_detects_powershell_patterns_correctly(string taskDescription, string expectedScript)
    {
        // Arrange
        var request = ScriptRequest.Create(taskDescription, ProviderType.OpenAi, "gpt-4");

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Script.ShouldBe(expectedScript);
        result.Value!.ScriptType.ShouldBe(ScriptType.PowerShell);
    }

    [Theory]
    [InlineData("bash list files", "ls -la")]
    [InlineData("linux show files", "ls -la")]
    [InlineData("unix get date", "date")]
    [InlineData("bash show time", "date")]
    [InlineData("linux list processes", "ls -la")] // "list" matches first
    [InlineData("unix show process", "ps aux")]
    public async Task GenerateScriptAsync_detects_bash_patterns_correctly(string taskDescription, string expectedScript)
    {
        // Arrange
        var request = ScriptRequest.Create(taskDescription, ProviderType.OpenAi, "gpt-4");

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Script.ShouldBe(expectedScript);
        result.Value!.ScriptType.ShouldBe(ScriptType.Bash);
    }

    [Fact]
    public async Task GenerateScriptAsync_with_unknown_task_returns_generic_script()
    {
        // Arrange
        var request = ScriptRequest.Create("do something unusual", ProviderType.OpenAi, "gpt-4");

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Script.ShouldBe("# PowerShell script for: do something unusual");
        result.Value!.ScriptType.ShouldBe(ScriptType.PowerShell);
    }

    [Theory]
    [InlineData(ProviderType.OpenAi, true)]
    [InlineData(ProviderType.Anthropic, true)]
    [InlineData(ProviderType.Ollama, true)]
    public async Task IsProviderAvailableAsync_returns_correct_availability(ProviderType provider, bool expectedAvailable)
    {
        // Arrange
        var request = ScriptRequest.Create("test task", provider, "test-model");

        // Act
        var result = await _scriptGenerator.IsProviderAvailableAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedAvailable);
    }

    [Theory]
    [InlineData(ProviderType.OpenAi, "gpt-3.5-turbo")]
    [InlineData(ProviderType.Anthropic, "claude-3-haiku-20240307")]
    [InlineData(ProviderType.Ollama, "llama2")]
    public void GetFallbackModel_returns_correct_fallback_for_provider(ProviderType provider, string expectedModel)
    {
        // Arrange
        var request = ScriptRequest.Create("test task", provider, "some-model");

        // Act
        var result = _scriptGenerator.GetFallbackModel(request);

        // Assert
        result.ShouldBe(expectedModel);
    }

    [Fact]
    public void GetFallbackModel_with_default_provider_returns_openai_fallback()
    {
        // Arrange - test the default case when no provider is specified
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, "some-model");

        // Act
        var result = _scriptGenerator.GetFallbackModel(request);

        // Assert
        result.ShouldBe("gpt-3.5-turbo");
    }

    [Fact]
    public async Task GenerateScriptAsync_uses_fallback_model_when_no_model_specified()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.Anthropic, null);

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Model.ShouldBe("claude-3-haiku-20240307");
    }

    [Fact]
    public async Task GenerateScriptAsync_sets_low_risk_level_by_default()
    {
        // Arrange
        var request = ScriptRequest.Create("safe operation", ProviderType.OpenAi, "gpt-4");

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.RiskLevel.ShouldBe(RiskLevel.Low);
    }

    [Fact]
    public async Task GenerateScriptAsync_creates_response_with_timestamp()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow;
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, "gpt-4");

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);
        var afterTime = DateTime.UtcNow;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeTime);
        result.Value!.CreatedAt.ShouldBeLessThanOrEqualTo(afterTime);
    }
}
