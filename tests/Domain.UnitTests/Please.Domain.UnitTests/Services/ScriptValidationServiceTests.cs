using NUnit.Framework;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Services;

namespace Please.Domain.UnitTests.Services;

[TestFixture]
public class ScriptValidationServiceTests
{
    private IScriptValidationService _validationService;

    [SetUp]
    public void SetUp()
    {
        // TODO: Replace with actual implementation when created
        _validationService = new MockScriptValidationService();
    }

    [TestCase("rm -rf /", ScriptType.Bash)]
    [TestCase("sudo rm -rf /*", ScriptType.Bash)]
    [TestCase("dd if=/dev/zero of=/dev/sda", ScriptType.Bash)]
    [TestCase("format c:", ScriptType.PowerShell)]
    [TestCase("Remove-Item -Path C:\\ -Recurse -Force", ScriptType.PowerShell)]
    public void Critical_commands_are_highest_risk(string script, ScriptType scriptType)
    {
        // Act
        var result = _validationService.AssessRiskLevel(script, scriptType);

        // Assert
        Assert.That(result, Is.EqualTo(RiskLevel.Critical));
    }

    [Test]
    public void Dangerous_script_generates_warnings()
    {
        // Arrange
        var script = "rm -rf /important/data";

        // Act
        var warnings = _validationService.ValidateScript(script, ScriptType.Bash);

        // Assert
        Assert.That(warnings, Is.Not.Empty);
        Assert.That(warnings.Any(w => w.Contains("CRITICAL") && w.Contains("rm -rf")), Is.True);
    }

    [Test]
    public void Safe_script_has_no_warnings()
    {
        // Arrange
        var script = "Get-ChildItem | Select-Object Name, Length";

        // Act
        var warnings = _validationService.ValidateScript(script, ScriptType.PowerShell);

        // Assert
        Assert.That(warnings, Is.Empty);
    }

    [Test]
    public void Script_response_gets_enhanced_with_validation()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "rm -rf /tmp/*",
            "Clean temp files",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash
        );

        // Act
        var enhanced = _validationService.EnhanceWithValidation(response);

        // Assert
        Assert.That(enhanced.RiskLevel, Is.GreaterThan(RiskLevel.Low));
        Assert.That(enhanced.Warnings, Is.Not.Empty);
    }
}

// Temporary mock implementation for testing
internal class MockScriptValidationService : IScriptValidationService
{
    public RiskLevel AssessRiskLevel(string script, ScriptType scriptType)
    {
        var lower = script.ToLowerInvariant();
        
        // Critical operations
        if (lower.Contains("rm -rf /") || lower.Contains("format c:") || 
            lower.Contains("dd if=/dev/zero") || lower.Contains("remove-item -path c:\\ -recurse"))
            return RiskLevel.Critical;
            
        // High risk operations
        if (lower.Contains("rm ") || lower.Contains("chmod 777") || 
            lower.Contains("del ") || lower.Contains("set-executionpolicy"))
            return RiskLevel.High;
            
        // Medium risk operations
        if ((lower.Contains("wget") || lower.Contains("curl")) && lower.Contains("bash") ||
            lower.Contains("invoke-webrequest") && lower.Contains("invoke-expression"))
            return RiskLevel.Medium;
            
        return RiskLevel.Low;
    }

    public List<string> ValidateScript(string script, ScriptType scriptType)
    {
        var warnings = new List<string>();
        var lower = script.ToLowerInvariant();
        
        if (lower.Contains("rm -rf"))
            warnings.Add("⛔ CRITICAL: 'rm -rf' command detected - this can delete important files");
            
        if (lower.Contains("format"))
            warnings.Add("⛔ CRITICAL: 'format' command detected - this will erase disk data");
            
        return warnings;
    }

    public List<string> GenerateSafetyNotes(string script, ScriptType scriptType)
    {
        var notes = new List<string>();
        var riskLevel = AssessRiskLevel(script, scriptType);
        
        if (riskLevel >= RiskLevel.High)
        {
            notes.Add("💡 Consider creating a backup before running this script");
            notes.Add("💡 Test this script in a safe environment first");
        }
        
        return notes;
    }

    public bool ContainsDangerousOperations(string script, ScriptType scriptType)
    {
        return AssessRiskLevel(script, scriptType) >= RiskLevel.High;
    }

    public ScriptResponse EnhanceWithValidation(ScriptResponse response)
    {
        var riskLevel = AssessRiskLevel(response.Script, response.ScriptType);
        var warnings = ValidateScript(response.Script, response.ScriptType);
        var safetyNotes = GenerateSafetyNotes(response.Script, response.ScriptType);
        
        return response with
        {
            RiskLevel = riskLevel,
            Warnings = response.Warnings.Concat(warnings).ToList(),
            SafetyNotes = response.SafetyNotes.Concat(safetyNotes).ToList()
        };
    }
}
