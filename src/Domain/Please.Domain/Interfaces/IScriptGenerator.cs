using Please.Domain.Entities;

namespace Please.Domain.Interfaces;

/// <summary>
/// Contract for AI-powered script generation
/// </summary>
public interface IScriptGenerator
{
    /// <summary>
    /// Generates a script based on the provided request
    /// </summary>
    Task<ScriptResponse> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the provider and model combination is supported
    /// </summary>
    Task<bool> IsProviderAvailableAsync(ScriptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the fallback model for a specific provider if the requested model is unavailable
    /// </summary>
    string GetFallbackModel(ScriptRequest request);
}
