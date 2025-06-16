using Please.Domain.Common;
using Please.Domain.Enums;

namespace Please.Domain.Entities;

public sealed class ScriptResponse
{
    public ScriptId Id { get; private set; } = ScriptId.Empty;
    public string Script { get; private set; } = string.Empty;
    public string TaskDescription { get; private set; } = string.Empty;
    public ProviderType Provider { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public ScriptType ScriptType { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public List<string> Warnings { get; private set; } = new();
    public List<string> SafetyNotes { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    private ScriptResponse() { }

    public static Result<ScriptResponse> Create(
        string script,
        string taskDescription,
        ProviderType provider,
        string model,
        ScriptType scriptType = ScriptType.Bash,
        RiskLevel riskLevel = RiskLevel.Low)
    {
        if (string.IsNullOrWhiteSpace(script))
            return Result<ScriptResponse>.Failure("Script content cannot be empty");
        if (string.IsNullOrWhiteSpace(taskDescription))
            return Result<ScriptResponse>.Failure("Task description cannot be empty");
        if (string.IsNullOrWhiteSpace(model))
            return Result<ScriptResponse>.Failure("Model cannot be empty");

        return Result<ScriptResponse>.Success(new ScriptResponse
        {
            Id = ScriptId.New(),
            Script = script.Trim(),
            TaskDescription = taskDescription.Trim(),
            Provider = provider,
            Model = model.Trim(),
            ScriptType = scriptType,
            RiskLevel = riskLevel,
            CreatedAt = DateTime.UtcNow
        });
    }

    public ScriptResponse WithWarning(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning))
            Warnings.Add(warning);
        return this;
    }

    public ScriptResponse WithSafetyNote(string note)
    {
        if (!string.IsNullOrWhiteSpace(note))
            SafetyNotes.Add(note);
        return this;
    }

    public bool RequiresConfirmation =>
        RiskLevel >= RiskLevel.Medium || Warnings.Any();

    public bool IsDangerous => RiskLevel == RiskLevel.High;
}
