using Please.Application.Commands.GenerateScript;
using Please.Domain.Enums;

namespace Please.TestUtilities.Builders;

/// <summary>
/// Builder for creating GenerateScriptCommand objects in tests with sensible defaults
/// </summary>
public class GenerateScriptCommandBuilder
{
    private string _taskDescription = "Default task";
    private ProviderType? _provider;
    private string? _model;
    private ScriptType? _scriptType;
    private bool _forceExecution = false;
    private string? _workingDirectory;

    public static GenerateScriptCommandBuilder Create() => new();

    public GenerateScriptCommandBuilder WithTask(string taskDescription)
    {
        _taskDescription = taskDescription;
        return this;
    }

    public GenerateScriptCommandBuilder WithProvider(ProviderType provider)
    {
        _provider = provider;
        return this;
    }

    public GenerateScriptCommandBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public GenerateScriptCommandBuilder WithScriptType(ScriptType scriptType)
    {
        _scriptType = scriptType;
        return this;
    }

    public GenerateScriptCommandBuilder WithForceExecution(bool forceExecution = true)
    {
        _forceExecution = forceExecution;
        return this;
    }

    public GenerateScriptCommandBuilder WithWorkingDirectory(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        return this;
    }

    public GenerateScriptCommand Build()
    {
        GenerateScriptCommand command;

        if (_provider.HasValue && _model != null)
        {
            command = GenerateScriptCommand.Create(_taskDescription, _provider.Value, _model);
        }
        else
        {
            command = GenerateScriptCommand.Create(_taskDescription);
        }

        // Set additional properties using record with syntax
        return command with
        {
            ScriptType = _scriptType,
            ForceExecution = _forceExecution,
            WorkingDirectory = _workingDirectory
        };
    }
}
