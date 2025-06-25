using Microsoft.Extensions.Logging;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Providers;

/// <summary>
/// Factory for creating AI providers
/// </summary>
public class ProviderFactory : IProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProviderConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public ProviderFactory(
        IHttpClientFactory httpClientFactory,
        ProviderConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    public IProvider CreateProvider(ProviderType providerType)
    {
        var httpClient = _httpClientFactory.CreateClient($"Provider_{providerType}");

        return providerType switch
        {
            ProviderType.OpenAi => new OpenAiProvider(
                httpClient,
                _configuration.OpenAi,
                _loggerFactory.CreateLogger<OpenAiProvider>()),

            ProviderType.Anthropic => new AnthropicProvider(
                httpClient,
                _configuration.Anthropic,
                _loggerFactory.CreateLogger<AnthropicProvider>()),

            ProviderType.Ollama => new OllamaProvider(
                httpClient,
                _configuration.Ollama,
                _loggerFactory.CreateLogger<OllamaProvider>()),

            ProviderType.OpenRouter => new OpenRouterProvider(
                httpClient,
                _configuration.OpenRouter,
                _loggerFactory.CreateLogger<OpenRouterProvider>()),

            ProviderType.Gemini => new GeminiProvider(
                httpClient,
                _configuration.Gemini,
                _loggerFactory.CreateLogger<GeminiProvider>()),

            _ => throw new ArgumentException($"Unsupported provider type: {providerType}")
        };
    }

    public string[] GetSupportedProviders()
    {
        return Enum.GetNames<ProviderType>();
    }
}
