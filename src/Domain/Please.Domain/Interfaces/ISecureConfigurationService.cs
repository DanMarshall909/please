using Please.Domain.Enums;

namespace Please.Domain.Interfaces;

/// <summary>
/// Service for securely managing API keys and configuration with encryption and validation.
/// Provides a priority chain: Environment Variables → Encrypted Storage → User Secrets → Interactive Prompt
/// </summary>
public interface ISecureConfigurationService
{
    /// <summary>
    /// Retrieves an API key for the specified provider from the priority chain.
    /// </summary>
    /// <param name="provider">The AI provider type</param>
    /// <returns>The API key if found, null otherwise</returns>
    Task<string?> GetApiKeyAsync(ProviderType provider);

    /// <summary>
    /// Stores an API key for the specified provider using encrypted local storage.
    /// </summary>
    /// <param name="provider">The AI provider type</param>
    /// <param name="apiKey">The API key to store securely</param>
    /// <returns>Task representing the async operation</returns>
    Task StoreApiKeyAsync(ProviderType provider, string apiKey);

    /// <summary>
    /// Validates an API key by making a minimal test call to the provider's API.
    /// </summary>
    /// <param name="provider">The AI provider type</param>
    /// <returns>True if the key is valid, false otherwise</returns>
    Task<bool> ValidateApiKeyAsync(ProviderType provider);

    /// <summary>
    /// Checks if a valid API key exists for the specified provider.
    /// </summary>
    /// <param name="provider">The AI provider type</param>
    /// <returns>True if a valid key exists, false otherwise</returns>
    Task<bool> HasValidApiKeyAsync(ProviderType provider);

    /// <summary>
    /// Clears sensitive data from memory to enhance security.
    /// Should be called after API operations complete.
    /// </summary>
    void ClearSensitiveData();
}
