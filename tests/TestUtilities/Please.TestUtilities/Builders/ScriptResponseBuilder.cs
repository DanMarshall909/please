using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.TestUtilities.Builders;

/// <summary>
/// Builder for creating ScriptResponse objects in tests with sensible defaults
/// </summary>
public class ScriptResponseBuilder
{
    private string _script = "echo 'default script'";
    private string _taskDescription = "Default task";
    private ProviderType _provider = ProviderType.OpenAi;
    private string _model = "gpt-4";
    private ScriptType _scriptType = ScriptType.Bash;
    private RiskLevel _riskLevel = RiskLevel.Low;
    private DateTime? _createdAt;
    private readonly List<ScriptResponse.Warning> _warnings = new();
    private readonly List<ScriptResponse.SafetyNote> _safetyNotes = new();

    public static ScriptResponseBuilder Create() => new();

    public ScriptResponseBuilder WithScript(string script)
    {
        _script = script;
        return this;
    }

    public ScriptResponseBuilder WithTask(string taskDescription)
    {
        _taskDescription = taskDescription;
        return this;
    }

    public ScriptResponseBuilder WithProvider(ProviderType provider)
    {
        _provider = provider;
        return this;
    }

    public ScriptResponseBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public ScriptResponseBuilder WithScriptType(ScriptType scriptType)
    {
        _scriptType = scriptType;
        return this;
    }

    public ScriptResponseBuilder WithRiskLevel(RiskLevel riskLevel)
    {
        _riskLevel = riskLevel;
        return this;
    }

    public ScriptResponseBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ScriptResponseBuilder WithWarning(string message)
    {
        _warnings.Add(new ScriptResponse.Warning(message));
        return this;
    }

    public ScriptResponseBuilder WithSafetyNote(string message)
    {
        _safetyNotes.Add(new ScriptResponse.SafetyNote(message));
        return this;
    }

    public ScriptResponse Build()
    {
        var response = _createdAt.HasValue
            ? ScriptResponse.Create(_script, _taskDescription, _provider, _model, _scriptType, _riskLevel,
                _createdAt.Value)
            : ScriptResponse.Create(_script, _taskDescription, _provider, _model, _scriptType, _riskLevel);

        foreach (var warning in _warnings) response = response.WithWarning(warning);

        foreach (var safetyNote in _safetyNotes) response = response.WithSafetyNote(safetyNote);

        return response;
    }
}
