using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.TestUtilities;

public sealed class FakeScriptGenerator : IScriptGenerator
{
    public ScriptRequest? LastRequest { get; private set; }

    public Result<ScriptResponse> NextResult { get; set; } =
        Result<ScriptResponse>.Failure("nope");

    public Result<bool> ProviderAvailable { get; set; } =
        Result<bool>.Success(true);

    public Task<Result<ScriptResponse>> GenerateScriptAsync(ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult(NextResult);
    }

    public Task<Result<bool>> IsProviderAvailableAsync(ScriptRequest request,
        CancellationToken cancellationToken = default) => Task.FromResult(ProviderAvailable);

    public string GetFallbackModel(ScriptRequest request) => "model";

    public Task<Result<ScriptResponse>> GenerateFixedScriptAsync(string originalScript, string errorMessage, ScriptRequest request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult(NextResult);
    }
}
