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

    public CommandProcessor(IContextService contextService, IScriptGenerator scriptGenerator, ILogger<CommandProcessor> logger)
    {
        _contextService = contextService;
        _scriptGenerator = scriptGenerator;
        _logger = logger;
    }

    public async Task<Result<ScriptResponse>> ProcessAsync(string command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing command '{Command}'", command);
        var intent = new CommandIntent(command);
        var contextResult = await _contextService.GetContextAsync(intent, cancellationToken);
        if (contextResult.IsFailure)
        {
            _logger.LogWarning("Context retrieval failed: {Error}", contextResult.Error);
            return Result<ScriptResponse>.Failure(contextResult.Error);
        }

        var request = ScriptRequest.Create(command);
        var result = await _scriptGenerator.GenerateScriptAsync(request, cancellationToken);
        if (result.IsSuccess)
            _logger.LogInformation("Command processed successfully");
        else
            _logger.LogWarning("Command processing failed: {Error}", result.Error);
        return result;
    }
}

