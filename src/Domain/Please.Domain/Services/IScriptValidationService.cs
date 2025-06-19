using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Domain.Services;

/// <summary>
/// Contract for script validation and risk assessment
/// </summary>
public interface IScriptValidationService
{
    /// <summary>
    /// Assesses the risk level of a script based on its content
    /// </summary>
    RiskLevel AssessRiskLevel(string script, ScriptType scriptType);

    /// <summary>
    /// Validates a script and returns warnings about potentially dangerous operations
    /// </summary>
    List<string> ValidateScript(string script, ScriptType scriptType);

    /// <summary>
    /// Generates safety notes for a script
    /// </summary>
    List<string> GenerateSafetyNotes(string script, ScriptType scriptType);

    /// <summary>
    /// Checks if a script contains specific dangerous patterns
    /// </summary>
    bool ContainsDangerousOperations(string script, ScriptType scriptType);

    /// <summary>
    /// Enhances a script response with validation results
    /// </summary>
    ScriptResponse EnhanceWithValidation(ScriptResponse response);
}
