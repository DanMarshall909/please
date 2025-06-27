using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Interfaces;

namespace Please.Application;

/// <summary>
/// Dependency injection configuration for the Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddLogging();

        // Register core application services with explicit interface registrations for AOT compatibility
        services.TryAddTransient<IScriptService, ScriptService>();
        services.TryAddTransient<CommandProcessor>();
        services.TryAddTransient<InstallationService>();

        // These interfaces need implementations in the Infrastructure layer or test doubles
        // We don't register them here, but we ensure the AOT compiler knows about them
        registerRequiredInterfacesForAot(services);

        return services;
    }

    // This method is never called at runtime but ensures the AOT compiler preserves these types
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "This method is only for AOT analysis and is never actually called at runtime")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
        Justification = "This method is only for AOT analysis and is never actually called at runtime")]
    private static void registerRequiredInterfacesForAot(IServiceCollection services)
    {
        // These types are registered to help the AOT compiler understand the required types
        // The #pragma directive suppresses the unreachable code warning
#pragma warning disable CS0162 // Unreachable code detected
        if (DateTime.Now.Ticks < 0) // This condition is always false but compiler can't determine that statically
        {
            // These are the interfaces that need implementations
            services.AddTransient<IScriptGenerator, UnusedScriptGenerator>();
            services.AddTransient<IScriptRepository, UnusedScriptRepository>();
            services.AddTransient<IContextService, UnusedContextService>();
        }
#pragma warning restore CS0162
    }

    // Placeholder classes that are never instantiated but help the AOT compiler
    private class UnusedScriptGenerator : IScriptGenerator
    {
        public string GetFallbackModel(Domain.Entities.ScriptRequest request) => string.Empty;

        public Task<Result<bool>> IsProviderAvailableAsync(Domain.Entities.ScriptRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<bool>.Success(false));

        public Task<Result<Domain.Entities.ScriptResponse>> GenerateScriptAsync(
            Domain.Entities.ScriptRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Domain.Entities.ScriptResponse>.Failure("Not implemented"));

        public Task<Result<Domain.Entities.ScriptResponse>> GenerateFixedScriptAsync(
            string originalScript, string errorMessage, Domain.Entities.ScriptRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Domain.Entities.ScriptResponse>.Failure("Not implemented"));
    }

    private class UnusedScriptRepository : IScriptRepository
    {
        public Task<VoidResult> ClearHistoryAsync(CancellationToken cancellationToken = default)
            => VoidResult.SuccessfulTask;

        public Task<Result<Domain.Entities.ScriptResponse?>> GetLastScriptAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Domain.Entities.ScriptResponse?>.Success(null));

        public Task<Result<IEnumerable<Domain.Entities.ScriptResponse>>> GetScriptHistoryAsync(
            int? count = null, DateTime? since = null, CancellationToken cancellationToken = default)
            => Task.FromResult(
                Result<IEnumerable<Domain.Entities.ScriptResponse>>.Success(
                    Array.Empty<Domain.Entities.ScriptResponse>()));

        public Task<Result<bool>> HasHistoryAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result<bool>.Success(false));

        public Task<VoidResult> SaveScriptAsync(Domain.Entities.ScriptResponse response,
            CancellationToken cancellationToken = default)
            => VoidResult.SuccessfulTask;
    }

    private class UnusedContextService : IContextService
    {
        public Task<Result<Domain.Commands.CommandContext>> GetContextAsync(
            Domain.Commands.CommandIntent intent, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Domain.Commands.CommandContext>.Failure("Not implemented"));

        public Task<VoidResult> StorePatternAsync(Domain.Commands.CommandExecution execution,
            CancellationToken cancellationToken = default)
            => VoidResult.SuccessfulTask;
    }
}
