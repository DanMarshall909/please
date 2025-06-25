namespace Please.Domain.Interfaces;

/// <summary>
/// Provides functionality to ask for user confirmation before executing operations.
/// </summary>
public interface IUserConfirmation
{
    /// <summary>
    /// Asks the user for confirmation with a message and script content.
    /// </summary>
    /// <param name="message">The confirmation message to display</param>
    /// <param name="scriptContent">The script content to show for review</param>
    /// <returns>True if the user confirms, false otherwise</returns>
    bool AskForConfirmation(string message, string scriptContent);
}
