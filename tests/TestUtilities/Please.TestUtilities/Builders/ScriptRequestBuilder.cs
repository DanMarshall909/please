using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.TestUtilities.Builders;

/// <summary>
/// Builder for creating ScriptRequest objects in tests with sensible defaults
/// </summary>
public class ScriptRequestBuilder
{
    private string _taskDescription = "Default task";
    private ProviderType? _provider;
    private string? _model;
    private ScriptType? _scriptType;
    private bool _forceExecution = false;
    private string? _workingDirectory;
    private readonly Dictionary<string, string> _additionalParameters = new();

    public static ScriptRequestBuilder Create() => new();

    public ScriptRequestBuilder WithTask(string taskDescription)
    {
        _taskDescription = taskDescription;
        return this;
    }

    public ScriptRequestBuilder WithProvider(ProviderType provider)
    {
        _provider = provider;
        return this;
    }

    public ScriptRequestBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public ScriptRequestBuilder WithScriptType(ScriptType scriptType)
    {
        _scriptType = scriptType;
        return this;
    }

    public ScriptRequestBuilder WithForceExecution(bool forceExecution = true)
    {
        _forceExecution = forceExecution;
        return this;
    }

    public ScriptRequestBuilder WithWorkingDirectory(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        return this;
    }

    public ScriptRequestBuilder WithParameter(string key, string value)
    {
        _additionalParameters[key] = value;
        return this;
    }

    public ScriptRequest Build()
    {
        ScriptRequest request;

        if (_provider.HasValue)
        {
            request = ScriptRequest.Create(_taskDescription, _provider.Value, _model);
        }
        else
        {
            request = ScriptRequest.Create(_taskDescription);
        }

        // Set additional properties
        if (_scriptType.HasValue)
        {
            request = request with { ScriptType = _scriptType.Value };
        }

        if (_forceExecution)
        {
            request = request with { ForceExecution = _forceExecution };
        }

        if (_workingDirectory != null)
        {
            request = request with { WorkingDirectory = _workingDirectory };
        }

        // Add additional parameters
        foreach (var kvp in _additionalParameters)
        {
            request.AdditionalParameters[kvp.Key] = kvp.Value;
        }

        return request;
    }
}
