using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Entities;
using Please.Infrastructure.Serialization;

namespace Please.Infrastructure.Providers;

/// <summary>
/// Google Gemini API provider implementation
/// </summary>
public class GeminiProvider : BaseHttpProvider<GeminiConfiguration>
{
    public GeminiProvider(HttpClient httpClient, GeminiConfiguration configuration, ILogger<GeminiProvider> logger)
        : base(httpClient, configuration, logger)
    {
    }

    protected override void ConfigureHttpClient()
    {
        // Ensure BaseUrl ends with '/' for proper URI combination
        string baseUrl = Configuration.BaseUrl.TrimEnd('/') + "/";
        HttpClient.BaseAddress = new Uri(baseUrl);
        HttpClient.Timeout = TimeSpan.FromSeconds(Configuration.TimeoutSeconds);
    }

    protected override bool IsConfigurationValid() => !string.IsNullOrEmpty(Configuration.ApiKey);

    protected override string GetConfigurationErrorMessage() => "Gemini API key not configured";

    protected override string GetProviderName() => "Gemini";

    protected override string GetModel(ScriptRequest request) => request.Model ?? Configuration.DefaultModel;

    protected override Task<HttpRequestMessage> CreateHttpRequestAsync(ScriptRequest request, string model,
        CancellationToken cancellationToken)
    {
        string prompt = BuildPrompt(request);

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

        string json = JsonSerializer.Serialize(requestBody, ApiSerializationContext.Default.GeminiRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"models/{model}:generateContent?key={Configuration.ApiKey}";
        return Task.FromResult(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        });
    }

    protected override Task<string> ExtractScriptFromResponseAsync(string responseContent,
        CancellationToken cancellationToken)
    {
        var apiResponse = JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.GeminiResponse);
        string script = apiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim() ??
                        string.Empty;
        return Task.FromResult(script);
    }

    protected override HttpRequestMessage CreateHealthCheckRequest()
    {
        var url = $"models?key={Configuration.ApiKey}";
        return new HttpRequestMessage(HttpMethod.Get, url);
    }

    public override string GetDefaultModel() => Configuration.DefaultModel;

    public override string[] GetSupportedModels()
    {
        return new[]
        {
            "gemini-pro",
            "gemini-pro-vision",
            "gemini-1.5-pro",
            "gemini-1.5-flash"
        };
    }
}
