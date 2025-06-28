namespace Please.Domain.Interfaces;

/// <summary>
/// Provides cross-platform clipboard operations
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Sets text content to the clipboard
    /// </summary>
    /// <param name="text">The text to copy to clipboard</param>
    /// <returns>True if the operation succeeded, false otherwise</returns>
    Task<bool> SetTextAsync(string text);

    /// <summary>
    /// Gets text content from the clipboard
    /// </summary>
    /// <returns>The text from clipboard, or null if not available</returns>
    Task<string?> GetTextAsync();

    /// <summary>
    /// Checks if clipboard operations are supported on the current platform
    /// </summary>
    /// <returns>True if clipboard is supported, false otherwise</returns>
    bool IsSupported();
}