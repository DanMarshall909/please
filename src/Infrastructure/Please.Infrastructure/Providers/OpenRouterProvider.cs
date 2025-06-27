using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Entities;
using Please.Infrastructure.Serialization;

namespace Please.Infrastructure.Providers;

/// <summary>
/// OpenRouter API provider implementation
/// </summary>
public class OpenRouterProvider : BaseHttpProvider<OpenRouterConfiguration>
{
    public OpenRouterProvider(HttpClient httpClient, OpenRouterConfiguration configuration,
        ILogger<OpenRouterProvider> logger)
        : base(httpClient, configuration, logger)
    {
    }

    protected override void ConfigureHttpClient()
    {
        // Ensure BaseUrl ends with '/' for proper URI combination
        string baseUrl = Configuration.BaseUrl.TrimEnd('/') + "/";
        HttpClient.BaseAddress = new Uri(baseUrl);
        HttpClient.Timeout = TimeSpan.FromSeconds(Configuration.TimeoutSeconds);

        if (!string.IsNullOrEmpty(Configuration.ApiKey))
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Configuration.ApiKey);
            HttpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/please-cli");
            HttpClient.DefaultRequestHeaders.Add("X-Title", "Please CLI");
        }
    }

    protected override bool IsConfigurationValid() => !string.IsNullOrEmpty(Configuration.ApiKey);

    protected override string GetConfigurationErrorMessage() => "OpenRouter API key not configured";

    protected override string GetProviderName() => "OpenRouter";

    protected override string GetModel(ScriptRequest request) => request.Model ?? Configuration.DefaultModel;

    protected override Task<HttpRequestMessage> CreateHttpRequestAsync(ScriptRequest request, string model,
        CancellationToken cancellationToken)
    {
        string systemPrompt = BuildSystemPrompt(request);
        string userPrompt = BuildUserPrompt(request);

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

        string json = JsonSerializer.Serialize(requestBody, ApiSerializationContext.Default.OpenRouterRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return Task.FromResult(new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = content
        });
    }

    protected override Task<string> ExtractScriptFromResponseAsync(string responseContent,
        CancellationToken cancellationToken)
    {
        var apiResponse =
            JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.OpenRouterResponse);
        return Task.FromResult(apiResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty);
    }

    protected override HttpRequestMessage CreateHealthCheckRequest() => new(HttpMethod.Get, "models");

    public override string GetDefaultModel() => Configuration.DefaultModel;

    public override string[] GetSupportedModels()
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
}
