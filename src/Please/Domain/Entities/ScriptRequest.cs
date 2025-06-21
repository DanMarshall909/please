using Please.Domain.Enums;
using Please.Domain.Common;

namespace Please.Domain.Entities;

public sealed record ScriptRequest
{
    public string TaskDescription { get; init; } = string.Empty;
    public ProviderType? Provider { get; init; }
    public string? Model { get; init; }
    public ScriptType ScriptType { get; init; } = ScriptType.Bash;
    public DateTime RequestTime { get; init; } = DateTime.UtcNow;
    public string? WorkingDirectory { get; init; }
    public bool ForceExecution { get; init; }

    public static Result<ScriptRequest> Create(string taskDescription,
        ProviderType? provider = null,
        string? model = null,
        ScriptType scriptType = ScriptType.Bash)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return Result<ScriptRequest>.Failure("Task description cannot be empty");

        return Result<ScriptRequest>.Success(new ScriptRequest
        {
            TaskDescription = taskDescription.Trim(),
            Provider = provider,
            Model = model,
            ScriptType = scriptType
        });
    }
}
