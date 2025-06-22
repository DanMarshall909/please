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
    private readonly ILocalizationService _localization;
    public ScriptService(IScriptGenerator generator, IScriptRepository repository, ILogger<ScriptService> logger, ILocalizationService localization)
    {
        _generator = generator;
        _repository = repository;
        _logger = logger;
        _localization = localization;
    }

    public async Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(_localization.GetString("GeneratingScript"));
        var generationResult = await _generator.GenerateScriptAsync(request, cancellationToken);
        if (generationResult.IsFailure)
        {
            _logger.LogWarning(_localization.GetString("GenerationFailed"), generationResult.Error);
            return Result<ScriptResponse>.Failure(generationResult.Error);
        }
        _logger.LogInformation(_localization.GetString("SavingScript"));
        var saveResult = await _repository.SaveScriptAsync(generationResult.Value!, cancellationToken);
        if (saveResult.IsFailure)
        {
            _logger.LogError(_localization.GetString("SaveFailed"), saveResult.Error);
            return Result<ScriptResponse>.Failure($"Failed to save script: {saveResult.Error}");
        }
        _logger.LogInformation(_localization.GetString("Generated"));
        return Result<ScriptResponse>.Success(generationResult.Value!);
    }
}
