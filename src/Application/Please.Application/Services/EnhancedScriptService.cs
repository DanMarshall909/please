using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Please.Application.Services;

public sealed class EnhancedScriptService : IScriptService
{
    private readonly IScriptGenerator _generator;
    private readonly IScriptRepository _repository;
    private readonly IScriptValidationService _validationService;
    private readonly ILogger<EnhancedScriptService> _logger;
    private readonly IConsoleUIService _consoleUI;
    private const int MaxRetryAttempts = 3;

    public EnhancedScriptService(
        IScriptGenerator generator, 
        IScriptRepository repository,
        IScriptValidationService validationService,
        ILogger<EnhancedScriptService> logger,
        IConsoleUIService consoleUI)
    {
        _generator = generator;
        _repository = repository;
        _validationService = validationService;
        _logger = logger;
        _consoleUI = consoleUI;
    }

    public async Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        return await GenerateScriptWithRetryAsync(request, cancellationToken);
    }

    private async Task<Result<ScriptResponse>> GenerateScriptWithRetryAsync(
        ScriptRequest request,
        CancellationToken cancellationToken,
        int attemptNumber = 1)
    {
        _logger.LogInformation("Generating script (attempt {AttemptNumber})", attemptNumber);
        
        // Generate the script
        var generationResult = await _generator.GenerateScriptAsync(request, cancellationToken);
        if (!generationResult.IsSuccess)
        {
            _logger.LogWarning("Generation failed: {Error}", generationResult.Error);
            return Result<ScriptResponse>.Failure(generationResult.Error);
        }

        var scriptResponse = generationResult.Value!;

        // Validate the generated script
        _logger.LogInformation("Validating generated script");
        var enhancedResponse = _validationService.EnhanceWithValidation(scriptResponse);
        
        // Check for syntax errors
        var syntaxErrors = _validationService.ValidateSyntax(enhancedResponse.Script, enhancedResponse.ScriptType);
        
        // If there are syntax errors and we haven't exceeded retry attempts
        if (syntaxErrors.Any() && attemptNumber < MaxRetryAttempts)
        {
            _logger.LogWarning("Script validation failed with {ErrorCount} errors, attempting to fix", syntaxErrors.Count);
            
            // Notify user about validation issues and retry
            _consoleUI.DisplayScript(
                $"🔄 Script validation detected {syntaxErrors.Count} issue(s). Automatically fixing...",
                $"Retry Attempt {attemptNumber}/{MaxRetryAttempts}");
            
            // Create error feedback for the LLM
            var errorFeedback = string.Join("\n", syntaxErrors);
            var fixRequest = $"The previous script had the following errors:\n{errorFeedback}\n\nPlease generate a corrected version that fixes these issues.";
            
            // Use the generator's fix capability with progress indicator
            var fixResult = await _consoleUI.DisplayProgressAsync(
                $"🛠️ Fixing script issues (attempt {attemptNumber + 1}/{MaxRetryAttempts})...",
                async () => await _generator.GenerateFixedScriptAsync(
                    enhancedResponse.Script, 
                    fixRequest, 
                    request, 
                    cancellationToken));
            
            if (fixResult.IsSuccess)
            {
                // Recursively retry with the fixed script
                return await GenerateScriptWithRetryAsync(request, cancellationToken, attemptNumber + 1);
            }
            else
            {
                _logger.LogWarning("Failed to generate fixed script: {Error}", fixResult.Error);
            }
        }

        // Save the final script (even if it has some warnings)
        _logger.LogInformation("Saving script");
        var saveResult = await _repository.SaveScriptAsync(enhancedResponse, cancellationToken);
        if (!saveResult.IsSuccess)
        {
            _logger.LogError("Failed to save script: {Error}", saveResult.Error);
            return Result<ScriptResponse>.Failure($"Failed to save script: {saveResult.Error}");
        }

        _logger.LogInformation("Script generated successfully after {AttemptCount} attempt(s)", attemptNumber);
        return Result<ScriptResponse>.Success(enhancedResponse);
    }
}