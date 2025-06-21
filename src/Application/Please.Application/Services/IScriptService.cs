using Please.Domain.Common;
using Please.Domain.Entities;

namespace Please.Application.Services;

public interface IScriptService
{
    Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request,
        CancellationToken cancellationToken = default);
}
