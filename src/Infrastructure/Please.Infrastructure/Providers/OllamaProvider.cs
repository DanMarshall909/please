using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Providers;

/// <summary>
/// Ollama API provider implementation
/// </summary>
public class OllamaProvider : IProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaConfiguration _configuration;
    private readonly ILogger<OllamaProvider> _logger;

    public OllamaProvider(HttpClient httpClient, OllamaConfiguration configuration, ILogger<OllamaProvider> logger)
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
            var model = request.Model ?? _configuration.DefaultModel;
            var prompt = buildPrompt(request);

            var requestBody = new
            {
                model = model,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.1,
                    num_predict = 1000
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to Ollama with model {Model}", model);

            var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Ollama API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<string>.Failure($"Ollama API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent);

            var script = apiResponse?.Response?.Trim();

            if (string.IsNullOrEmpty(script))
            {
                return Result<string>.Failure("Empty response from Ollama");
            }

            _logger.LogInformation("Successfully generated script using Ollama");
            return Result<string>.Success(script);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Ollama API");
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Ollama API request timed out");
            return Result<string>.Failure("Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Ollama API");
            return Result<string>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
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
            "llama2",
            "llama2:13b",
            "llama2:70b",
            "codellama",
            "codellama:13b",
            "codellama:34b",
            "mistral",
            "mixtral",
            "phi",
            "gemma"
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

/// <summary>
/// Ollama API response structure
/// </summary>
internal class OllamaResponse
{
    public string? Response { get; set; }
    public bool? Done { get; set; }
}
