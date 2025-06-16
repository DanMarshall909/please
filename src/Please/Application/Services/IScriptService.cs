using Please.Domain.Common;
using Please.Domain.Entities;

namespace Please.Application.Services;

public interface IScriptService
{
    Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ScriptResponse>> GetScriptAsync(
        ScriptId id,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<ScriptResponse>>> GetRecentScriptsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);
}
