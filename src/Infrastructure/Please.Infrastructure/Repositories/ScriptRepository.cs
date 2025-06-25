using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of script repository for development and testing
/// </summary>
public class ScriptRepository : IScriptRepository
{
    private readonly List<ScriptResponse> _scripts = [];
    private readonly object _lock = new();

    public Task<VoidResult> SaveScriptAsync(ScriptResponse response, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(response);

            lock (_lock)
            {
                _scripts.Add(response);
            }

            return Task.FromResult(VoidResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(VoidResult.Failure($"Failed to save script: {ex.Message}"));
        }
    }

    public Task<Result<ScriptResponse?>> GetLastScriptAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                var lastScript = _scripts
                    .OrderByDescending(s => s.GeneratedAt)
                    .FirstOrDefault();

                return Task.FromResult(Result<ScriptResponse?>.Success(lastScript));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ScriptResponse?>.Failure($"Failed to retrieve last script: {ex.Message}"));
        }
    }

    public Task<Result<IEnumerable<ScriptResponse>>> GetScriptHistoryAsync(
        int? count = null,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                var query = _scripts.AsEnumerable();

                // Filter by date if specified
                if (since.HasValue)
                {
                    query = query.Where(s => s.GeneratedAt >= since.Value);
                }

                // Order by most recent first
                query = query.OrderByDescending(s => s.GeneratedAt);

                // Apply count limit if specified
                if (count.HasValue && count.Value > 0)
                {
                    query = query.Take(count.Value);
                }

                var results = query.ToList();
                return Task.FromResult(Result<IEnumerable<ScriptResponse>>.Success(results));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                Result<IEnumerable<ScriptResponse>>.Failure($"Failed to retrieve script history: {ex.Message}"));
        }
    }

    public Task<VoidResult> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                _scripts.Clear();
            }

            return Task.FromResult(VoidResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(VoidResult.Failure($"Failed to clear history: {ex.Message}"));
        }
    }

    public Task<Result<bool>> HasHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                var hasHistory = _scripts.Count > 0;
                return Task.FromResult(Result<bool>.Success(hasHistory));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool>.Failure($"Failed to check history: {ex.Message}"));
        }
    }
}
