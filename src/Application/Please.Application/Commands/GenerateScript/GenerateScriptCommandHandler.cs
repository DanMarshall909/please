using MediatR;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.Application.Commands.GenerateScript;

/// <summary>
/// Handles script generation commands
/// </summary>
public class GenerateScriptCommandHandler : IRequestHandler<GenerateScriptCommand, ScriptResponse>
{
    private readonly IScriptGenerator _scriptGenerator;
    private readonly IScriptRepository _scriptRepository;

    public GenerateScriptCommandHandler(
        IScriptGenerator scriptGenerator,
        IScriptRepository scriptRepository)
    {
        _scriptGenerator = scriptGenerator;
        _scriptRepository = scriptRepository;
    }

    public async Task<ScriptResponse> Handle(GenerateScriptCommand request, CancellationToken cancellationToken)
    {
        // Convert command to domain request
        var scriptRequest = new ScriptRequest
        {
            TaskDescription = request.TaskDescription,
            Provider = request.Provider,
            Model = request.Model,
            ScriptType = request.ScriptType,
            ForceExecution = request.ForceExecution,
            WorkingDirectory = request.WorkingDirectory ?? Environment.CurrentDirectory
        };

        // Generate script using AI provider
        var response = await _scriptGenerator.GenerateScriptAsync(scriptRequest, cancellationToken);

        // Save to repository for history
        await _scriptRepository.SaveScriptAsync(response, cancellationToken);

        return response;
    }
}
