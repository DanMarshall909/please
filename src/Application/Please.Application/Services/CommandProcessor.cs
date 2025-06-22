using Please.Domain.Commands;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Please.Application.Services;

/// <summary>
/// Processes user commands using context service and script generator
/// </summary>
public sealed class CommandProcessor
{
    private readonly IContextService _contextService;
    private readonly IScriptGenerator _scriptGenerator;
    private readonly ILogger<CommandProcessor> _logger;
    private readonly ILocalizationService _localization;

    public CommandProcessor(IContextService contextService, IScriptGenerator scriptGenerator, ILogger<CommandProcessor> logger, ILocalizationService localization)
    {
        _contextService = contextService;
        _scriptGenerator = scriptGenerator;
        _logger = logger;
        _localization = localization;
    }

    public async Task<Result<ScriptResponse>> ProcessAsync(string command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(_localization.GetString("ProcessingCommand"), command);
        var intent = new CommandIntent(command);
        var contextResult = await _contextService.GetContextAsync(intent, cancellationToken);
        if (contextResult.IsFailure)
        {
            _logger.LogWarning(_localization.GetString("ContextFailed"), contextResult.Error);
            return Result<ScriptResponse>.Failure(contextResult.Error);
        }

        var request = ScriptRequest.Create(command);
        var result = await _scriptGenerator.GenerateScriptAsync(request, cancellationToken);
        if (result.IsSuccess)
            _logger.LogInformation(_localization.GetString("CommandProcessed"));
        else
            _logger.LogWarning(_localization.GetString("ProcessingFailed"), result.Error);
        return result;
    }
}

