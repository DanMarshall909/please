using Microsoft.Extensions.DependencyInjection;
using Please.Domain.Interfaces;
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
        // Register repositories
        services.AddSingleton<IScriptRepository, ScriptRepository>();

        // Register services
        services.AddSingleton<IScriptGenerator, ScriptGenerator>();
        services.AddSingleton<IContextService, ContextService>();

        // TODO: Add AI provider implementations when needed
        // services.AddSingleton<IOpenAiProvider, OpenAiProvider>();
        // services.AddSingleton<IAnthropicProvider, AnthropicProvider>();

        return services;
    }
}
