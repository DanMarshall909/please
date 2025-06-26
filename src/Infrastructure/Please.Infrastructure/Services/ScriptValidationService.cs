using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Services;
using System.Text.RegularExpressions;

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

        // Re-assess risk level with comprehensive validation
        var actualRiskLevel = AssessRiskLevel(response.Script, response.ScriptType);
        
        // Generate validation warnings
        var warnings = ValidateScript(response.Script, response.ScriptType);
        
        // Generate safety notes
        var safetyNotes = GenerateSafetyNotes(response.Script, response.ScriptType);

        // Combine existing warnings with validation warnings
        var existingWarningMessages = response.Warnings.Select(w => w.Message);
        var allWarnings = existingWarningMessages.Concat(warnings).Distinct().ToList();

        return response.WithRiskLevel(actualRiskLevel)
                      .WithWarnings(allWarnings)
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

    private class ValidationRules
    {
        public string[] HighRiskPatterns { get; set; } = Array.Empty<string>();
        public string[] MediumRiskPatterns { get; set; } = Array.Empty<string>();
        public string[] DangerousOperations { get; set; } = Array.Empty<string>();
        public string[] CorruptionPatterns { get; set; } = Array.Empty<string>();
    }
}