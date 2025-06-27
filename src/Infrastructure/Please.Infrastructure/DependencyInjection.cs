using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using Please.Infrastructure.Providers;
using Please.Infrastructure.Repositories;
using Please.Infrastructure.Services;

namespace Please.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// </summary>
public static class DependencyInjection
{
    private static string GetConfigurationValue(string environmentKey, string? configurationKey, IConfiguration? configuration, string fallback)
    {
        var envVar = Environment.GetEnvironmentVariable(environmentKey);
        var configVar = configurationKey != null ? configuration?[configurationKey] : null;
        return envVar ?? configVar ?? fallback;
    }
    /// <summary>
    /// Registers Infrastructure layer services with the DI container
    /// </summary>
    /// <param name="services">The service collection to register services with</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register HTTP clients for AI providers
        services.AddHttpClient();

        // Register provider configuration
        services.AddSingleton<ProviderConfiguration>(provider =>
        {
            var configuration = provider.GetService<IConfiguration>();
            return new ProviderConfiguration
            {
                OpenAi = new OpenAiConfiguration
                {
                    ApiKey = GetConfigurationValue("OPENAI_API_KEY", "OPENAI_API_KEY", configuration, ""),
                    BaseUrl = GetConfigurationValue("OPENAI_BASE_URL", "OPENAI_BASE_URL", configuration, "https://api.openai.com/v1"),
                    DefaultModel = GetConfigurationValue("OPENAI_DEFAULT_MODEL", "OPENAI_DEFAULT_MODEL", configuration, "gpt-3.5-turbo")
                },
                Anthropic = new AnthropicConfiguration
                {
                    ApiKey = GetConfigurationValue("ANTHROPIC_API_KEY", "ANTHROPIC_API_KEY", configuration, ""),
                    BaseUrl = GetConfigurationValue("ANTHROPIC_BASE_URL", "ANTHROPIC_BASE_URL", configuration, "https://api.anthropic.com/v1"),
                    DefaultModel = GetConfigurationValue("ANTHROPIC_DEFAULT_MODEL", "ANTHROPIC_DEFAULT_MODEL", configuration, "claude-3-haiku-20240307")
                },
                Ollama = new OllamaConfiguration
                {
                    BaseUrl = GetConfigurationValue("OLLAMA_BASE_URL", "DefaultSettings:OLLAMA_BASE_URL", configuration, "http://localhost:11434"),
                    DefaultModel = GetConfigurationValue("OLLAMA_DEFAULT_MODEL", "DefaultSettings:OLLAMA_DEFAULT_MODEL", configuration, "llama3:latest")
                },
                OpenRouter = new OpenRouterConfiguration
                {
                    ApiKey = GetConfigurationValue("OPENROUTER_API_KEY", "OPENROUTER_API_KEY", configuration, ""),
                    BaseUrl = GetConfigurationValue("OPENROUTER_BASE_URL", "OPENROUTER_BASE_URL", configuration, "https://openrouter.ai/api/v1"),
                    DefaultModel = GetConfigurationValue("OPENROUTER_DEFAULT_MODEL", "OPENROUTER_DEFAULT_MODEL", configuration, "microsoft/wizardlm-2-8x22b")
                },
                Gemini = new GeminiConfiguration
                {
                    ApiKey = GetConfigurationValue("GEMINI_API_KEY", "GEMINI_API_KEY", configuration, ""),
                    BaseUrl = GetConfigurationValue("GEMINI_BASE_URL", "GEMINI_BASE_URL", configuration, "https://generativelanguage.googleapis.com/v1beta"),
                    DefaultModel = GetConfigurationValue("GEMINI_DEFAULT_MODEL", "GEMINI_DEFAULT_MODEL", configuration, "gemini-pro")
                }
            };
        });

        // Register individual configuration classes that providers expect
        services.AddSingleton<OpenAiConfiguration>(provider =>
            provider.GetRequiredService<ProviderConfiguration>().OpenAi);
        services.AddSingleton<AnthropicConfiguration>(provider =>
            provider.GetRequiredService<ProviderConfiguration>().Anthropic);
        services.AddSingleton<OllamaConfiguration>(provider =>
            provider.GetRequiredService<ProviderConfiguration>().Ollama);
        services.AddSingleton<OpenRouterConfiguration>(provider =>
            provider.GetRequiredService<ProviderConfiguration>().OpenRouter);
        services.AddSingleton<GeminiConfiguration>(provider =>
            provider.GetRequiredService<ProviderConfiguration>().Gemini);

        // Register AI provider factory
        services.AddSingleton<IProviderFactory, ProviderFactory>();

        // Register individual AI providers
        services.AddTransient<OpenAiProvider>();
        services.AddTransient<AnthropicProvider>();
        services.AddTransient<OllamaProvider>();
        services.AddTransient<OpenRouterProvider>();
        services.AddTransient<GeminiProvider>();

        // Register repositories
        services.AddSingleton<IScriptRepository, ScriptRepository>();

        // Register services
        services.AddSingleton<IScriptGenerator, ScriptGenerator>();
        services.AddSingleton<IScriptValidationService, ScriptValidationService>();
        services.AddSingleton<IContextService, ContextService>();
        services.AddSingleton<IScriptExecutor, PowerShellScriptExecutor>();
        services.AddSingleton<IUserConfirmation, ConsoleUserConfirmation>();
        services.AddSingleton<IPlatformService, PlatformService>();

        return services;
    }
}
