using Please.Domain.Entities;

namespace Please.Domain.Interfaces;

/// <summary>
/// Contract for script persistence and retrieval
/// </summary>
public interface IScriptRepository
{
    /// <summary>
    /// Saves a script response to history
    /// </summary>
    Task SaveScriptAsync(ScriptResponse response, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the last generated script
    /// </summary>
    Task<ScriptResponse?> GetLastScriptAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves script history with optional filtering
    /// </summary>
    Task<IEnumerable<ScriptResponse>> GetScriptHistoryAsync(
        int? count = null, 
        DateTime? since = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears script history
    /// </summary>
    Task ClearHistoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any scripts exist in history
    /// </summary>
    Task<bool> HasHistoryAsync(CancellationToken cancellationToken = default);
}
