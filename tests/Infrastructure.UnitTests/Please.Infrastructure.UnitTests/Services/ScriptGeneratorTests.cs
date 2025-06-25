using Please.Domain.Enums;
using Please.Infrastructure.Services;
using Please.TestUtilities.Builders;

namespace Please.Infrastructure.UnitTests.Services;

public class ScriptGeneratorTests
{
    private readonly ScriptGenerator _scriptGenerator= new();

    [Fact]
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
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Script.ShouldNotBeEmpty();
        result.Value.TaskDescription.ShouldBe("list files in current directory");
        result.Value.Provider.ShouldBe(ProviderType.OpenAi);
        result.Value.Model.ShouldBe("gpt-4");
    }

    [Fact]
    public async Task GenerateScriptAsync_with_null_request_should_return_failure()
    {
        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("request");
    }

    [Fact]
    public async Task GenerateScriptAsync_with_empty_task_description_should_return_failure()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithTask("")
            .Build();

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("task description");
    }

    [Fact]
    public async Task IsProviderAvailableAsync_with_openai_provider_should_return_true()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithProvider(ProviderType.OpenAi)
            .Build();

        // Act
        var result = await _scriptGenerator.IsProviderAvailableAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task IsProviderAvailableAsync_with_anthropic_provider_should_return_true()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithProvider(ProviderType.Anthropic)
            .Build();

        // Act
        var result = await _scriptGenerator.IsProviderAvailableAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
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
        fallbackModel.ShouldBe("gpt-3.5-turbo");
    }

    [Fact]
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
        fallbackModel.ShouldBe("claude-3-haiku-20240307");
    }

    [Fact]
    public async Task GenerateScriptAsync_should_detect_powershell_script_type()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithTask("get current date using powershell")
            .Build();

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ScriptType.ShouldBe(ScriptType.PowerShell);
    }

    [Fact]
    public async Task GenerateScriptAsync_should_detect_bash_script_type()
    {
        // Arrange
        var request = new ScriptRequestBuilder()
            .WithTask("list files using bash")
            .Build();

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ScriptType.ShouldBe(ScriptType.Bash);
    }
}
