package script

import (
	"strings"
	"testing"

	"please/types"
)

func TestValidateScript(t *testing.T) {
	tests := []struct {
		name           string
		script         string
		scriptType     string
		expectedCount  int
		expectedLevel  string // "critical", "warning", "caution", "info", "none"
		description    string
	}{
		{
			name:          "PowerShell Date Format - Should NOT Warn",
			script:        `$currentTime = Get-Date -Format "HH:mm:ss"`,
			scriptType:    "powershell",
			expectedCount: 0,
			expectedLevel: "none",
			description:   "PowerShell -Format parameter should not trigger format warning",
		},
		{
			name:          "System Information Script - Should NOT Warn",
			script:        `$computerName = (Get-WmiObject -Class Win32_ComputerSystem).Name`,
			scriptType:    "powershell",
			expectedCount: 0,
			expectedLevel: "none",
			description:   "Basic system info script should be safe",
		},
		{
			name:          "Critical - Format C Drive",
			script:        `format c: /fs:ntfs`,
			scriptType:    "powershell",
			expectedCount: 2, // Critical + short script info
			expectedLevel: "critical",
			description:   "Formatting C: drive should trigger critical warning",
		},
		{
			name:          "Critical - Delete Filesystem",
			script:        `rm -rf /`,
			scriptType:    "bash",
			expectedCount: 4, // Critical + caution + shebang + short script
			expectedLevel: "critical",
			description:   "Deleting root filesystem should trigger critical warning",
		},
		{
			name:          "Warning - Shutdown System",
			script:        `shutdown /s /t 0`,
			scriptType:    "powershell",
			expectedCount: 2, // Warning + short script info
			expectedLevel: "warning",
			description:   "Shutdown command should trigger warning",
		},
		{
			name:          "Caution - Recursive Delete with Path",
			script:        `rm -rf ./old_logs`,
			scriptType:    "bash",
			expectedCount: 3, // Caution + shebang info + short script info
			expectedLevel: "caution",
			description:   "Recursive delete should trigger caution",
		},
		{
			name:          "Info - Missing Shebang",
			script:        `echo "Hello World"\necho "Test"`,
			scriptType:    "bash",
			expectedCount: 1,
			expectedLevel: "info",
			description:   "Bash script without shebang should trigger info",
		},
		{
			name:          "Safe - String with Format",
			script:        `Write-Output "The format is: HH:mm:ss"`,
			scriptType:    "powershell",
			expectedCount: 0,
			expectedLevel: "none",
			description:   "String containing 'format' should not trigger warning",
		},
		{
			name:          "Safe - Complex PowerShell",
			script: `try {
    $currentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $computerName = $env:COMPUTERNAME
    Write-Output "Computer: $computerName, Time: $currentTime"
} catch {
    Write-Error "Failed to get information: $_"
}`,
			scriptType:    "powershell",
			expectedCount: 0,
			expectedLevel: "none",
			description:   "Complex but safe PowerShell should not warn",
		},
		{
			name:          "Multiple Warnings",
			script:        `rm -rf /tmp/test\nshutdown -h now`,
			scriptType:    "bash",
			expectedCount: 4, // Caution (rm -rf) + Warning (shutdown) + Info (shebang) + Info (error handling)
			expectedLevel: "multiple",
			description:   "Script with multiple issues should show multiple warnings",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			response := &types.ScriptResponse{
				Script:     tt.script,
				ScriptType: tt.scriptType,
			}

			warnings := ValidateScript(response)
			
			// Check warning count
			if len(warnings) != tt.expectedCount {
				t.Errorf("Expected %d warnings, got %d for script: %s\nWarnings: %v", 
					tt.expectedCount, len(warnings), tt.script, warnings)
			}

			// Check warning levels if we expect warnings
			if tt.expectedCount > 0 && len(warnings) > 0 {
				switch tt.expectedLevel {
				case "critical":
					if !containsLevel(warnings, "⛔ CRITICAL") {
						t.Errorf("Expected critical warning for: %s\nGot: %v", tt.description, warnings)
					}
				case "warning":
					if !containsLevel(warnings, "🔴 WARNING") {
						t.Errorf("Expected warning level for: %s\nGot: %v", tt.description, warnings)
					}
				case "caution":
					if !containsLevel(warnings, "🟡 CAUTION") {
						t.Errorf("Expected caution level for: %s\nGot: %v", tt.description, warnings)
					}
				case "info":
					if !containsLevel(warnings, "🟢 INFO") {
						t.Errorf("Expected info level for: %s\nGot: %v", tt.description, warnings)
					}
				}
			}
		})
	}
}

func TestContainsCommand(t *testing.T) {
	tests := []struct {
		name     string
		script   string
		pattern  string
		expected bool
		reason   string
	}{
		{
			name:     "PowerShell Format Parameter",
			script:   `Get-Date -Format "HH:mm:ss"`,
			pattern:  "format",
			expected: false,
			reason:   "Should not match -Format parameter",
		},
		{
			name:     "Actual Format Command",
			script:   `format c: /fs:ntfs`,
			pattern:  "format c:",
			expected: true,
			reason:   "Should match actual format command",
		},
		{
			name:     "Quoted Format String",
			script:   `echo "format this text"`,
			pattern:  "format",
			expected: false,
			reason:   "Should not match quoted strings",
		},
		{
			name:     "Single Quoted Format",
			script:   `echo 'format this text'`,
			pattern:  "format",
			expected: false,
			reason:   "Should not match single quoted strings",
		},
		{
			name:     "Standalone Dangerous Command",
			script:   `rm -rf /tmp`,
			pattern:  "rm -rf",
			expected: true,
			reason:   "Should match standalone dangerous commands",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := containsCommand(strings.ToLower(tt.script), tt.pattern)
			if result != tt.expected {
				t.Errorf("containsCommand(%q, %q) = %v, expected %v: %s", 
					tt.script, tt.pattern, result, tt.expected, tt.reason)
			}
		})
	}
}

// Helper function to check if warnings contain a specific level
func containsLevel(warnings []string, level string) bool {
	for _, warning := range warnings {
		if strings.Contains(warning, level) {
			return true
		}
	}
	return false
}

// Benchmark the validation function
func BenchmarkValidateScript(b *testing.B) {
	response := &types.ScriptResponse{
		Script: `try {
    $currentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $computerName = $env:COMPUTERNAME
    $processes = Get-Process | Sort-Object CPU -Descending | Select-Object -First 10
    Write-Output "Computer: $computerName"
    Write-Output "Time: $currentTime"
    Write-Output "Top processes:"
    $processes | ForEach-Object { Write-Output "$($_.Name): $($_.CPU)" }
} catch {
    Write-Error "Failed to get system information: $_"
}`,
		ScriptType: "powershell",
	}

	for i := 0; i < b.N; i++ {
		ValidateScript(response)
	}
}
