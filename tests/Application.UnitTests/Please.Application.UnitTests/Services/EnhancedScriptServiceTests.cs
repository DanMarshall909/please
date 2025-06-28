using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using Shouldly;

namespace Please.Application.UnitTests.Services;

public class EnhancedScriptServiceTests
{
    private readonly IScriptGenerator _generator;
    private readonly IScriptRepository _repository;
    private readonly IScriptValidationService _validationService;
    private readonly IConsoleUIService _consoleUI;
    private readonly ILogger<EnhancedScriptService> _logger;
    private readonly EnhancedScriptService _service;

    public EnhancedScriptServiceTests()
    {
        _generator = Substitute.For<IScriptGenerator>();
        _repository = Substitute.For<IScriptRepository>();
        _validationService = Substitute.For<IScriptValidationService>();
        _consoleUI = Substitute.For<IConsoleUIService>();
        _logger = Substitute.For<ILogger<EnhancedScriptService>>();
        
        _service = new EnhancedScriptService(_generator, _repository, _validationService, _logger, _consoleUI);
    }

    private static ScriptRequest CreateScriptRequest(string task = "test task")
    {
        return ScriptRequest.Create(task);
    }

    private static ScriptResponse CreateScriptResponse(string script = "Get-Date", RiskLevel risk = RiskLevel.Low)
    {
        return ScriptResponse.Create(
            script,
            "test task",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.PowerShell,
            risk);
    }

    [Fact]
    public async Task Generates_script_successfully_when_no_validation_errors()
    {
        // Arrange
        var request = CreateScriptRequest();
        var response = CreateScriptResponse();
        
        _generator.GenerateScriptAsync(Arg.Any<ScriptRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ScriptResponse>.Success(response));
        
        _validationService.EnhanceWithValidation(Arg.Any<ScriptResponse>())
            .Returns(response);
        
        _validationService.ValidateSyntax(Arg.Any<string>(), Arg.Any<ScriptType>())
            .Returns(new List<string>()); // No errors
        
        _repository.SaveScriptAsync(Arg.Any<ScriptResponse>(), Arg.Any<CancellationToken>())
            .Returns(VoidResult.Success);

        // Act
        var result = await _service.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(response);
        
        await _generator.Received(1).GenerateScriptAsync(request, Arg.Any<CancellationToken>());
        _validationService.Received(1).EnhanceWithValidation(response);
        await _repository.Received(1).SaveScriptAsync(response, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Retries_script_generation_when_validation_errors_detected()
    {
        // Arrange
        var request = CreateScriptRequest();
        var originalResponse = CreateScriptResponse("Get-ComputerName"); // Invalid cmdlet
        var fixedResponse = CreateScriptResponse("$env:COMPUTERNAME"); // Fixed script
        
        // First generation returns invalid script
        _generator.GenerateScriptAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<ScriptResponse>.Success(originalResponse));
        
        // Validation detects syntax errors
        _validationService.EnhanceWithValidation(originalResponse)
            .Returns(originalResponse);
        
        _validationService.ValidateSyntax("Get-ComputerName", ScriptType.PowerShell)
            .Returns(new List<string> { "Cmdlet 'Get-ComputerName' does not exist" });
        
        // Mock progress display for fix attempt
        _consoleUI.DisplayProgressAsync(
            Arg.Is<string>(s => s.Contains("Fixing script issues")),
            Arg.Any<Func<Task<Result<ScriptResponse>>>>())
            .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());
        
        // Fix generation succeeds
        _generator.GenerateFixedScriptAsync(
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<ScriptRequest>(), 
            Arg.Any<CancellationToken>())
            .Returns(Result<ScriptResponse>.Success(fixedResponse));
        
        // Fixed script has no validation errors
        _validationService.EnhanceWithValidation(fixedResponse)
            .Returns(fixedResponse);
        
        _validationService.ValidateSyntax("$env:COMPUTERNAME", ScriptType.PowerShell)
            .Returns(new List<string>()); // No errors
        
        _repository.SaveScriptAsync(Arg.Any<ScriptResponse>(), Arg.Any<CancellationToken>())
            .Returns(VoidResult.Success);

        // Act
        var result = await _service.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(fixedResponse);
        
        // Verify retry flow
        _consoleUI.Received(1).DisplayScript(
            Arg.Is<string>(s => s.Contains("Script validation detected") && s.Contains("issue(s)")),
            Arg.Is<string>(s => s.Contains("Retry Attempt")));
        
        await _generator.Received(1).GenerateFixedScriptAsync(
            "Get-ComputerName",
            Arg.Is<string>(s => s.Contains("Cmdlet 'Get-ComputerName' does not exist")),
            request,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Stops_retrying_after_max_attempts_reached()
    {
        // Arrange
        var request = CreateScriptRequest();
        var brokenResponse = CreateScriptResponse("Get-NonExistentCmdlet");
        
        _generator.GenerateScriptAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<ScriptResponse>.Success(brokenResponse));
        
        _validationService.EnhanceWithValidation(brokenResponse)
            .Returns(brokenResponse);
        
        // Always return validation errors
        _validationService.ValidateSyntax(Arg.Any<string>(), Arg.Any<ScriptType>())
            .Returns(new List<string> { "Syntax error" });
        
        // Mock fix attempts that also fail
        _generator.GenerateFixedScriptAsync(
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<ScriptRequest>(), 
            Arg.Any<CancellationToken>())
            .Returns(Result<ScriptResponse>.Success(brokenResponse)); // Still broken
        
        _consoleUI.DisplayProgressAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<Result<ScriptResponse>>>>())
            .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());
        
        _repository.SaveScriptAsync(Arg.Any<ScriptResponse>(), Arg.Any<CancellationToken>())
            .Returns(VoidResult.Success);

        // Act
        var result = await _service.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue(); // Still saves the final attempt
        
        // Should attempt fixes up to max retry limit (3 attempts total)
        await _generator.Received(2).GenerateFixedScriptAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ScriptRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_failure_when_initial_generation_fails()
    {
        // Arrange
        var request = CreateScriptRequest();
        
        _generator.GenerateScriptAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<ScriptResponse>.Failure("API connection failed"));

        // Act
        var result = await _service.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("API connection failed");
        
        _validationService.DidNotReceive().EnhanceWithValidation(Arg.Any<ScriptResponse>());
        await _repository.DidNotReceive().SaveScriptAsync(Arg.Any<ScriptResponse>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_failure_when_save_fails()
    {
        // Arrange
        var request = CreateScriptRequest();
        var response = CreateScriptResponse();
        
        _generator.GenerateScriptAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<ScriptResponse>.Success(response));
        
        _validationService.EnhanceWithValidation(response)
            .Returns(response);
        
        _validationService.ValidateSyntax(Arg.Any<string>(), Arg.Any<ScriptType>())
            .Returns(new List<string>()); // No validation errors
        
        _repository.SaveScriptAsync(response, Arg.Any<CancellationToken>())
            .Returns(VoidResult.Failure("Database error"));

        // Act
        var result = await _service.GenerateScriptAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Failed to save script: Database error");
    }

    [Fact]
    public async Task Respects_cancellation_token()
    {
        // Arrange
        var request = CreateScriptRequest();
        var response = CreateScriptResponse();
        var cts = new CancellationTokenSource();
        
        _generator.GenerateScriptAsync(request, cts.Token)
            .Returns(Result<ScriptResponse>.Success(response));
        
        _validationService.EnhanceWithValidation(response)
            .Returns(response);
        
        _validationService.ValidateSyntax(Arg.Any<string>(), Arg.Any<ScriptType>())
            .Returns(new List<string>()); // No errors
        
        _repository.SaveScriptAsync(response, cts.Token)
            .Returns(VoidResult.Success);

        // Act
        var result = await _service.GenerateScriptAsync(request, cts.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _generator.Received(1).GenerateScriptAsync(request, cts.Token);
        await _repository.Received(1).SaveScriptAsync(response, cts.Token);
    }
}