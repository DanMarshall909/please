using Please.Domain.Common;
using Please.Domain.Entities;

namespace Please.Domain.Interfaces;

/// <summary>
/// Service for file operations
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Save a script to a file with appropriate extension based on script type
    /// </summary>
    /// <param name="script">The script response to save</param>
    /// <param name="directory">Optional directory to save to (defaults to user's Documents or current directory)</param>
    /// <param name="fileName">Optional custom file name (defaults to auto-generated based on task description)</param>
    /// <returns>Result containing the full path where the file was saved</returns>
    Task<Result<string>> SaveScriptToFileAsync(ScriptResponse script, string? directory = null, string? fileName = null);

    /// <summary>
    /// Get the appropriate file extension for a script type
    /// </summary>
    /// <param name="scriptType">The type of script</param>
    /// <returns>File extension including the dot (e.g., ".ps1", ".sh")</returns>
    string GetFileExtension(Enums.ScriptType scriptType);

    /// <summary>
    /// Generate a safe file name from task description
    /// </summary>
    /// <param name="taskDescription">The task description to convert to a file name</param>
    /// <returns>Safe file name without extension</returns>
    string GenerateFileName(string taskDescription);

    /// <summary>
    /// Get the default save directory for scripts
    /// </summary>
    /// <returns>Default directory path</returns>
    string GetDefaultSaveDirectory();
}