using Please.Domain.Enums;

namespace Please.Domain.Interfaces;

/// <summary>
/// Factory interface for creating AI providers
/// </summary>
public interface IProviderFactory
{
    /// <summary>
    /// Creates a provider instance for the specified provider type
    /// </summary>
    IProvider CreateProvider(ProviderType providerType);

    /// <summary>
    /// Gets the list of supported provider names
    /// </summary>
    string[] GetSupportedProviders();
}
