namespace Please.Domain.Interfaces;

/// <summary>
/// Service for secure user input handling including password masking and validation.
/// </summary>
public interface ISecureInputService
{
    /// <summary>
    /// Prompts the user for sensitive input (like API keys) with masked display.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user</param>
    /// <returns>The user's input securely captured</returns>
    Task<string> PromptForSecureInputAsync(string prompt);

    /// <summary>
    /// Prompts the user for confirmation (Y/N) with a specific message.
    /// </summary>
    /// <param name="message">The confirmation message</param>
    /// <returns>True if user confirms, false otherwise</returns>
    Task<bool> PromptForConfirmationAsync(string message);

    /// <summary>
    /// Validates that the input contains only safe characters and meets basic security requirements.
    /// </summary>
    /// <param name="input">The input to validate</param>
    /// <returns>True if input is valid, false otherwise</returns>
    bool ValidateSecureInput(string input);
}
