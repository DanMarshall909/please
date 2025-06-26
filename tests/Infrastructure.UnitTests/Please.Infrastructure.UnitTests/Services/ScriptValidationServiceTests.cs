using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Infrastructure.Services;

namespace Please.Infrastructure.UnitTests.Services;

public class ScriptValidationServiceTests
{
    private readonly ScriptValidationService _validationService;

    public ScriptValidationServiceTests()
    {
        _validationService = new ScriptValidationService();
    }

    [Fact]
    public void Safe_powershell_script_has_low_risk_level()
    {
        // Arrange
        string safeScript = "Get-Date | Format-Table";

        // Act
        var riskLevel = _validationService.AssessRiskLevel(safeScript, ScriptType.PowerShell);

        // Assert
        riskLevel.ShouldBe(RiskLevel.Low);
    }

    [Fact]
    public void Powershell_script_with_file_creation_has_medium_risk()
    {
        // Arrange
        string script = "New-Item -Path 'C:\\temp\\testfile.txt' -ItemType File";

        // Act
        var riskLevel = _validationService.AssessRiskLevel(script, ScriptType.PowerShell);

        // Assert
        riskLevel.ShouldBe(RiskLevel.Medium);
    }

    [Fact]
    public void Powershell_script_with_file_deletion_has_high_risk()
    {
        // Arrange
        string script = "Remove-Item -Path 'C:\\temp\\*' -Recurse -Force";

        // Act
        var riskLevel = _validationService.AssessRiskLevel(script, ScriptType.PowerShell);

        // Assert
        riskLevel.ShouldBe(RiskLevel.High);
    }

    [Fact]
    public void Script_with_corrupted_ai_tokens_returns_critical_warning()
    {
        // Arrange
        string corruptedScript = "if ($LastExitCode -ne <|begin▁of▁sentence|>) { Write-Host 'Error' }";

        // Act
        var warnings = _validationService.ValidateScript(corruptedScript, ScriptType.PowerShell);


        // Assert
        warnings.ShouldContain(warning => warning.Contains("CRITICAL") && warning.Contains("Corrupted"));
    }

    [Fact]
    public void Script_with_network_download_returns_security_warning()
    {
        // Arrange
        string script = "Invoke-WebRequest -Uri 'https://example.com/script.ps1' | Invoke-Expression";

        // Act
        var warnings = _validationService.ValidateScript(script, ScriptType.PowerShell);

        // Assert
        warnings.ShouldContain(warning => warning.Contains("downloads") && warning.Contains("internet"));
    }

    [Fact]
    public void Script_with_registry_modification_contains_dangerous_operations()
    {
        // Arrange
        string script = "Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows' -Name 'Test' -Value 'Value'";

        // Act
        var isDangerous = _validationService.ContainsDangerousOperations(script, ScriptType.PowerShell);

        // Assert
        isDangerous.ShouldBeTrue();
    }

    [Fact]
    public void Safe_script_does_not_contain_dangerous_operations()
    {
        // Arrange
        string script = "Get-Process | Where-Object { $_.CPU -gt 100 } | Select-Object Name, CPU";

        // Act
        var isDangerous = _validationService.ContainsDangerousOperations(script, ScriptType.PowerShell);

        // Assert
        isDangerous.ShouldBeFalse();
    }

    [Fact]
    public void Script_with_elevated_permissions_generates_safety_note()
    {
        // Arrange
        string script = "Start-Process powershell -Verb RunAs -ArgumentList '-Command', 'Get-Service'";

        // Act
        var safetyNotes = _validationService.GenerateSafetyNotes(script, ScriptType.PowerShell);

        // Assert
        safetyNotes.ShouldContain(note => note.Contains("elevated") || note.Contains("administrator"));
    }

    [Fact]
    public void Bash_script_with_sudo_has_high_risk()
    {
        // Arrange
        string script = "sudo rm -rf /tmp/*";

        // Act
        var riskLevel = _validationService.AssessRiskLevel(script, ScriptType.Bash);

        // Assert
        riskLevel.ShouldBe(RiskLevel.High);
    }

    [Fact]
    public void Script_validation_handles_case_insensitive_patterns()
    {
        // Arrange
        string script = "REMOVE-ITEM -Path 'C:\\temp' -FORCE";

        // Act
        var riskLevel = _validationService.AssessRiskLevel(script, ScriptType.PowerShell);

        // Assert
        riskLevel.ShouldBe(RiskLevel.High);
    }

    [Fact]
    public void Enhanced_script_response_includes_validation_results()
    {
        // Arrange
        var originalResponse = ScriptResponse.Create(
            "Remove-Item -Path 'C:\\temp' -Force",
            "Delete temporary files",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.PowerShell,
            RiskLevel.Low // This will be updated by validation
        );

        // Act
        var enhancedResponse = _validationService.EnhanceWithValidation(originalResponse);

        // Assert
        enhancedResponse.RiskLevel.ShouldBe(RiskLevel.High);
        enhancedResponse.Warnings.ShouldNotBeEmpty();
        enhancedResponse.SafetyNotes.ShouldNotBeEmpty();
    }

    [Fact]
    public void Script_with_multiple_dangerous_patterns_aggregates_highest_risk()
    {
        // Arrange
        string script = @"
            Remove-Item -Path 'C:\temp' -Force
            New-Item -Path 'C:\test' -ItemType Directory
            Get-Process
        ";

        // Act
        var riskLevel = _validationService.AssessRiskLevel(script, ScriptType.PowerShell);

        // Assert
        riskLevel.ShouldBe(RiskLevel.High); // Should take highest risk found
    }
}