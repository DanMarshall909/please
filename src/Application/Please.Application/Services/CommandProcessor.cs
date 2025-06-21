using Please.Domain.Commands;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.Application.Services;

/// <summary>
/// Processes user commands using context service and script generator
/// </summary>
public sealed class CommandProcessor
{
    private readonly IContextService _contextService;
    private readonly IScriptGenerator _scriptGenerator;

    public CommandProcessor(IContextService contextService, IScriptGenerator scriptGenerator)
    {
        _contextService = contextService;
        _scriptGenerator = scriptGenerator;
    }

    public async Task<Result<ScriptResponse>> ProcessAsync(string command, CancellationToken cancellationToken = default)
    {
        var intent = new CommandIntent(command);
        var contextResult = await _contextService.GetContextAsync(intent, cancellationToken);
        if (contextResult.IsFailure)
            return Result<ScriptResponse>.Failure(contextResult.Error);

        var request = ScriptRequest.Create(command);
        return await _scriptGenerator.GenerateScriptAsync(request, cancellationToken);
    }
}

