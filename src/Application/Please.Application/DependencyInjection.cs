using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Please.Application;

/// <summary>
/// Dependency injection configuration for the Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddLogging();

        // Register MediatR with all handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register core application services
        services.AddTransient<IScriptService, ScriptService>();
        services.AddTransient<CommandProcessor>();

        return services;
    }
}
