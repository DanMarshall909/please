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
    public async Task<Result<ScriptResponse>> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return Result<ScriptResponse>.Failure("Script request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.TaskDescription))
        {
            return Result<ScriptResponse>.Failure("task description cannot be empty");
        }

        // For now, return a mock implementation
        // In a real implementation, this would call the AI provider
        await Task.Delay(1, cancellationToken); // Simulate async operation

        var scriptType = detectScriptType(request.TaskDescription);
        var script = generateMockScript(request.TaskDescription, scriptType);

        var response = ScriptResponse.Create(
            script,
            request.TaskDescription,
            request.Provider ?? ProviderType.OpenAi,
            request.Model ?? GetFallbackModel(request),
            scriptType,
            RiskLevel.Low
        );

        return Result<ScriptResponse>.Success(response);
    }

    public async Task<Result<bool>> IsProviderAvailableAsync(ScriptRequest request, CancellationToken cancellationToken = default)
    {
        // Mock implementation - in reality this would check API keys, network connectivity, etc.
        await Task.Delay(1, cancellationToken); // Simulate async operation

        return request.Provider switch
        {
            ProviderType.OpenAi => Result<bool>.Success(true),
            ProviderType.Anthropic => Result<bool>.Success(true),
            ProviderType.Ollama => Result<bool>.Success(true),
            _ => Result<bool>.Success(false)
        };
    }

    public string GetFallbackModel(ScriptRequest request)
    {
        return (request.Provider ?? ProviderType.OpenAi) switch
        {
            ProviderType.OpenAi => "gpt-3.5-turbo",
            ProviderType.Anthropic => "claude-3-haiku-20240307",
            ProviderType.Ollama => "llama2",
            _ => "gpt-3.5-turbo"
        };
    }

    private ScriptType detectScriptType(string taskDescription)
    {
        var lowerTask = taskDescription.ToLowerInvariant();

        if (lowerTask.Contains("powershell") || lowerTask.Contains("get-") || lowerTask.Contains("set-"))
        {
            return ScriptType.PowerShell;
        }

        if (lowerTask.Contains("bash") || lowerTask.Contains("linux") || lowerTask.Contains("unix"))
        {
            return ScriptType.Bash;
        }

        // Default to PowerShell on Windows
        return ScriptType.PowerShell;
    }

    private string generateMockScript(string taskDescription, ScriptType scriptType)
    {
        var lowerTask = taskDescription.ToLowerInvariant();

        return scriptType switch
        {
            ScriptType.PowerShell => lowerTask switch
            {
                var task when task.Contains("list") || task.Contains("files") => "Get-ChildItem",
                var task when task.Contains("date") || task.Contains("time") => "Get-Date",
                var task when task.Contains("process") => "Get-Process",
                _ => "# PowerShell script for: " + taskDescription
            },
            ScriptType.Bash => lowerTask switch
            {
                var task when task.Contains("list") || task.Contains("files") => "ls -la",
                var task when task.Contains("date") || task.Contains("time") => "date",
                var task when task.Contains("process") => "ps aux",
                _ => "# Bash script for: " + taskDescription
            },
            _ => "echo 'Script for: " + taskDescription + "'"
        };
    }
}
