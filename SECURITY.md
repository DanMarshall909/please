# Security Documentation

## Overview

Please implements comprehensive security measures to protect users from dangerous scripts while maintaining usability for legitimate automation tasks.

## Security Architecture

### Multi-Layer Validation
1. **Syntax Validation**: PowerShell AST parser detects malformed scripts
2. **Security Analysis**: Pattern-based detection of dangerous operations
3. **Risk Assessment**: 4-tier classification with specific warnings
4. **Auto-Fix Integration**: Safe correction of common syntax errors

### Risk Assessment Levels

| Level | Description | Example Operations |
|-------|-------------|-------------------|
| **Low** | Safe read-only operations | Get-Date, Get-Process, Format-Table |
| **Medium** | File system modifications | New-Item, Copy-Item, Move-Item |
| **High** | System changes, deletions | Remove-Item -Force, Registry modifications |
| **Critical** | Destructive or corrupted | Format disk, corrupted AI output |

## Syntax Validation and Auto-Fix

### PowerShell AST Parser
Native PowerShell syntax validation using Microsoft.PowerShell.SDK:
```csharp
var ast = Parser.ParseInput(script, out tokens, out parseErrors);
if (parseErrors.Length > 0)
{
    foreach (var error in parseErrors)
        errors.Add($"Line {error.Extent.StartLineNumber}: {error.Message}");
}
```

### Semantic Validation
Detection of non-existent cmdlets and common AI mistakes:
```csharp
private bool IsLikelyNonExistentCmdlet(string cmdletName)
{
    var nonExistentCmdlets = new[]
    {
        "Get-ComputerName", // Should be $env:COMPUTERNAME
        "Get-SystemInfo",   // Should be Get-ComputerInfo
        "Get-CPUInfo",      // Should be Get-WmiObject Win32_Processor
    };
    return nonExistentCmdlets.Contains(cmdletName, StringComparer.OrdinalIgnoreCase);
}
```

### Auto-Fix Engine
Automatic correction of common AI syntax errors:
```csharp
var cmdletFixes = new Dictionary<string, string>
{
    { "Get-ComputerName", "$env:COMPUTERNAME" },
    { "Get-SystemInfo", "Get-ComputerInfo" },
    { "Get-CPUInfo", "Get-WmiObject -Class Win32_Processor" },
    { "Create-Directory", "New-Item -ItemType Directory" }
};
```

## Security Pattern Detection

### High-Risk Patterns
- **File System Destruction**: `rm -rf`, `Remove-Item -Force -Recurse`
- **System Modification**: Registry changes, service management
- **Code Execution**: `Invoke-Expression`, `Invoke-WebRequest | iex`
- **Network Downloads**: Remote script execution
- **Administrative Actions**: `Start-Process -Verb RunAs`

### Medium-Risk Patterns
- **File Operations**: Creation, copying, moving files
- **Service Management**: Start/stop services
- **Process Management**: Start/stop processes
- **Permission Changes**: ACL modifications

### Corruption Detection
Detection of corrupted AI output:
```csharp
var corruptionPatterns = new[]
{
    @"<\|begin▁of▁sentence\|>", @"<\|end▁of▁sentence\|>",
    @"<\|.*\|>", @"▁", @"<pad>", @"<unk>", @"<mask>"
};
```

## API Key Security

### Storage Priority Chain
1. **Environment Variables** (highest security)
2. **Encrypted Local Storage** (Windows DPAPI)
3. **User Secrets** (development only)
4. **Configuration Files** (non-sensitive data only)

### Windows DPAPI Encryption
Secure local storage for API keys:
```csharp
public void StoreApiKey(string provider, string apiKey)
{
    var encrypted = ProtectedData.Protect(
        Encoding.UTF8.GetBytes(apiKey),
        null,
        DataProtectionScope.CurrentUser
    );
    // Store encrypted data locally
}
```

### Memory Security
- **SecureString Usage**: Sensitive data cleared from memory
- **Automatic Cleanup**: Disposal patterns for sensitive objects
- **No Logging**: API keys never written to logs or console

## Runtime Safety Features

### Non-Interactive Environment Detection
Safe defaults for CI/automation environments:
```csharp
if (Environment.GetEnvironmentVariable("CI") == "true" || 
    Environment.GetEnvironmentVariable("TERM") == "dumb" ||
    !Environment.UserInteractive)
{
    // Default to cancel for safety
    return false;
}
```

### Execution Confirmation
Multi-step confirmation process:
1. **Script Display**: Full script content with syntax highlighting
2. **Risk Assessment**: Clear risk level and warnings
3. **User Confirmation**: Explicit execution approval
4. **Final Review**: Last chance to cancel

### External Editor Security
- **Temporary Files**: Secure temp file handling
- **File Cleanup**: Automatic cleanup after editing
- **Re-validation**: Full security check after editing
- **Path Validation**: Secure temp directory usage

## Validation Integration

### Enhanced Validation Workflow
```csharp
public ScriptResponse EnhanceWithValidation(ScriptResponse response)
{
    // 1. Syntax validation with auto-fix
    var syntaxErrors = ValidateSyntax(script, scriptType);
    if (syntaxErrors.Any())
    {
        script = AutoFixSyntaxErrors(script, scriptType, syntaxErrors);
        syntaxErrors = ValidateSyntax(script, scriptType); // Re-validate
    }

    // 2. Security analysis
    var riskLevel = AssessRiskLevel(script, scriptType);
    var warnings = ValidateScript(script, scriptType);
    var safetyNotes = GenerateSafetyNotes(script, scriptType);

    // 3. Return enhanced response
    return updatedResponse.WithValidation(riskLevel, warnings, safetyNotes);
}
```

### Validation Rules Configuration
Extensible pattern-based validation:
```csharp
private static readonly Dictionary<ScriptType, ValidationRules> ScriptTypeRules = new()
{
    [ScriptType.PowerShell] = new ValidationRules
    {
        HighRiskPatterns = new[] { "remove-item.*-force", "invoke-expression" },
        MediumRiskPatterns = new[] { "new-item.*file", "start-service" },
        CorruptionPatterns = new[] { @"<\|.*\|>", "▁" }
    }
};
```

## Security Recommendations

### For Users
- **Review Scripts**: Always review generated scripts before execution
- **Test Environment**: Use non-production environments for testing
- **Backup Data**: Create backups before running file operation scripts
- **Check Permissions**: Verify script permissions match requirements

### For Developers
- **Input Validation**: Validate all user inputs and AI responses
- **Error Handling**: Use Result pattern for explicit error handling
- **Security Testing**: Include security-focused unit and integration tests
- **Regular Updates**: Keep security patterns updated with new threats

### For Organizations
- **Policy Configuration**: Customize validation rules for organizational policies
- **Audit Logging**: Implement audit trails for script generation and execution
- **Environment Variables**: Use environment variables for API key management
- **Training**: Educate users on script security best practices

## Security Testing

### Comprehensive Test Coverage
- **Syntax Validation Tests**: PowerShell AST parser testing
- **Security Pattern Tests**: Dangerous operation detection
- **Auto-Fix Tests**: Correction mechanism validation
- **Integration Tests**: End-to-end security workflow testing

### Test Examples
```csharp
[Fact]
public void Script_with_corrupted_ai_tokens_returns_critical_warning()
{
    var corruptedScript = "if ($LastExitCode -ne <|begin▁of▁sentence|>) { Write-Host 'Error' }";
    var warnings = _validationService.ValidateScript(corruptedScript, ScriptType.PowerShell);
    warnings.ShouldContain(warning => warning.Contains("CRITICAL") && warning.Contains("Corrupted"));
}
```

## Incident Response

### Security Issue Handling
1. **Detection**: Automated detection through validation rules
2. **Assessment**: Risk level determination and impact analysis
3. **Response**: Immediate warning display and execution prevention
4. **Remediation**: Auto-fix application when possible
5. **Reporting**: Clear user communication about risks and fixes

### Continuous Improvement
- **Pattern Updates**: Regular updates to security detection patterns
- **False Positive Reduction**: Refinement of validation rules
- **User Feedback**: Integration of user-reported security concerns
- **Threat Intelligence**: Incorporation of new security threat information

This security framework provides comprehensive protection while maintaining usability for legitimate automation and scripting tasks.