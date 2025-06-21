using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.Application.Services;

public sealed class ScriptService : IScriptService
{
    private readonly IScriptGenerator _generator;
    private readonly IScriptRepository _repository;

    public ScriptService(IScriptGenerator generator, IScriptRepository repository)
    {
        _generator = generator;
        _repository = repository;
    }

    public async Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        var generationResult = await _generator.GenerateScriptAsync(request, cancellationToken);
        if (generationResult.IsFailure)
            return Result<ScriptResponse>.Failure(generationResult.Error);

        var saveResult = await _repository.SaveScriptAsync(generationResult.Value!, cancellationToken);
        if (saveResult.IsFailure)
            return Result<ScriptResponse>.Failure($"Failed to save script: {saveResult.Error}");

        return Result<ScriptResponse>.Success(generationResult.Value!);
    }
}
