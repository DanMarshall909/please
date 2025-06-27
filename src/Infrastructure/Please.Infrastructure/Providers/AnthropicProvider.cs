using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Entities;
using Please.Infrastructure.Serialization;

namespace Please.Infrastructure.Providers;

/// <summary>
/// Anthropic Claude API provider implementation
/// </summary>
public class AnthropicProvider : BaseHttpProvider<AnthropicConfiguration>
{
    public AnthropicProvider(HttpClient httpClient, AnthropicConfiguration configuration,
        ILogger<AnthropicProvider> logger)
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
            HttpClient.DefaultRequestHeaders.Add("x-api-key", Configuration.ApiKey);
            HttpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }
    }

    protected override bool IsConfigurationValid() => !string.IsNullOrEmpty(Configuration.ApiKey);

    protected override string GetConfigurationErrorMessage() => "Anthropic API key not configured";

    protected override string GetProviderName() => "Anthropic";

    protected override string GetModel(ScriptRequest request) => request.Model ?? Configuration.DefaultModel;

    protected override Task<HttpRequestMessage> CreateHttpRequestAsync(ScriptRequest request, string model,
        CancellationToken cancellationToken)
    {
        string systemPrompt = BuildSystemPrompt(request);
        string userPrompt = BuildUserPrompt(request);

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

        string json = JsonSerializer.Serialize(requestBody, ApiSerializationContext.Default.AnthropicRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return Task.FromResult(new HttpRequestMessage(HttpMethod.Post, "messages")
        {
            Content = content
        });
    }

    protected override Task<string> ExtractScriptFromResponseAsync(string responseContent,
        CancellationToken cancellationToken)
    {
        var apiResponse =
            JsonSerializer.Deserialize(responseContent, ApiSerializationContext.Default.AnthropicResponse);
        string script = apiResponse?.Content?.FirstOrDefault()?.Text?.Trim() ?? string.Empty;
        return Task.FromResult(script);
    }

    protected override HttpRequestMessage CreateHealthCheckRequest() => new(HttpMethod.Get, "messages");

    public override string GetDefaultModel() => Configuration.DefaultModel;

    public override string[] GetSupportedModels()
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
}
