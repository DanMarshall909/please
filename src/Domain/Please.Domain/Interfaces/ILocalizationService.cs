namespace Please.Domain.Interfaces;

/// <summary>
/// Provides localized strings for user-facing messages.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the localized string for the given key.
    /// </summary>
    string GetString(string key);
}
