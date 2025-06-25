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
        if (response == null) return Task.FromResult(VoidResult.Failure("Script response cannot be null"));

        lock (_lock)
        {
            _scripts.Add(response);
        }

        return Task.FromResult(VoidResult.Success);
    }

    public Task<Result<ScriptResponse?>> GetLastScriptAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var lastScript = _scripts
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            return Task.FromResult(Result<ScriptResponse?>.Success(lastScript));
        }
    }

    public Task<Result<IEnumerable<ScriptResponse>>> GetScriptHistoryAsync(
        int? count = null,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _scripts.AsEnumerable();

            // Filter by date if specified
            if (since.HasValue) query = query.Where(s => s.CreatedAt >= since.Value);

            // Order by most recent first
            query = query.OrderByDescending(s => s.CreatedAt);

            // Apply count limit if specified
            if (count.HasValue && count.Value > 0) query = query.Take(count.Value);

            var results = query.ToList();
            return Task.FromResult(Result<IEnumerable<ScriptResponse>>.Success(results));
        }
    }

    public Task<VoidResult> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _scripts.Clear();
        }

        return Task.FromResult(VoidResult.Success);
    }

    public Task<Result<bool>> HasHistoryAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            bool hasHistory = _scripts.Count > 0;
            return Task.FromResult(Result<bool>.Success(hasHistory));
        }
    }
}
