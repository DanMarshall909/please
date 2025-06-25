using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Please.Infrastructure.Serialization;

namespace Please.Infrastructure.Providers;

/// <summary>
/// Google Gemini API provider implementation
/// </summary>
public class GeminiProvider : IProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiConfiguration _configuration;
    private readonly ILogger<GeminiProvider> _logger;

    public GeminiProvider(HttpClient httpClient, GeminiConfiguration configuration, ILogger<GeminiProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);
    }

    public async Task<Result<string>> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_configuration.ApiKey))
            {
                return Result<string>.Failure("Gemini API key not configured");
            }

            var model = request.Model ?? _configuration.DefaultModel;
            var prompt = buildPrompt(request);

            var requestBody = new GeminiRequest
            {
                Contents = new[]
                {
                    new GeminiContentItem
                    {
                        Parts = new[]
                        {
                            new GeminiPart { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = 0.1,
                    MaxOutputTokens = 1000
                }
            };

            var json = JsonSerializer.Serialize(requestBody, ApiSerializationContext.Default.GeminiRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to Gemini with model {Model}", model);

            var url = $"/models/{model}:generateContent?key={_configuration.ApiKey}";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<string>.Failure($"Gemini API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.GeminiResponse);

            var script = apiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim();

            if (string.IsNullOrEmpty(script))
            {
                return Result<string>.Failure("Empty response from Gemini");
            }

            _logger.LogInformation("Successfully generated script using Gemini");
            return Result<string>.Success(script);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Gemini API");
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Gemini API request timed out");
            return Result<string>.Failure("Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Gemini API");
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

            var url = $"/models?key={_configuration.ApiKey}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
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
            "gemini-pro",
            "gemini-pro-vision",
            "gemini-1.5-pro",
            "gemini-1.5-flash"
        };
    }

    private string buildPrompt(ScriptRequest request)
    {
        var scriptTypeHint = request.ScriptType?.ToString().ToLower() ?? "shell script";
        var platform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "Windows" : "Unix/Linux";

        var prompt = new StringBuilder();
        prompt.AppendLine($"You are an expert {scriptTypeHint} developer. Generate safe, efficient, and well-commented scripts.");
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

        prompt.AppendLine();
        prompt.AppendLine("Script:");

        return prompt.ToString();
    }
}
