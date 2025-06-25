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
public class GeminiProvider : BaseHttpProvider<GeminiConfiguration>
{
    public GeminiProvider(HttpClient httpClient, GeminiConfiguration configuration, ILogger<GeminiProvider> logger)
        : base(httpClient, configuration, logger)
    {
    }

    protected override void ConfigureHttpClient()
    {
        HttpClient.BaseAddress = new Uri(Configuration.BaseUrl);
        HttpClient.Timeout = TimeSpan.FromSeconds(Configuration.TimeoutSeconds);
    }

    protected override bool IsConfigurationValid()
    {
        return !string.IsNullOrEmpty(Configuration.ApiKey);
    }

    protected override string GetConfigurationErrorMessage()
    {
        return "Gemini API key not configured";
    }

    protected override string GetProviderName()
    {
        return "Gemini";
    }

    protected override string GetModel(ScriptRequest request)
    {
        return request.Model ?? Configuration.DefaultModel;
    }

    protected override Task<HttpRequestMessage> CreateHttpRequestAsync(ScriptRequest request, string model, CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(request);

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

        var url = $"/models/{model}:generateContent?key={Configuration.ApiKey}";
        return Task.FromResult(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        });
    }

    protected override Task<string> ExtractScriptFromResponseAsync(string responseContent, CancellationToken cancellationToken)
    {
        var apiResponse = JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.GeminiResponse);
        var script = apiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim() ?? string.Empty;
        return Task.FromResult(script);
    }

    protected override HttpRequestMessage CreateHealthCheckRequest()
    {
        var url = $"/models?key={Configuration.ApiKey}";
        return new HttpRequestMessage(HttpMethod.Get, url);
    }

    public override string GetDefaultModel()
    {
        return Configuration.DefaultModel;
    }

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
