using Please.Domain.Common;
using Please.Domain.Entities;

namespace Please.Domain.Interfaces;

/// <summary>
/// Base interface for AI providers
/// </summary>
public interface IProvider
{
    /// <summary>
    /// Generates a script using the AI provider
    /// </summary>
    Task<Result<string>> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the provider is available and configured
    /// </summary>
    Task<Result<bool>> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default model for this provider
    /// </summary>
    string GetDefaultModel();

    /// <summary>
    /// Gets the supported models for this provider
    /// </summary>
    string[] GetSupportedModels();
}
