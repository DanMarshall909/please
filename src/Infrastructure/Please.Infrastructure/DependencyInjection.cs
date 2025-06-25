using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Please.Domain.Interfaces;
using Please.Infrastructure.Providers;
using Please.Infrastructure.Repositories;
using Please.Infrastructure.Services;

namespace Please.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// </summary>
public static class DependencyInjection
{
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
                    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                             configuration?["OPENAI_API_KEY"] ?? "",
                    BaseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL") ??
                              configuration?["OPENAI_BASE_URL"] ?? "https://api.openai.com/v1",
                    DefaultModel = Environment.GetEnvironmentVariable("OPENAI_DEFAULT_MODEL") ??
                                   configuration?["OPENAI_DEFAULT_MODEL"] ?? "gpt-3.5-turbo"
                },
                Anthropic = new AnthropicConfiguration
                {
                    ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ??
                             configuration?["ANTHROPIC_API_KEY"] ?? "",
                    BaseUrl = Environment.GetEnvironmentVariable("ANTHROPIC_BASE_URL") ??
                              configuration?["ANTHROPIC_BASE_URL"] ??
                              "https://api.anthropic.com/v1",
                    DefaultModel = Environment.GetEnvironmentVariable("ANTHROPIC_DEFAULT_MODEL") ??
                                   configuration?["ANTHROPIC_DEFAULT_MODEL"] ??
                                   "claude-3-haiku-20240307"
                },
                Ollama = new OllamaConfiguration
                {
                    BaseUrl = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ??
                              configuration?["OLLAMA_BASE_URL"] ?? "http://localhost:11434",
                    DefaultModel = Environment.GetEnvironmentVariable("OLLAMA_DEFAULT_MODEL") ??
                                   configuration?["OLLAMA_DEFAULT_MODEL"] ?? "llama2"
                },
                OpenRouter = new OpenRouterConfiguration
                {
                    ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
                             configuration?["OPENROUTER_API_KEY"] ?? "",
                    BaseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ??
                              configuration?["OPENROUTER_BASE_URL"] ??
                              "https://openrouter.ai/api/v1",
                    DefaultModel = Environment.GetEnvironmentVariable("OPENROUTER_DEFAULT_MODEL") ??
                                   configuration?["OPENROUTER_DEFAULT_MODEL"] ??
                                   "microsoft/wizardlm-2-8x22b"
                },
                Gemini = new GeminiConfiguration
                {
                    ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ??
                             configuration?["GEMINI_API_KEY"] ?? "",
                    BaseUrl = Environment.GetEnvironmentVariable("GEMINI_BASE_URL") ??
                              configuration?["GEMINI_BASE_URL"] ??
                              "https://generativelanguage.googleapis.com/v1beta",
                    DefaultModel = Environment.GetEnvironmentVariable("GEMINI_DEFAULT_MODEL") ??
                                   configuration?["GEMINI_DEFAULT_MODEL"] ?? "gemini-pro"
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
        services.AddSingleton<IContextService, ContextService>();
        services.AddSingleton<IScriptExecutor, PowerShellScriptExecutor>();
        services.AddSingleton<IUserConfirmation, ConsoleUserConfirmation>();

        return services;
    }
}
