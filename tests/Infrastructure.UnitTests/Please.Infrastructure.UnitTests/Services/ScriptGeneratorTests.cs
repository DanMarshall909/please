using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Infrastructure.Services;
using Shouldly;
using Xunit;

namespace Please.Infrastructure.UnitTests.Services;

public class ScriptGeneratorTests
{
    private readonly ScriptGenerator _scriptGenerator;
    private readonly IProviderFactory _mockProviderFactory;
    private readonly IProvider _mockProvider;
    private readonly ILogger<ScriptGenerator> _mockLogger;

    public ScriptGeneratorTests()
    {
        _mockProviderFactory = Substitute.For<IProviderFactory>();
        _mockProvider = Substitute.For<IProvider>();
        _mockLogger = Substitute.For<ILogger<ScriptGenerator>>();

        // Setup mock provider to return success by default
        _mockProvider.GenerateScriptAsync(Arg.Any<ScriptRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("Get-ChildItem"));
        _mockProvider.GetDefaultModel().Returns("gpt-3.5-turbo");
        _mockProvider.IsAvailableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        _mockProviderFactory.CreateProvider(Arg.Any<ProviderType>())
            .Returns(_mockProvider);

        _scriptGenerator = new ScriptGenerator(_mockProviderFactory, _mockLogger);
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

        _mockProvider.GenerateScriptAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("Get-ChildItem"));

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Script.ShouldBe("Get-ChildItem");
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
    public async Task GenerateScriptAsync_when_provider_fails_returns_failure()
    {
        // Arrange
        var request = ScriptRequest.Create(
            "test task",
            ProviderType.OpenAi,
            "gpt-4"
        );

        _mockProvider.GenerateScriptAsync(Arg.Any<ScriptRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Failure("API error"));

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("API error");
    }

    [Fact]
    public async Task GenerateScriptAsync_uses_fallback_model_when_no_model_specified()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, null);
        _mockProvider.GetDefaultModel().Returns("gpt-3.5-turbo");
        _mockProvider.GenerateScriptAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("test script"));

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Model.ShouldBe("gpt-3.5-turbo");
    }

    [Fact]
    public async Task GenerateScriptAsync_detects_script_type_when_not_provided()
    {
        // Arrange
        var request = ScriptRequest.Create("list files", ProviderType.OpenAi, "gpt-4");
        _mockProvider.GenerateScriptAsync(Arg.Any<ScriptRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("Get-ChildItem"));

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ScriptType.ShouldBe(ScriptType.PowerShell);
    }

    [Theory]
    [InlineData("remove-item file.txt", RiskLevel.High)]
    [InlineData("new-item -path test", RiskLevel.Medium)]
    [InlineData("get-childitem", RiskLevel.Low)]
    public async Task GenerateScriptAsync_assesses_risk_level_correctly(string script, RiskLevel expectedRisk)
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, "gpt-4");
        _mockProvider.GenerateScriptAsync(Arg.Any<ScriptRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(script));

        // Act
        var result = await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.RiskLevel.ShouldBe(expectedRisk);
    }

    [Fact]
    public async Task IsProviderAvailableAsync_returns_provider_availability()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, "gpt-4");
        _mockProvider.IsAvailableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _scriptGenerator.IsProviderAvailableAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task IsProviderAvailableAsync_handles_provider_exception()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, "gpt-4");
        _mockProviderFactory.When(x => x.CreateProvider(Arg.Any<ProviderType>()))
            .Do(_ => throw new Exception("Provider error"));

        // Act
        var result = await _scriptGenerator.IsProviderAvailableAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public void GetFallbackModel_returns_provider_default_model()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, "some-model");
        _mockProvider.GetDefaultModel().Returns("gpt-3.5-turbo");

        // Act
        var result = _scriptGenerator.GetFallbackModel(request);

        // Assert
        result.ShouldBe("gpt-3.5-turbo");
    }

    [Fact]
    public void GetFallbackModel_returns_ultimate_fallback_on_exception()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.OpenAi, "some-model");
        _mockProviderFactory.When(x => x.CreateProvider(Arg.Any<ProviderType>()))
            .Do(_ => throw new Exception("Provider error"));

        // Act
        var result = _scriptGenerator.GetFallbackModel(request);

        // Assert
        result.ShouldBe("gpt-3.5-turbo");
    }

    [Fact]
    public async Task GenerateScriptAsync_calls_provider_factory_with_correct_provider_type()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", ProviderType.Anthropic, "claude-3");

        // Act
        await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        _mockProviderFactory.Received(1).CreateProvider(ProviderType.Anthropic);
    }

    [Fact]
    public async Task GenerateScriptAsync_uses_default_provider_when_not_specified()
    {
        // Arrange
        var request = ScriptRequest.Create("test task", (ProviderType?)null, "model");

        // Mock Ollama as unavailable so it falls back to OpenAI
        var ollamaProvider = Substitute.For<IProvider>();
        ollamaProvider.IsAvailableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(false));

        _mockProviderFactory.CreateProvider(ProviderType.Ollama)
            .Returns(ollamaProvider);
        _mockProviderFactory.CreateProvider(ProviderType.OpenAi)
            .Returns(_mockProvider);

        // Act
        await _scriptGenerator.GenerateScriptAsync(request);

        // Assert
        _mockProviderFactory.Received(1).CreateProvider(ProviderType.Ollama);
        _mockProviderFactory.Received(2).CreateProvider(ProviderType.OpenAi);
    }
}
