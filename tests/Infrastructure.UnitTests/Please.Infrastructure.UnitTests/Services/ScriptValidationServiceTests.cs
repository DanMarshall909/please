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

    [Fact]
    public void Generated_powershell_script_has_valid_syntax()
    {
        // Arrange: Sample generated PowerShell script
        string generatedScript = "# Get the current date\nGet-Date -Format \"yyyy-MM-dd HH:mm:ss\"";

        // Act: Validate PowerShell syntax using native parser
        var syntaxErrors = _validationService.ValidateSyntax(generatedScript, ScriptType.PowerShell);

        // Assert: Script should have no syntax errors
        syntaxErrors.ShouldBeEmpty();
    }

    [Fact]
    public void Generated_script_with_syntax_errors_is_detected()
    {
        // Arrange: Script with common syntax errors
        string invalidScript = @"
            Get-Date -Format ""unclosed quote
            if ($true {
                Write-Host 'Missing closing brace'
        ";

        // Act: Validate syntax using native parser
        var syntaxErrors = _validationService.ValidateSyntax(invalidScript, ScriptType.PowerShell);

        // Assert: Should detect syntax errors
        syntaxErrors.ShouldNotBeEmpty();
    }

    [Fact]
    public void Generated_bash_script_has_valid_syntax()
    {
        // Arrange: Sample generated Bash script
        string bashScript = "#!/bin/bash\necho \"Current date: $(date)\"";

        // Act: Validate basic Bash syntax patterns
        var syntaxErrors = _validationService.ValidateSyntax(bashScript, ScriptType.Bash);

        // Assert: Script should have no syntax errors
        syntaxErrors.ShouldBeEmpty();
    }

    [Fact]
    public void Auto_fix_corrects_nonexistent_cmdlets()
    {
        // Arrange: Script with non-existent cmdlet
        string scriptWithBadCmdlet = "$computerName = Get-ComputerName";
        var syntaxErrors = new List<string> { "Cmdlet 'Get-ComputerName' does not exist" };

        // Act: Auto-fix the script
        var fixedScript = _validationService.AutoFixSyntaxErrors(scriptWithBadCmdlet, ScriptType.PowerShell, syntaxErrors);

        // Assert: Should replace with correct syntax
        fixedScript.ShouldBe("$computerName = $env:COMPUTERNAME");
    }

    [Fact]
    public void Auto_fix_corrects_mathematical_expressions()
    {
        // Arrange: Script with malformed mathematical expression
        string scriptWithMathError = "RAM = (Get-WmiObject -Class Win32_ComputerSystem).TotalPhysicalMemory / 1GB + \" GB\"";
        var syntaxErrors = new List<string> { "Mathematical expression syntax error" };

        // Act: Auto-fix the script
        var fixedScript = _validationService.AutoFixSyntaxErrors(scriptWithMathError, ScriptType.PowerShell, syntaxErrors);

        // Assert: Should add proper parentheses
        fixedScript.ShouldContain("((");
        fixedScript.ShouldContain(") +");
    }

    [Fact]
    public void Enhancement_with_validation_auto_fixes_and_updates_script()
    {
        // Arrange: Script response with syntax errors
        var responseWithErrors = ScriptResponse.Create(
            "Get-ComputerName | Out-Host",
            "Get computer name",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.PowerShell,
            RiskLevel.Low
        );

        // Act: Enhance with validation (should auto-fix)
        var enhancedResponse = _validationService.EnhanceWithValidation(responseWithErrors);

        // Assert: Script should be fixed
        enhancedResponse.Script.ShouldNotBe(responseWithErrors.Script);
        enhancedResponse.Script.ShouldContain("$env:COMPUTERNAME");
    }

    [Fact]
    public void Generated_script_with_nonexistent_cmdlet_is_detected()
    {
        // Arrange: Script with non-existent cmdlet (common AI mistake)
        string scriptWithBadCmdlet = "Get-ComputerName | Out-Host";

        // Act: Validate syntax and semantics
        var syntaxErrors = _validationService.ValidateSyntax(scriptWithBadCmdlet, ScriptType.PowerShell);

        // Assert: Should detect non-existent cmdlet
        syntaxErrors.ShouldContain(error => error.Contains("Get-ComputerName") && error.Contains("does not exist"));
    }

    [Fact]
    public void PowerShell_native_parser_detects_actual_syntax_errors()
    {
        // Arrange: Script with real PowerShell syntax errors
        string syntaxErrorScript = @"
            # Missing closing quote
            Write-Host ""Hello world
            
            # Missing closing parenthesis
            if ($true -and ($false
            {
                Write-Host 'Test'
            }
        ";

        // Act: Use native PowerShell parser
        var syntaxErrors = _validationService.ValidateSyntax(syntaxErrorScript, ScriptType.PowerShell);

        // Assert: Should detect multiple syntax errors
        syntaxErrors.Count.ShouldBeGreaterThan(0);
        syntaxErrors.ShouldContain(error => error.Contains("quote") || error.Contains("string"));
    }

    [Fact]
    public void Bash_auto_fix_adds_missing_fi_statement()
    {
        // Arrange: Bash script missing fi
        string incompleteBash = "if [ $? -eq 0 ]; then\n  echo 'Success'";
        var syntaxErrors = new List<string> { "Incomplete if statement - missing 'fi'" };

        // Act: Auto-fix the script
        var fixedScript = _validationService.AutoFixSyntaxErrors(incompleteBash, ScriptType.Bash, syntaxErrors);

        // Assert: Should add fi statement
        fixedScript.ShouldContain("fi");
    }
}