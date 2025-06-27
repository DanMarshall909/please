using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Services;
using System.Text.RegularExpressions;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Please.Infrastructure.Services;

/// <summary>
/// Service for validating scripts and assessing security risks
/// </summary>
public class ScriptValidationService : IScriptValidationService
{
    private static readonly Dictionary<ScriptType, ValidationRules> ScriptTypeRules = new()
    {
        [ScriptType.PowerShell] = new ValidationRules
        {
            HighRiskPatterns = new[]
            {
                // File system destruction
                @"remove-item.*-force", @"remove-item.*-recurse", @"rm\s+-rf", @"del\s+/[fs]", 
                @"format\s+", @"diskpart", @"rmdir\s+/s",
                
                // System modification
                @"net\s+user", @"reg\s+delete", @"reg\s+add.*hklm", 
                @"shutdown", @"restart-computer", @"stop-computer",
                
                // Code execution
                @"invoke-expression", @"\biex\b", @"powershell\s+-c", 
                @"cmd\s+/c", @"start-process.*-verb\s+runas",
                
                // Network/Download
                @"invoke-webrequest.*invoke-expression", @"wget.*\|", @"curl.*\|",
                @"download.*invoke", @"(iwr|wget|curl).*iex",
                
                // Installation/Uninstallation
                @"msiexec", @"setup\.exe", @"install.*-force",
                @"uninstall-", @"remove-.*package",
                
                // Registry manipulation (high risk areas)
                @"set-itemproperty.*hklm", @"new-itemproperty.*hklm",
                @"remove-itemproperty.*hklm"
            },
            MediumRiskPatterns = new[]
            {
                // File system modification
                @"new-item.*-itemtype.*file", @"copy-item", @"move-item",
                @"rename-item", @"mkdir", @"touch\s+",
                
                // Service management
                @"start-service", @"stop-service", @"restart-service",
                @"set-service", @"new-service",
                
                // Process management
                @"start-process", @"stop-process", @"get-process.*stop",
                
                // Permission changes
                @"set-acl", @"set-executionpolicy", @"chmod\s+",
                @"chown\s+", @"icacls",
                
                // Environment changes
                @"set-variable.*-scope.*global", @"\$env:", @"export\s+",
                
                // Network operations (non-destructive)
                @"invoke-webrequest", @"test-netconnection", @"new-netroute"
            },
            DangerousOperations = new[]
            {
                @"remove-item.*-recurse", @"format", @"diskpart", 
                @"reg\s+delete", @"net\s+user.*delete",
                @"invoke-expression", @"start-process.*-verb\s+runas",
                @"set-itemproperty.*hklm", @"shutdown", @"restart"
            },
            CorruptionPatterns = new[]
            {
                @"<\|begin▁of▁sentence\|>", @"<\|end▁of▁sentence\|>",
                @"<\|.*\|>", @"▁", @"<pad>", @"<unk>", @"<mask>"
            }
        },
        [ScriptType.Bash] = new ValidationRules
        {
            HighRiskPatterns = new[]
            {
                // File system destruction
                @"rm\s+-rf\s+/", @"rm\s+-rf\s+\*", @"rmdir.*-rf",
                @"dd\s+if=/dev/zero", @"mkfs\.", @"fdisk",
                
                // System modification with sudo
                @"sudo\s+rm\s+-rf", @"sudo\s+.*>\s*/", @"sudo\s+dd",
                @"sudo\s+chmod.*777", @"sudo\s+chown.*root",
                
                // Code execution
                @"bash\s+-c.*\$\(", @"sh\s+-c.*\$\(", @"eval\s+\$\(",
                @"curl.*\|\s*bash", @"wget.*\|\s*sh",
                
                // System shutdown/reboot
                @"sudo\s+shutdown", @"sudo\s+reboot", @"sudo\s+halt",
                @"init\s+0", @"init\s+6",
                
                // Package management (destructive)
                @"sudo\s+apt\s+remove.*--purge", @"sudo\s+yum\s+remove",
                @"sudo\s+dnf\s+remove", @"sudo\s+pacman\s+-R"
            },
            MediumRiskPatterns = new[]
            {
                // File operations
                @"cp\s+.*-r", @"mv\s+", @"mkdir\s+", @"touch\s+",
                @"chmod\s+", @"chown\s+",
                
                // Process management
                @"kill\s+-9", @"killall", @"pkill",
                
                // Service management
                @"systemctl\s+start", @"systemctl\s+stop", @"systemctl\s+restart",
                @"service\s+.*start", @"service\s+.*stop",
                
                // Network operations
                @"wget\s+", @"curl\s+", @"scp\s+", @"rsync\s+",
                
                // Environment
                @"export\s+PATH", @"source\s+", @"\.\s+/"
            },
            DangerousOperations = new[]
            {
                @"sudo", @"rm\s+-rf", @"dd\s+", @"mkfs",
                @"fdisk", @"parted", @"eval", @"exec",
                @"curl.*\|.*sh", @"wget.*\|.*bash"
            },
            CorruptionPatterns = new[]
            {
                @"<\|begin▁of▁sentence\|>", @"<\|end▁of▁sentence\|>",
                @"<\|.*\|>", @"▁", @"<pad>", @"<unk>", @"<mask>"
            }
        }
    };

    public RiskLevel AssessRiskLevel(string script, ScriptType scriptType)
    {
        if (string.IsNullOrWhiteSpace(script)) return RiskLevel.Low;

        var rules = ScriptTypeRules.GetValueOrDefault(scriptType) ?? ScriptTypeRules[ScriptType.PowerShell];
        string lowerScript = script.ToLowerInvariant();

        // Check for corrupted AI output first (always critical)
        if (ContainsPatterns(script, rules.CorruptionPatterns))
            return RiskLevel.Critical;

        // Check high risk patterns
        if (ContainsPatterns(lowerScript, rules.HighRiskPatterns))
            return RiskLevel.High;

        // Check medium risk patterns
        if (ContainsPatterns(lowerScript, rules.MediumRiskPatterns))
            return RiskLevel.Medium;

        return RiskLevel.Low;
    }

    public List<string> ValidateScript(string script, ScriptType scriptType)
    {
        var warnings = new List<string>();
        
        if (string.IsNullOrWhiteSpace(script))
        {
            warnings.Add("⚠️ WARNING: Script is empty or contains only whitespace");
            return warnings;
        }

        var rules = ScriptTypeRules.GetValueOrDefault(scriptType) ?? ScriptTypeRules[ScriptType.PowerShell];

        // Check syntax first using native parsers
        var syntaxErrors = ValidateSyntax(script, scriptType);
        if (syntaxErrors.Any())
        {
            warnings.AddRange(syntaxErrors.Select(error => $"💥 SYNTAX ERROR: {error}"));
        }

        // Check for corrupted AI output
        if (ContainsPatterns(script, rules.CorruptionPatterns))
        {
            warnings.Add("⛔ CRITICAL: Corrupted AI response detected. Script contains invalid tokens and should not be executed.");
        }

        // Check for network downloads with execution
        if (ContainsNetworkExecution(script, scriptType))
        {
            warnings.Add("🌐 SECURITY: Script downloads and executes code from the internet. Verify source trustworthiness.");
        }

        // Check for elevated permissions
        if (ContainsElevatedPermissions(script, scriptType))
        {
            warnings.Add("🔐 PRIVILEGE: Script requests elevated/administrator permissions.");
        }

        // Check for file system modifications
        if (ContainsFileSystemRisks(script, scriptType))
        {
            warnings.Add("📁 FILESYSTEM: Script modifies files or directories. Review paths carefully.");
        }

        // Check for registry modifications (Windows)
        if (scriptType == ScriptType.PowerShell && ContainsRegistryModifications(script))
        {
            warnings.Add("🔧 REGISTRY: Script modifies Windows registry. Changes may affect system stability.");
        }

        // Check for service management
        if (ContainsServiceManagement(script, scriptType))
        {
            warnings.Add("⚙️ SERVICES: Script manages system services. May affect system functionality.");
        }

        return warnings;
    }

    public List<string> GenerateSafetyNotes(string script, ScriptType scriptType)
    {
        var notes = new List<string>();
        
        if (string.IsNullOrWhiteSpace(script)) return notes;

        var riskLevel = AssessRiskLevel(script, scriptType);

        switch (riskLevel)
        {
            case RiskLevel.Critical:
                notes.Add("🚫 DO NOT EXECUTE: Script contains corrupted AI output");
                notes.Add("🔄 Request a new script generation");
                break;
                
            case RiskLevel.High:
                notes.Add("⚠️ High risk script - Review carefully before execution");
                notes.Add("💾 Backup important data before running");
                notes.Add("🧪 Test in a safe environment first");
                if (ContainsElevatedPermissions(script, scriptType))
                    notes.Add("🔐 Will require administrator/root privileges");
                break;
                
            case RiskLevel.Medium:
                notes.Add("⚡ Medium risk - Verify script behavior matches expectations");
                notes.Add("👀 Review file paths and operations");
                break;
                
            case RiskLevel.Low:
                notes.Add("✅ Low risk script - Generally safe to execute");
                break;
        }

        // Add specific operational notes
        if (ContainsFileSystemRisks(script, scriptType))
        {
            notes.Add("📂 Ensure target directories and files exist and are correct");
        }

        if (scriptType == ScriptType.PowerShell && script.ToLowerInvariant().Contains("execution") && script.ToLowerInvariant().Contains("policy"))
        {
            notes.Add("🔒 May require PowerShell execution policy changes");
        }

        return notes;
    }

    public bool ContainsDangerousOperations(string script, ScriptType scriptType)
    {
        if (string.IsNullOrWhiteSpace(script)) return false;

        var rules = ScriptTypeRules.GetValueOrDefault(scriptType) ?? ScriptTypeRules[ScriptType.PowerShell];
        return ContainsPatterns(script.ToLowerInvariant(), rules.DangerousOperations);
    }

    public ScriptResponse EnhanceWithValidation(ScriptResponse response)
    {
        if (response == null) return response!;

        var script = response.Script;
        
        // Auto-fix syntax errors if possible
        var syntaxErrors = ValidateSyntax(script, response.ScriptType);
        if (syntaxErrors.Any())
        {
            var fixedScript = AutoFixSyntaxErrors(script, response.ScriptType, syntaxErrors);
            if (!string.IsNullOrEmpty(fixedScript) && fixedScript != script)
            {
                script = fixedScript;
                // Re-validate after fixes
                syntaxErrors = ValidateSyntax(script, response.ScriptType);
            }
        }

        // Re-assess risk level with comprehensive validation
        var actualRiskLevel = AssessRiskLevel(script, response.ScriptType);
        
        // Generate validation warnings
        var warnings = ValidateScript(script, response.ScriptType);
        
        // Generate safety notes
        var safetyNotes = GenerateSafetyNotes(script, response.ScriptType);

        // Combine existing warnings with validation warnings
        var existingWarningMessages = response.Warnings.Select(w => w.Message);
        var allWarnings = existingWarningMessages.Concat(warnings).Distinct().ToList();

        // Create updated response with fixed script if changes were made
        var enhancedResponse = script != response.Script 
            ? ScriptResponse.Create(script, response.TaskDescription, response.Provider, response.Model, response.ScriptType, actualRiskLevel)
            : response.WithRiskLevel(actualRiskLevel);

        return enhancedResponse.WithWarnings(allWarnings)
                              .WithSafetyNotes(safetyNotes);
    }

    private bool ContainsPatterns(string text, string[] patterns)
    {
        return patterns.Any(pattern => Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private bool ContainsNetworkExecution(string script, ScriptType scriptType)
    {
        string lowerScript = script.ToLowerInvariant();
        
        return scriptType switch
        {
            ScriptType.PowerShell => lowerScript.Contains("invoke-webrequest") && (lowerScript.Contains("invoke-expression") || lowerScript.Contains("iex")),
            ScriptType.Bash => Regex.IsMatch(lowerScript, @"(curl|wget).*\|\s*(bash|sh)"),
            _ => false
        };
    }

    private bool ContainsElevatedPermissions(string script, ScriptType scriptType)
    {
        string lowerScript = script.ToLowerInvariant();
        
        return scriptType switch
        {
            ScriptType.PowerShell => lowerScript.Contains("-verb runas") || lowerScript.Contains("start-process") && lowerScript.Contains("runas"),
            ScriptType.Bash => lowerScript.Contains("sudo "),
            _ => false
        };
    }

    private bool ContainsFileSystemRisks(string script, ScriptType scriptType)
    {
        string lowerScript = script.ToLowerInvariant();
        
        var fileSystemPatterns = scriptType switch
        {
            ScriptType.PowerShell => new[] { "remove-item", "new-item", "copy-item", "move-item", "rename-item" },
            ScriptType.Bash => new[] { "rm ", "cp ", "mv ", "mkdir", "rmdir", "touch " },
            _ => Array.Empty<string>()
        };

        return fileSystemPatterns.Any(pattern => lowerScript.Contains(pattern));
    }

    private bool ContainsRegistryModifications(string script)
    {
        string lowerScript = script.ToLowerInvariant();
        return lowerScript.Contains("set-itemproperty") || 
               lowerScript.Contains("new-itemproperty") || 
               lowerScript.Contains("remove-itemproperty") ||
               lowerScript.Contains("reg add") ||
               lowerScript.Contains("reg delete");
    }

    private bool ContainsServiceManagement(string script, ScriptType scriptType)
    {
        string lowerScript = script.ToLowerInvariant();
        
        return scriptType switch
        {
            ScriptType.PowerShell => lowerScript.Contains("start-service") || lowerScript.Contains("stop-service") || lowerScript.Contains("restart-service"),
            ScriptType.Bash => lowerScript.Contains("systemctl") || lowerScript.Contains("service "),
            _ => false
        };
    }

    /// <summary>
    /// Validates script syntax using native language parsers
    /// </summary>
    /// <param name="script">Script content to validate</param>
    /// <param name="scriptType">Type of script (PowerShell, Bash, etc.)</param>
    /// <returns>List of syntax errors found</returns>
    public List<string> ValidateSyntax(string script, ScriptType scriptType)
    {
        return scriptType switch
        {
            ScriptType.PowerShell => ValidatePowerShellSyntax(script),
            ScriptType.Bash => ValidateBashSyntaxBasic(script), // No native bash parser in .NET
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Uses PowerShell's native AST parser to validate syntax
    /// </summary>
    private List<string> ValidatePowerShellSyntax(string script)
    {
        var errors = new List<string>();
        
        try
        {
            // Use PowerShell's AST parser to validate syntax
            Token[] tokens;
            ParseError[] parseErrors;
            
            var ast = Parser.ParseInput(script, out tokens, out parseErrors);
            
            if (parseErrors != null && parseErrors.Length > 0)
            {
                foreach (var error in parseErrors)
                {
                    errors.Add($"Line {error.Extent.StartLineNumber}: {error.Message}");
                }
            }
            
            // Additional semantic validation
            var semanticErrors = ValidatePowerShellSemantics(ast, tokens);
            errors.AddRange(semanticErrors);
        }
        catch (Exception ex)
        {
            errors.Add($"Parser error: {ex.Message}");
        }
        
        return errors;
    }

    /// <summary>
    /// Validates PowerShell semantic issues that the parser might miss
    /// </summary>
    private List<string> ValidatePowerShellSemantics(Ast ast, Token[] tokens)
    {
        var errors = new List<string>();
        
        // Find command invocations and check if cmdlets exist
        var commandAsts = ast.FindAll(testAst => testAst is CommandAst, true).Cast<CommandAst>();
        
        foreach (var commandAst in commandAsts)
        {
            var commandName = commandAst.GetCommandName();
            if (!string.IsNullOrEmpty(commandName))
            {
                // Check for common non-existent cmdlets that AI generates
                if (IsLikelyNonExistentCmdlet(commandName))
                {
                    errors.Add($"Cmdlet '{commandName}' does not exist or is not available");
                }
            }
        }
        
        return errors;
    }

    /// <summary>
    /// Checks if a cmdlet name is likely non-existent (common AI mistakes)
    /// </summary>
    private bool IsLikelyNonExistentCmdlet(string cmdletName)
    {
        var nonExistentCmdlets = new[]
        {
            "Get-ComputerName", // Should be $env:COMPUTERNAME or Get-ComputerInfo
            "Get-SystemInfo",   // Should be Get-ComputerInfo
            "Get-CPUInfo",      // Should be Get-WmiObject Win32_Processor
            "Get-MemoryInfo",   // Should be Get-WmiObject Win32_ComputerSystem
            "Get-DiskInfo",     // Should be Get-WmiObject Win32_LogicalDisk
            "Set-Folder",       // Should be New-Item or Set-Location
            "Create-Directory", // Should be New-Item -ItemType Directory
            "Remove-Folder",    // Should be Remove-Item
            "Copy-File",        // Should be Copy-Item
            "Move-File"         // Should be Move-Item
        };
        
        return nonExistentCmdlets.Contains(cmdletName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Basic Bash syntax validation (no native parser available)
    /// </summary>
    private List<string> ValidateBashSyntaxBasic(string script)
    {
        var errors = new List<string>();
        
        // Use AST-based validation for more accuracy
        try
        {
            // Basic structural validation for Bash
            var lines = script.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;
                
                // Check for incomplete if statements
                if (line.Contains("if [") && !script.Contains("fi"))
                    errors.Add($"Line {i + 1}: Incomplete if statement - missing 'fi'");
                    
                // Check for unmatched quotes in individual lines
                if (HasUnmatchedQuotesInLine(line))
                    errors.Add($"Line {i + 1}: Unmatched quotes detected");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Bash validation error: {ex.Message}");
        }
            
        return errors;
    }

    private bool HasUnmatchedQuotesInLine(string line)
    {
        int doubleQuotes = 0;
        int singleQuotes = 0;
        bool inEscape = false;
        
        foreach (char c in line)
        {
            if (inEscape)
            {
                inEscape = false;
                continue;
            }
            
            if (c == '\\')
            {
                inEscape = true;
                continue;
            }
            
            if (c == '"') doubleQuotes++;
            if (c == '\'') singleQuotes++;
        }
        
        return (doubleQuotes % 2 != 0) || (singleQuotes % 2 != 0);
    }

    /// <summary>
    /// Automatically fixes common syntax errors in scripts
    /// </summary>
    /// <param name="script">Original script with errors</param>
    /// <param name="scriptType">Type of script</param>
    /// <param name="syntaxErrors">List of detected syntax errors</param>
    /// <returns>Fixed script or empty string if cannot fix</returns>
    public string AutoFixSyntaxErrors(string script, ScriptType scriptType, List<string> syntaxErrors)
    {
        if (string.IsNullOrWhiteSpace(script) || !syntaxErrors.Any())
            return script;

        return scriptType switch
        {
            ScriptType.PowerShell => AutoFixPowerShellErrors(script, syntaxErrors),
            ScriptType.Bash => AutoFixBashErrors(script, syntaxErrors),
            _ => script
        };
    }

    /// <summary>
    /// Fixes common PowerShell syntax errors and cmdlet mistakes
    /// </summary>
    private string AutoFixPowerShellErrors(string script, List<string> syntaxErrors)
    {
        var fixedScript = script;
        
        // Fix common non-existent cmdlets
        var cmdletFixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Get-ComputerName", "$env:COMPUTERNAME" },
            { "Get-SystemInfo", "Get-ComputerInfo" },
            { "Get-CPUInfo", "Get-WmiObject -Class Win32_Processor" },
            { "Get-MemoryInfo", "Get-WmiObject -Class Win32_ComputerSystem" },
            { "Get-DiskInfo", "Get-WmiObject -Class Win32_LogicalDisk" },
            { "Create-Directory", "New-Item -ItemType Directory" },
            { "Remove-Folder", "Remove-Item" },
            { "Copy-File", "Copy-Item" },
            { "Move-File", "Move-Item" }
        };

        foreach (var fix in cmdletFixes)
        {
            if (syntaxErrors.Any(error => error.Contains(fix.Key)))
            {
                // Use regex for more precise replacement
                fixedScript = Regex.Replace(fixedScript, 
                    $@"\b{Regex.Escape(fix.Key)}\b", 
                    fix.Value, 
                    RegexOptions.IgnoreCase);
            }
        }

        // Fix common parameter syntax issues
        fixedScript = FixPowerShellParameterSyntax(fixedScript);
        
        // Fix mathematical expressions
        fixedScript = FixPowerShellMathExpressions(fixedScript);
        
        return fixedScript;
    }

    /// <summary>
    /// Fixes PowerShell parameter syntax issues
    /// </summary>
    private string FixPowerShellParameterSyntax(string script)
    {
        // Fix common parameter grouping issues
        // Example: (.TotalPhysicalMemory / 1GB) + " GB" should be ((.TotalPhysicalMemory / 1GB) + " GB")
        var pattern = @"(\w+)\s*=\s*([^\s]+)\s*/\s*(\d+\w+)\s*\+\s*([""'][^""']*[""'])";
        var replacement = "$1 = (($2 / $3) + $4)";
        
        return Regex.Replace(script, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }

    /// <summary>
    /// Fixes PowerShell mathematical expression syntax
    /// </summary>
    private string FixPowerShellMathExpressions(string script)
    {
        // Fix expressions like: RAM = (...).TotalPhysicalMemory / 1GB + " GB"
        // Should be: RAM = ((...).TotalPhysicalMemory / 1GB) + " GB"
        var lines = script.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Contains("TotalPhysicalMemory") && line.Contains("/") && line.Contains("GB"))
            {
                // Fix the mathematical expression grouping
                var pattern = @"(\w+)\s*=\s*(.+?\.TotalPhysicalMemory)\s*/\s*(\d+GB)\s*\+\s*(.+)";
                var match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    lines[i] = $"{match.Groups[1].Value} = (({match.Groups[2].Value} / {match.Groups[3].Value}) + {match.Groups[4].Value})";
                }
            }
        }
        
        return string.Join('\n', lines);
    }

    /// <summary>
    /// Fixes common Bash syntax errors
    /// </summary>
    private string AutoFixBashErrors(string script, List<string> syntaxErrors)
    {
        var fixedScript = script;
        
        // Fix incomplete if statements
        if (syntaxErrors.Any(error => error.Contains("if") && error.Contains("fi")))
        {
            if (fixedScript.Contains("if [") && !fixedScript.Contains("fi"))
            {
                fixedScript += "\nfi";
            }
        }
        
        return fixedScript;
    }

    private class ValidationRules
    {
        public string[] HighRiskPatterns { get; set; } = Array.Empty<string>();
        public string[] MediumRiskPatterns { get; set; } = Array.Empty<string>();
        public string[] DangerousOperations { get; set; } = Array.Empty<string>();
        public string[] CorruptionPatterns { get; set; } = Array.Empty<string>();
    }
}