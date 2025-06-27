using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Services;

/// <summary>
/// Service for generating scripts using AI providers
/// </summary>
public class ScriptGenerator : IScriptGenerator
{
    private readonly IProviderFactory _providerFactory;
    private readonly ILogger<ScriptGenerator> _logger;

    public ScriptGenerator(IProviderFactory providerFactory, ILogger<ScriptGenerator> logger)
    {
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<Result<ScriptResponse>> GenerateScriptAsync(ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) return Result<ScriptResponse>.Failure("Script request cannot be null");

        if (string.IsNullOrWhiteSpace(request.TaskDescription))
            return Result<ScriptResponse>.Failure("task description cannot be empty");

        try
        {
            var providerType = request.Provider ?? await getDefaultProviderAsync(cancellationToken);
            var provider = _providerFactory.CreateProvider(providerType);

            _logger.LogInformation("Generating script using {Provider} for task: {Task}",
                providerType, request.TaskDescription);

            // Ensure script type is detected if not provided
            if (request.ScriptType == null)
                request = request with { ScriptType = detectScriptType(request.TaskDescription) };

            var scriptResult = await provider.GenerateScriptAsync(request, cancellationToken);

            if (scriptResult.IsFailure)
            {
                _logger.LogWarning("Failed to generate script using {Provider}: {Error}",
                    providerType, scriptResult.Error);
                return Result<ScriptResponse>.Failure(scriptResult.Error);
            }

            string script = scriptResult.Value ?? string.Empty;
            string model = request.Model ?? provider.GetDefaultModel();
            var scriptType = request.ScriptType ?? detectScriptType(request.TaskDescription);
            var riskLevel = assessRiskLevel(script, scriptType);

            var response = ScriptResponse.Create(
                script,
                request.TaskDescription,
                providerType,
                model,
                scriptType,
                riskLevel
            );

            _logger.LogInformation("Successfully generated script using {Provider} with model {Model}",
                providerType, model);

            return Result<ScriptResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating script");
            return Result<ScriptResponse>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsProviderAvailableAsync(ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var providerType = request.Provider ?? ProviderType.OpenAi;
            var provider = _providerFactory.CreateProvider(providerType);

            return await provider.IsAvailableAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking provider availability for {Provider}", request.Provider);
            return Result<bool>.Success(false);
        }
    }

    public string GetFallbackModel(ScriptRequest request)
    {
        try
        {
            var providerType = request.Provider ?? ProviderType.OpenAi;
            var provider = _providerFactory.CreateProvider(providerType);
            return provider.GetDefaultModel();
        }
        catch
        {
            return "gpt-3.5-turbo"; // Ultimate fallback
        }
    }

    public async Task<Result<ScriptResponse>> GenerateFixedScriptAsync(
        string originalScript,
        string errorMessage,
        ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) return Result<ScriptResponse>.Failure("Script request cannot be null");

        if (string.IsNullOrWhiteSpace(errorMessage))
            return Result<ScriptResponse>.Failure("Error message cannot be empty");

        try
        {
            var providerType = request.Provider ?? await getDefaultProviderAsync(cancellationToken);
            var provider = _providerFactory.CreateProvider(providerType);

            _logger.LogInformation("Generating fixed script using {Provider} for error: {Error}",
                providerType, errorMessage);

            // Create a new request for fixing the script
            var fixRequest = createFixScriptRequest(originalScript, errorMessage, request);

            var scriptResult = await provider.GenerateScriptAsync(fixRequest, cancellationToken);

            if (scriptResult.IsFailure)
            {
                _logger.LogWarning("Failed to generate fixed script using {Provider}: {Error}",
                    providerType, scriptResult.Error);
                return Result<ScriptResponse>.Failure(scriptResult.Error);
            }

            string script = scriptResult.Value ?? string.Empty;
            string model = request.Model ?? provider.GetDefaultModel();
            var scriptType = request.ScriptType ?? detectScriptType(request.TaskDescription ?? "fix script");
            var riskLevel = assessRiskLevel(script, scriptType);

            var response = ScriptResponse.Create(
                script,
                $"Fixed script - Original error: {errorMessage}",
                providerType,
                model,
                scriptType,
                riskLevel
            );

            _logger.LogInformation("Successfully generated fixed script using {Provider} with model {Model}",
                providerType, model);

            return Result<ScriptResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating fixed script");
            return Result<ScriptResponse>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private ScriptRequest createFixScriptRequest(string originalScript, string errorMessage, ScriptRequest baseRequest)
    {
        string fixDescription = string.IsNullOrWhiteSpace(originalScript)
            ? $"Generate a script to handle this error: {errorMessage}. The original script was empty or missing."
            : $"Fix this script that has an error.\n\nOriginal Script:\n{originalScript}\n\nError Message:\n{errorMessage}\n\nPlease provide a corrected version.";

        var fixRequest = ScriptRequest.Create(fixDescription, baseRequest.Provider, baseRequest.Model);

        return fixRequest with
        {
            ScriptType = baseRequest.ScriptType,
            WorkingDirectory = baseRequest.WorkingDirectory,
            ForceExecution = baseRequest.ForceExecution,
            AdditionalParameters = baseRequest.AdditionalParameters
        };
    }

    private ScriptType detectScriptType(string taskDescription)
    {
        string lowerTask = taskDescription.ToLowerInvariant();

        if (lowerTask.Contains("powershell") || lowerTask.Contains("get-") || lowerTask.Contains("set-"))
            return ScriptType.PowerShell;

        if (lowerTask.Contains("bash") || lowerTask.Contains("linux") || lowerTask.Contains("unix"))
            return ScriptType.Bash;

        // Default to PowerShell on Windows
        return ScriptType.PowerShell;
    }

    private RiskLevel assessRiskLevel(string script, ScriptType scriptType)
    {
        string lowerScript = script.ToLowerInvariant();

        // High risk patterns
        var highRiskPatterns = new[]
        {
            "remove-item", "rm -rf", "del /f", "format", "diskpart",
            "net user", "reg delete", "shutdown", "restart-computer",
            "invoke-expression", "iex", "powershell -c", "cmd /c",
            "start-process", "& ", "wget", "curl", "download",
            "install", "uninstall", "msiexec", "setup.exe"
        };

        // Medium risk patterns
        var mediumRiskPatterns = new[]
        {
            "new-item", "mkdir", "copy", "move", "rename", "chmod",
            "chown", "export", "set-", "new-", "add-", "enable-",
            "disable-", "stop-", "start-", "restart-"
        };

        if (highRiskPatterns.Any(pattern => lowerScript.Contains(pattern))) return RiskLevel.High;

        if (mediumRiskPatterns.Any(pattern => lowerScript.Contains(pattern))) return RiskLevel.Medium;

        return RiskLevel.Low;
    }

    private async Task<ProviderType> getDefaultProviderAsync(CancellationToken cancellationToken)
    {
        try
        {
            // First preference: Ollama (local provider)
            var ollamaProvider = _providerFactory.CreateProvider(ProviderType.Ollama);
            var ollamaAvailable = await ollamaProvider.IsAvailableAsync(cancellationToken);

            if (ollamaAvailable.IsSuccess && ollamaAvailable.Value)
            {
                _logger.LogInformation("Using Ollama as default provider (available locally)");
                return ProviderType.Ollama;
            }

            // Second preference: OpenAI (most reliable cloud provider)
            var openAiProvider = _providerFactory.CreateProvider(ProviderType.OpenAi);
            var openAiAvailable = await openAiProvider.IsAvailableAsync(cancellationToken);

            if (openAiAvailable.IsSuccess && openAiAvailable.Value)
            {
                _logger.LogInformation("Using OpenAI as default provider");
                return ProviderType.OpenAi;
            }

            // Third preference: Anthropic
            var anthropicProvider = _providerFactory.CreateProvider(ProviderType.Anthropic);
            var anthropicAvailable = await anthropicProvider.IsAvailableAsync(cancellationToken);

            if (anthropicAvailable.IsSuccess && anthropicAvailable.Value)
            {
                _logger.LogInformation("Using Anthropic as default provider");
                return ProviderType.Anthropic;
            }

            // Final fallback: OpenAI (even if not available, will provide clear error)
            _logger.LogWarning("No providers available, falling back to OpenAI");
            return ProviderType.OpenAi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining default provider, falling back to OpenAI");
            return ProviderType.OpenAi;
        }
    }
}
