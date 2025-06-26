namespace Please.Domain.Interfaces;

/// <summary>
/// Service for professional console UI rendering using Spectre.Console
/// </summary>
public interface IConsoleUIService
{
    /// <summary>
    /// Display a PowerShell script with beautiful formatting and syntax highlighting
    /// </summary>
    /// <param name="script">The script content to display</param>
    /// <param name="title">Title for the script panel</param>
    void DisplayScript(string script, string title);

    /// <summary>
    /// Show a professional progress indicator during AI generation
    /// </summary>
    /// <param name="message">Progress message to display</param>
    /// <param name="action">Action to execute while showing progress</param>
    /// <returns>Task representing the async operation</returns>
    Task DisplayProgressAsync(string message, Func<Task> action);

    /// <summary>
    /// Show a professional progress indicator during AI generation with return value
    /// </summary>
    /// <param name="message">Progress message to display</param>
    /// <param name="action">Action to execute while showing progress</param>
    /// <returns>Task with result from the action</returns>
    Task<T> DisplayProgressAsync<T>(string message, Func<Task<T>> action);

    /// <summary>
    /// Display interactive menu with single-key navigation
    /// </summary>
    /// <param name="options">Menu options to display</param>
    /// <returns>Selected option index</returns>
    int DisplayInteractiveMenu(string[] options);

    /// <summary>
    /// Display risk warnings with colored safety indicators
    /// </summary>
    /// <param name="riskLevel">Risk level (LOW, MEDIUM, HIGH)</param>
    /// <param name="warnings">List of warning messages</param>
    void DisplayRiskWarning(string riskLevel, string[] warnings);

    /// <summary>
    /// Display professional application banner
    /// </summary>
    /// <param name="version">Application version</param>
    /// <param name="description">Application description</param>
    void DisplayBanner(string version, string description);
}
