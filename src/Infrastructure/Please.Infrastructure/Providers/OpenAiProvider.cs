using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Please.Infrastructure.Serialization;

namespace Please.Infrastructure.Providers;

/// <summary>
/// OpenAI API provider implementation
/// </summary>
public class OpenAiProvider : BaseHttpProvider<OpenAiConfiguration>
{
    public OpenAiProvider(HttpClient httpClient, OpenAiConfiguration configuration, ILogger<OpenAiProvider> logger)
        : base(httpClient, configuration, logger)
    {
    }

    protected override void ConfigureHttpClient()
    {
        HttpClient.BaseAddress = new Uri(Configuration.BaseUrl);
        HttpClient.Timeout = TimeSpan.FromSeconds(Configuration.TimeoutSeconds);

        if (!string.IsNullOrEmpty(Configuration.ApiKey))
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Configuration.ApiKey);
        }
    }

    protected override bool IsConfigurationValid()
    {
        return !string.IsNullOrEmpty(Configuration.ApiKey) && Configuration.ApiKey != "your-api-key-here";
    }

    protected override string GetConfigurationErrorMessage()
    {
        return "OpenAI API key not configured";
    }

    protected override string GetProviderName()
    {
        return "OpenAI";
    }

    protected override string GetModel(ScriptRequest request)
    {
        return request.Model ?? Configuration.DefaultModel;
    }

    protected override Task<HttpRequestMessage> CreateHttpRequestAsync(ScriptRequest request, string model, CancellationToken cancellationToken)
    {
        var systemPrompt = BuildSystemPrompt(request);
        var userPrompt = BuildUserPrompt(request);

        var requestBody = new OpenAiRequest
        {
            Model = model,
            Messages = new[]
            {
                new OpenAiMessage { Role = "system", Content = systemPrompt },
                new OpenAiMessage { Role = "user", Content = userPrompt }
            },
            Temperature = 0.1,
            MaxTokens = 1000
        };

        var json = JsonSerializer.Serialize(requestBody, ApiSerializationContext.Default.OpenAiRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return Task.FromResult(new HttpRequestMessage(HttpMethod.Post, "/chat/completions")
        {
            Content = content
        });
    }

    protected override Task<string> ExtractScriptFromResponseAsync(string responseContent, CancellationToken cancellationToken)
    {
        var apiResponse = JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.OpenAiResponse);
        return Task.FromResult(apiResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty);
    }

    protected override HttpRequestMessage CreateHealthCheckRequest()
    {
        return new HttpRequestMessage(HttpMethod.Get, "/models");
    }

    public override string GetDefaultModel()
    {
        return Configuration.DefaultModel;
    }

    public override string[] GetSupportedModels()
    {
        return new[]
        {
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4-turbo",
            "gpt-4",
            "gpt-3.5-turbo"
        };
    }
}
