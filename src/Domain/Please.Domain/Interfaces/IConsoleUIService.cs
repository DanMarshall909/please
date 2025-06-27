using Please.Domain.Entities;
using Please.Domain.Enums;

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

    /// <summary>
    /// Display script with enhanced syntax highlighting
    /// </summary>
    /// <param name="script">Script content</param>
    /// <param name="title">Script title</param>
    /// <param name="scriptType">Type of script for syntax highlighting</param>
    void DisplayScriptWithSyntaxHighlighting(string script, string title, ScriptType scriptType);

    /// <summary>
    /// Display complete script response with all metadata
    /// </summary>
    /// <param name="response">Script response to display</param>
    void DisplayScriptResponse(ScriptResponse response);

    /// <summary>
    /// Display safety notes in a formatted panel
    /// </summary>
    /// <param name="safetyNotes">List of safety notes</param>
    void DisplaySafetyNotes(IEnumerable<string> safetyNotes);

    /// <summary>
    /// Display enhanced progress with multiple steps
    /// </summary>
    /// <param name="title">Progress title</param>
    /// <param name="steps">Array of step descriptions</param>
    /// <param name="stepAction">Action to execute for each step</param>
    Task DisplayEnhancedProgressAsync(string title, string[] steps, Func<string, int, Task> stepAction);

    /// <summary>
    /// Display script preview with metadata
    /// </summary>
    /// <param name="response">Script response to preview</param>
    void DisplayScriptPreview(ScriptResponse response);

    /// <summary>
    /// Open script in external editor for modification
    /// </summary>
    /// <param name="script">Initial script content</param>
    /// <param name="scriptType">Type of script for file extension</param>
    /// <param name="taskDescription">Description for temp file naming</param>
    /// <returns>Modified script content, or null if cancelled</returns>
    Task<string?> EditScriptExternallyAsync(string script, ScriptType scriptType, string taskDescription);

    /// <summary>
    /// Confirm script execution after review/editing
    /// </summary>
    /// <param name="response">Script response to confirm</param>
    /// <returns>True if user wants to execute, false otherwise</returns>
    bool ConfirmScriptExecution(ScriptResponse response);
}
