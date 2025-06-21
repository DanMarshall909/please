using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.TestUtilities;

public sealed class FakeScriptRepository : IScriptRepository
{
    private readonly List<ScriptResponse> _scripts = new();

    public IReadOnlyList<ScriptResponse> Scripts => _scripts;
    public Result NextSaveResult { get; set; } = Result.Success();

    public Task<Result> SaveScriptAsync(ScriptResponse response, CancellationToken cancellationToken = default)
    {
        if (NextSaveResult.IsSuccess)
            _scripts.Add(response);
        return Task.FromResult(NextSaveResult);
    }

    public Task<Result<ScriptResponse?>> GetLastScriptAsync(CancellationToken cancellationToken = default)
    {
        ScriptResponse? last = _scripts.Count > 0 ? _scripts[^1] : null;
        return Task.FromResult(Result<ScriptResponse?>.Success(last));
    }

    public Task<Result<IEnumerable<ScriptResponse>>> GetScriptHistoryAsync(int? count = null, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<ScriptResponse> result = _scripts;
        if (since.HasValue)
            result = result.Where(s => s.GeneratedAt >= since.Value);
        if (count.HasValue)
            result = result.Take(count.Value);
        return Task.FromResult(Result<IEnumerable<ScriptResponse>>.Success(result));
    }

    public Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        _scripts.Clear();
        return Task.FromResult(Result.Success());
    }

    public Task<Result<bool>> HasHistoryAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(_scripts.Count > 0));
    }
}
