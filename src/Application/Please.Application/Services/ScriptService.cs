using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Please.Application.Services;

public sealed class ScriptService : IScriptService
{
    private readonly IScriptGenerator _generator;
    private readonly IScriptRepository _repository;
    private readonly ILogger<ScriptService> _logger;
    public ScriptService(IScriptGenerator generator, IScriptRepository repository, ILogger<ScriptService> logger)
    {
        _generator = generator;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating script");
        var generationResult = await _generator.GenerateScriptAsync(request, cancellationToken);
        if (generationResult.IsFailure)
        {
            _logger.LogWarning("Generation failed: {Error}", generationResult.Error);
            return Result<ScriptResponse>.Failure(generationResult.Error);
        }
        _logger.LogInformation("Saving script");
        var saveResult = await _repository.SaveScriptAsync(generationResult.Value!, cancellationToken);
        if (saveResult.IsFailure)
        {
            _logger.LogError("Failed to save script: {Error}", saveResult.Error);
            return Result<ScriptResponse>.Failure($"Failed to save script: {saveResult.Error}");
        }
        _logger.LogInformation("Script generated successfully");
        return Result<ScriptResponse>.Success(generationResult.Value!);
    }
}
