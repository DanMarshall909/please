using Please.Domain.Enums;

namespace Please.Domain.Entities;

/// <summary>
/// Represents the response from script generation
/// </summary>
public partial record ScriptResponse
{
    public required string Script { get; init; }
    public required string TaskDescription { get; init; }
    public required ProviderType Provider { get; init; }
    public required string Model { get; init; }
    public required ScriptType ScriptType { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public RiskLevel RiskLevel { get; init; }
    public List<Warning> Warnings { get; init; } = [];
    public List<SafetyNote> SafetyNotes { get; init; } = [];
    public string? Explanation { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = [];

    /// <summary>
    /// Determines if the script requires user confirmation before execution
    /// </summary>
    public bool RequiresConfirmation => RiskLevel >= RiskLevel.Medium || Warnings.Count != 0;

    /// <summary>
    /// Checks if the script contains potentially dangerous operations
    /// </summary>
    public bool IsDangerous => RiskLevel >= RiskLevel.High;

    /// <summary>
    /// Creates a successful script response
    /// </summary>
    public static ScriptResponse Create(
        string script,
        string taskDescription,
        ProviderType provider,
        string model,
        ScriptType scriptType,
        RiskLevel riskLevel = RiskLevel.Low) => new()
        {
            Script = script,
            TaskDescription = taskDescription,
            Provider = provider,
            Model = model,
            ScriptType = scriptType,
            RiskLevel = riskLevel
        };

    /// <summary>
    /// Adds a warning to the response
    /// </summary>
    public ScriptResponse WithWarning(Warning warning) => this with
    {
        Warnings = [.. Warnings, warning]
    };

    /// <summary>
    /// Adds a safety note to the response
    /// </summary>
    public ScriptResponse WithSafetyNote(SafetyNote note) => this with
    {
        SafetyNotes = [.. SafetyNotes, note]
    };

    /// <summary>
    /// Sets the risk level
    /// </summary>
    public ScriptResponse WithRiskLevel(RiskLevel riskLevel) => this with
    {
        RiskLevel = riskLevel
    };
}

public interface IMessage
{
    string Message { get; }
}