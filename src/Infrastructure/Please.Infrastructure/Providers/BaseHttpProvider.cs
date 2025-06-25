using System.Text;
using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Providers;

/// <summary>
/// Base class for HTTP-based AI providers that handles common functionality
/// </summary>
public abstract class BaseHttpProvider<TConfig> : IProvider
{
    protected readonly HttpClient HttpClient;
    protected readonly TConfig Configuration;
    protected readonly ILogger Logger;

    protected BaseHttpProvider(HttpClient httpClient, TConfig configuration, ILogger logger)
    {
        HttpClient = httpClient;
        Configuration = configuration;
        Logger = logger;

        ConfigureHttpClient();
    }

    public async Task<Result<string>> GenerateScriptAsync(ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConfigurationValid()) return Result<string>.Failure(GetConfigurationErrorMessage());

            string model = GetModel(request);
            Logger.LogInformation("Sending request to {ProviderName} with model {Model}", GetProviderName(), model);

            var httpRequest = await CreateHttpRequestAsync(request, model, cancellationToken);
            var response = await HttpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("{ProviderName} API error: {StatusCode} - {Content}", GetProviderName(),
                    response.StatusCode, errorContent);
                return Result<string>.Failure($"{GetProviderName()} API error: {response.StatusCode}");
            }

            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            string script = await ExtractScriptFromResponseAsync(responseContent, cancellationToken);

            if (string.IsNullOrEmpty(script)) return Result<string>.Failure($"Empty response from {GetProviderName()}");

            Logger.LogInformation("Successfully generated script using {ProviderName}", GetProviderName());
            return Result<string>.Success(script);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP error calling {ProviderName} API", GetProviderName());
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "{ProviderName} API request timed out", GetProviderName());
            return Result<string>.Failure("Request timed out");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error calling {ProviderName} API", GetProviderName());
            return Result<string>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConfigurationValid()) return Result<bool>.Success(false);

            var healthCheckRequest = CreateHealthCheckRequest();
            var response = await HttpClient.SendAsync(healthCheckRequest, cancellationToken);
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch
        {
            return Result<bool>.Success(false);
        }
    }

    public abstract string GetDefaultModel();
    public abstract string[] GetSupportedModels();

    // Protected methods that subclasses can override or implement
    protected virtual void ConfigureHttpClient()
    {
    }

    protected abstract bool IsConfigurationValid();
    protected abstract string GetConfigurationErrorMessage();
    protected abstract string GetProviderName();
    protected abstract string GetModel(ScriptRequest request);

    protected abstract Task<HttpRequestMessage> CreateHttpRequestAsync(ScriptRequest request, string model,
        CancellationToken cancellationToken);

    protected abstract Task<string> ExtractScriptFromResponseAsync(string responseContent,
        CancellationToken cancellationToken);

    protected abstract HttpRequestMessage CreateHealthCheckRequest();

    // Utility methods for prompt building
    protected virtual string BuildSystemPrompt(ScriptRequest request)
    {
        string scriptTypeHint = request.ScriptType?.ToString().ToLower() ?? "shell script";
        string platform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "Windows" : "Unix/Linux";

        return $@"You are an expert {scriptTypeHint} developer. Generate safe, efficient, and well-commented scripts.

Guidelines:
- Write for {platform} platform
- Include error handling where appropriate
- Add helpful comments explaining complex logic
- Use best practices for the script type
- Keep the script focused on the specific task
- Return ONLY the script code, no additional text or explanations";
    }

    protected virtual string BuildUserPrompt(ScriptRequest request)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"Task: {request.TaskDescription}");

        if (!string.IsNullOrEmpty(request.WorkingDirectory))
            prompt.AppendLine($"Working Directory: {request.WorkingDirectory}");

        if (request.AdditionalParameters.Any())
        {
            prompt.AppendLine("Additional Context:");
            foreach (var param in request.AdditionalParameters) prompt.AppendLine($"- {param.Key}: {param.Value}");
        }

        return prompt.ToString();
    }

    protected virtual string BuildPrompt(ScriptRequest request)
    {
        string scriptTypeHint = request.ScriptType?.ToString().ToLower() ?? "shell script";
        string platform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "Windows" : "Unix/Linux";

        var prompt = new StringBuilder();
        prompt.AppendLine(
            $"You are an expert {scriptTypeHint} developer. Generate safe, efficient, and well-commented scripts.");
        prompt.AppendLine();
        prompt.AppendLine("Guidelines:");
        prompt.AppendLine($"- Write for {platform} platform");
        prompt.AppendLine("- Include error handling where appropriate");
        prompt.AppendLine("- Add helpful comments explaining complex logic");
        prompt.AppendLine("- Use best practices for the script type");
        prompt.AppendLine("- Keep the script focused on the specific task");
        prompt.AppendLine("- Return ONLY the script code, no additional text or explanations");
        prompt.AppendLine();
        prompt.AppendLine($"Task: {request.TaskDescription}");

        if (!string.IsNullOrEmpty(request.WorkingDirectory))
            prompt.AppendLine($"Working Directory: {request.WorkingDirectory}");

        if (request.AdditionalParameters.Any())
        {
            prompt.AppendLine("Additional Context:");
            foreach (var param in request.AdditionalParameters) prompt.AppendLine($"- {param.Key}: {param.Value}");
        }

        prompt.AppendLine();
        prompt.AppendLine("Script:");

        return prompt.ToString();
    }
}
