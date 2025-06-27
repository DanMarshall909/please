using Please.Domain.Common;

namespace Please.Domain.Interfaces;

/// <summary>
/// Provides functionality to execute generated scripts safely.
/// </summary>
public interface IScriptExecutor
{
    /// <summary>
    /// Executes the provided script and returns the output or error result.
    /// </summary>
    /// <param name="script">The script content to execute</param>
    /// <returns>A result containing the execution output or error message</returns>
    Task<Result<string>> ExecuteScriptAsync(string script);
}
