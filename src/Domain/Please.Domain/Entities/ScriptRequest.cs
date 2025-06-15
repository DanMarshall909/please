using Please.Domain.Enums;

namespace Please.Domain.Entities;

/// <summary>
/// Represents a request to generate a script
/// </summary>
public record ScriptRequest
{
    public required string TaskDescription { get; init; }
    public ProviderType? Provider { get; init; }
    public string? Model { get; init; }
    public ScriptType? ScriptType { get; init; }
    public DateTime RequestTime { get; init; } = DateTime.UtcNow;
    public string? WorkingDirectory { get; init; }
    public bool ForceExecution { get; init; }
    public Dictionary<string, string> AdditionalParameters { get; init; } = new();

    /// <summary>
    /// Creates a basic script request with just a task description
    /// </summary>
    public static ScriptRequest Create(string taskDescription) => new()
    {
        TaskDescription = taskDescription
    };

    /// <summary>
    /// Creates a script request with provider and model specification
    /// </summary>
    public static ScriptRequest Create(string taskDescription, ProviderType provider, string? model = null) => new()
    {
        TaskDescription = taskDescription,
        Provider = provider,
        Model = model
    };
}
