using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Please.Infrastructure.Serialization;

namespace Please.Infrastructure.Providers;

/// <summary>
/// Anthropic API provider implementation
/// </summary>
public class AnthropicProvider : IProvider
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicConfiguration _configuration;
    private readonly ILogger<AnthropicProvider> _logger;

    public AnthropicProvider(HttpClient httpClient, AnthropicConfiguration configuration, ILogger<AnthropicProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);

        if (!string.IsNullOrEmpty(_configuration.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }
    }

    public async Task<Result<string>> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_configuration.ApiKey))
            {
                return Result<string>.Failure("Anthropic API key not configured");
            }

            var model = request.Model ?? _configuration.DefaultModel;
            var systemPrompt = buildSystemPrompt(request);
            var userPrompt = buildUserPrompt(request);

            var requestBody = new AnthropicRequest
            {
                Model = model,
                MaxTokens = 1000,
                System = systemPrompt,
                Messages = new[]
                {
                    new AnthropicMessage { Role = "user", Content = userPrompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody, ApiSerializationContext.Default.AnthropicRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to Anthropic with model {Model}", model);

            var response = await _httpClient.PostAsync("/messages", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Anthropic API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<string>.Failure($"Anthropic API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.AnthropicResponse);

            var script = apiResponse?.Content?.FirstOrDefault()?.Text?.Trim();

            if (string.IsNullOrEmpty(script))
            {
                return Result<string>.Failure("Empty response from Anthropic");
            }

            _logger.LogInformation("Successfully generated script using Anthropic");
            return Result<string>.Success(script);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Anthropic API");
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Anthropic API request timed out");
            return Result<string>.Failure("Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Anthropic API");
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

            // Anthropic doesn't have a models endpoint, but we can test with a minimal request
            var testBody = new AnthropicRequest
            {
                Model = _configuration.DefaultModel,
                MaxTokens = 1,
                Messages = new[] { new AnthropicMessage { Role = "user", Content = "test" } }
            };

            var json = JsonSerializer.Serialize(testBody, ApiSerializationContext.Default.AnthropicRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/messages", content, cancellationToken);
            return Result<bool>.Success(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest);
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
            "claude-3-5-sonnet-20241022",
            "claude-3-5-haiku-20241022",
            "claude-3-opus-20240229",
            "claude-3-sonnet-20240229",
            "claude-3-haiku-20240307"
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
