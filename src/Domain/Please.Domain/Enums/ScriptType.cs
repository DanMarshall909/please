namespace Please.Domain.Enums;

/// <summary>
/// Types of scripts that can be generated
/// </summary>
public enum ScriptType
{
    PowerShell,
    Bash,
    Command,
    Python,
    Auto // Let the AI decide based on OS/context
}
