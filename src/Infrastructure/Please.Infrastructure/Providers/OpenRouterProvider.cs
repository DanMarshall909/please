using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Please.Infrastructure.Serialization;

namespace Please.Infrastructure.Providers;

/// <summary>
/// OpenRouter API provider implementation
/// </summary>
public class OpenRouterProvider : IProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterConfiguration _configuration;
    private readonly ILogger<OpenRouterProvider> _logger;

    public OpenRouterProvider(HttpClient httpClient, OpenRouterConfiguration configuration, ILogger<OpenRouterProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);

        if (!string.IsNullOrEmpty(_configuration.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/please-cli");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "Please CLI");
        }
    }

    public async Task<Result<string>> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_configuration.ApiKey))
            {
                return Result<string>.Failure("OpenRouter API key not configured");
            }

            var model = request.Model ?? _configuration.DefaultModel;
            var systemPrompt = buildSystemPrompt(request);
            var userPrompt = buildUserPrompt(request);

            var requestBody = new OpenRouterRequest
            {
                Model = model,
                Messages = new[]
                {
                    new OpenRouterMessage { Role = "system", Content = systemPrompt },
                    new OpenRouterMessage { Role = "user", Content = userPrompt }
                },
                Temperature = 0.1,
                MaxTokens = 1000
            };

            var json = JsonSerializer.Serialize(requestBody, ApiSerializationContext.Default.OpenRouterRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to OpenRouter with model {Model}", model);

            var response = await _httpClient.PostAsync("/chat/completions", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenRouter API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<string>.Failure($"OpenRouter API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.OpenRouterResponse);

            var script = apiResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrEmpty(script))
            {
                return Result<string>.Failure("Empty response from OpenRouter");
            }

            _logger.LogInformation("Successfully generated script using OpenRouter");
            return Result<string>.Success(script);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling OpenRouter API");
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OpenRouter API request timed out");
            return Result<string>.Failure("Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling OpenRouter API");
            return Result<string>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_configuration.ApiKey))
            {
                return Result<bool>.Success(false);
            }

            var response = await _httpClient.GetAsync("/models", cancellationToken);
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch
        {
            return Result<bool>.Success(false);
        }
    }

    public string GetDefaultModel()
    {
        return _configuration.DefaultModel;
    }

    public string[] GetSupportedModels()
    {
        return new[]
        {
            "microsoft/wizardlm-2-8x22b",
            "microsoft/wizardlm-2-7b",
            "anthropic/claude-3-opus",
            "anthropic/claude-3-sonnet",
            "anthropic/claude-3-haiku",
            "openai/gpt-4o",
            "openai/gpt-4o-mini",
            "openai/gpt-4-turbo",
            "meta-llama/llama-3-8b-instruct",
            "meta-llama/llama-3-70b-instruct",
            "mistralai/mixtral-8x7b-instruct",
            "google/gemini-pro"
        };
    }

    private string buildSystemPrompt(ScriptRequest request)
    {
        var scriptTypeHint = request.ScriptType?.ToString().ToLower() ?? "shell script";
        var platform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "Windows" : "Unix/Linux";

        return $@"You are an expert {scriptTypeHint} developer. Generate safe, efficient, and well-commented scripts.

Guidelines:
- Write for {platform} platform
- Include error handling where appropriate
- Add helpful comments explaining complex logic
- Use best practices for the script type
- Keep the script focused on the specific task
- Return ONLY the script code, no additional text or explanations";
    }

    private string buildUserPrompt(ScriptRequest request)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"Task: {request.TaskDescription}");

        if (!string.IsNullOrEmpty(request.WorkingDirectory))
        {
            prompt.AppendLine($"Working Directory: {request.WorkingDirectory}");
        }

        if (request.AdditionalParameters.Any())
        {
            prompt.AppendLine("Additional Context:");
            foreach (var param in request.AdditionalParameters)
            {
                prompt.AppendLine($"- {param.Key}: {param.Value}");
            }
        }

        return prompt.ToString();
    }
}
