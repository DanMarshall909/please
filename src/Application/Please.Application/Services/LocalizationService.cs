using System.Collections.Generic;
using Please.Domain.Interfaces;

namespace Please.Application.Services;

/// <summary>
/// Basic in-memory implementation of <see cref="ILocalizationService"/>.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, string> _strings;

    public LocalizationService()
    {
        _strings = new Dictionary<string, string>
        {
            ["DependencyInjectionConfigured"] = "Dependency injection configured.",
            ["ProcessingCommand"] = "Processing command '{Command}'",
            ["CommandProcessed"] = "Command processed successfully",
            ["ContextFailed"] = "Context retrieval failed: {Error}",
            ["ProcessingFailed"] = "Command processing failed: {Error}",
            ["GeneratingScript"] = "Generating script",
            ["GenerationFailed"] = "Generation failed: {Error}",
            ["SavingScript"] = "Saving script",
            ["SaveFailed"] = "Failed to save script: {Error}",
            ["Generated"] = "Script generated successfully"
        };
    }

    public string GetString(string key) =>
        _strings.TryGetValue(key, out var value) ? value : key;
}
