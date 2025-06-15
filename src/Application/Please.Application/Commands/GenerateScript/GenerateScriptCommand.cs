using MediatR;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Application.Commands.GenerateScript;

/// <summary>
/// Command to generate a script based on user requirements
/// </summary>
public record GenerateScriptCommand : IRequest<ScriptResponse>
{
    public required string TaskDescription { get; init; }
    public ProviderType? Provider { get; init; }
    public string? Model { get; init; }
    public ScriptType? ScriptType { get; init; }
    public bool ForceExecution { get; init; }
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Creates a basic generate script command
    /// </summary>
    public static GenerateScriptCommand Create(string taskDescription) => new()
    {
        TaskDescription = taskDescription
    };

    /// <summary>
    /// Creates a generate script command with provider specification
    /// </summary>
    public static GenerateScriptCommand Create(string taskDescription, ProviderType provider, string? model = null) => new()
    {
        TaskDescription = taskDescription,
        Provider = provider,
        Model = model
    };
}
