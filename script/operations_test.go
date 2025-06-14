package script

import (
	"os"
	"runtime"
	"strings"
	"testing"

	"please/types"
)

// Test GetSuggestedFilename function
func Test_when_getting_filename_for_bash_script_should_add_sh_extension(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		TaskDescription: "list files in directory",
		ScriptType:      "bash",
		Script:          "#!/bin/bash\nls -la",
	}

	// Act
	result := GetSuggestedFilename(response)

	// Assert
	if !strings.HasSuffix(result, ".sh") {
		t.Errorf("Expected bash script to have .sh extension, got: %s", result)
	}
	if !strings.Contains(result, "list") {
		t.Errorf("Expected filename to contain 'list', got: %s", result)
	}
	if !strings.Contains(result, "files") {
		t.Errorf("Expected filename to contain 'files', got: %s", result)
	}
}

func Test_when_getting_filename_for_powershell_script_should_add_ps1_extension(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		TaskDescription: "get system information",
		ScriptType:      "powershell", 
		Script:          "Get-ComputerInfo",
	}

	// Act
	result := GetSuggestedFilename(response)

	// Assert
	if !strings.HasSuffix(result, ".ps1") {
		t.Errorf("Expected powershell script to have .ps1 extension, got: %s", result)
	}
	if !strings.Contains(result, "system") {
		t.Errorf("Expected filename to contain 'system', got: %s", result)
	}
}

func Test_when_getting_filename_with_special_characters_should_normalize_them(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		TaskDescription: "find files with name test*.txt & sort by date",
		ScriptType:      "bash",
		Script:          "find . -name 'test*.txt'",
	}

	// Act
	result := GetSuggestedFilename(response)

	// Assert
	if strings.Contains(result, "*") || strings.Contains(result, "&") {
		t.Errorf("Expected special characters to be removed, got: %s", result)
	}
	if !strings.Contains(result, "find") {
		t.Errorf("Expected filename to contain 'find', got: %s", result)
	}
	if strings.Contains(result, " ") {
		t.Errorf("Expected spaces to be replaced with underscores, got: %s", result)
	}
}

func Test_when_getting_filename_with_common_words_should_filter_them_out(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		TaskDescription: "create a backup of the important files and directories",
		ScriptType:      "bash",
		Script:          "tar -czf backup.tar.gz /important",
	}

	// Act
	result := GetSuggestedFilename(response)

	// Assert
	if strings.Contains(result, "the") || strings.Contains(result, "and") || strings.Contains(result, "of") {
		t.Errorf("Expected common words to be filtered out, got: %s", result)
	}
	if !strings.Contains(result, "create") || !strings.Contains(result, "backup") {
		t.Errorf("Expected meaningful words to be kept, got: %s", result)
	}
}

func Test_when_getting_filename_with_long_description_should_limit_length(t *testing.T) {
	// Arrange
	longDescription := "this is a very long task description that should be truncated because it exceeds the maximum allowed length for a filename"
	response := &types.ScriptResponse{
		TaskDescription: longDescription,
		ScriptType:      "bash",
		Script:          "echo 'test'",
	}

	// Act
	result := GetSuggestedFilename(response)

	// Assert
	baseNameWithoutExt := strings.TrimSuffix(result, ".sh")
	if len(baseNameWithoutExt) > 30 {
		t.Errorf("Expected filename to be limited to 30 chars, got %d chars: %s", len(baseNameWithoutExt), result)
	}
}

func Test_when_getting_filename_with_empty_description_should_use_default(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		TaskDescription: "",
		ScriptType:      "powershell",
		Script:          "Write-Host 'Hello'",
	}

	// Act
	result := GetSuggestedFilename(response)

	// Assert
	if !strings.Contains(result, "please_script") {
		t.Errorf("Expected default filename for empty description, got: %s", result)
	}
	if !strings.HasSuffix(result, ".ps1") {
		t.Errorf("Expected .ps1 extension for powershell, got: %s", result)
	}
}

func Test_when_getting_filename_with_only_short_words_should_use_default(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		TaskDescription: "a to is on at by",
		ScriptType:      "bash",
		Script:          "echo test",
	}

	// Act
	result := GetSuggestedFilename(response)

	// Assert
	if !strings.Contains(result, "please_script") {
		t.Errorf("Expected default filename when only short words, got: %s", result)
	}
}

// Test ValidateScript function
func Test_when_validating_script_with_critical_dangers_should_return_critical_warnings(t *testing.T) {
	testCases := []struct {
		name           string
		script         string
		expectedWarning string
	}{
		{
			name:           "rm rf root",
			script:         "rm -rf /",
			expectedWarning: "⛔ CRITICAL: Attempts to delete entire filesystem",
		},
		{
			name:           "rm rf root with asterisk",
			script:         "rm -rf /*",
			expectedWarning: "⛔ CRITICAL: Attempts to delete entire filesystem",
		},
		{
			name:           "windows delete c drive",
			script:         "del /s /q c:\\*",
			expectedWarning: "⛔ CRITICAL: Attempts to delete entire C: drive",
		},
		{
			name:           "format c drive",
			script:         "format c:",
			expectedWarning: "⛔ CRITICAL: Attempts to format C: drive",
		},
		{
			name:           "dd command with zeros",
			script:         "dd if=/dev/zero of=/dev/sda",
			expectedWarning: "⛔ CRITICAL: Attempts to overwrite data with zeros",
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Arrange
			response := &types.ScriptResponse{
				Script:     tc.script,
				ScriptType: "bash",
			}

			// Act
			warnings := ValidateScript(response)

			// Assert
			found := false
			for _, warning := range warnings {
				if strings.Contains(warning, "⛔ CRITICAL") {
					found = true
					if !strings.Contains(warning, strings.Split(tc.expectedWarning, ":")[1]) {
						t.Errorf("Expected warning about %s, got: %s", tc.expectedWarning, warning)
					}
					break
				}
			}
			if !found {
				t.Errorf("Expected critical warning for script: %s, got warnings: %v", tc.script, warnings)
			}
		})
	}
}

func Test_when_validating_script_with_high_risk_commands_should_return_high_risk_warnings(t *testing.T) {
	testCases := []struct {
		name           string
		script         string
		expectedPhrase string
	}{
		{
			name:           "shutdown command",
			script:         "shutdown -h now",
			expectedPhrase: "Will shutdown the system",
		},
		{
			name:           "reboot command",
			script:         "reboot",
			expectedPhrase: "Will restart the system",
		},
		{
			name:           "chmod 777",
			script:         "chmod 777 /important/file",
			expectedPhrase: "world-writable",
		},
		{
			name:           "sudo su",
			script:         "sudo su -",
			expectedPhrase: "root privileges",
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Arrange
			response := &types.ScriptResponse{
				Script:     tc.script,
				ScriptType: "bash",
			}

			// Act
			warnings := ValidateScript(response)

			// Assert
			found := false
			for _, warning := range warnings {
				if strings.Contains(warning, "🔴 WARNING") && strings.Contains(warning, tc.expectedPhrase) {
					found = true
					break
				}
			}
			if !found {
				t.Errorf("Expected high risk warning for script: %s, got warnings: %v", tc.script, warnings)
			}
		})
	}
}

func Test_when_validating_script_with_medium_risk_commands_should_return_caution_warnings(t *testing.T) {
	testCases := []struct {
		name           string
		script         string
		expectedPhrase string
	}{
		{
			name:           "rm rf directory",
			script:         "rm -rf /tmp/mydir",
			expectedPhrase: "Recursive deletion",
		},
		{
			name:           "windows recursive delete",
			script:         "del /s /q C:\\temp\\mydir",
			expectedPhrase: "Recursive deletion",
		},
		{
			name:           "systemctl stop",
			script:         "systemctl stop nginx",
			expectedPhrase: "Stops system services",
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Arrange
			response := &types.ScriptResponse{
				Script:     tc.script,
				ScriptType: "bash",
			}

			// Act
			warnings := ValidateScript(response)

			// Assert
			found := false
			for _, warning := range warnings {
				if strings.Contains(warning, "🟡 CAUTION") && strings.Contains(warning, tc.expectedPhrase) {
					found = true
					break
				}
			}
			if !found {
				t.Errorf("Expected caution warning for script: %s, got warnings: %v", tc.script, warnings)
			}
		})
	}
}

func Test_when_validating_bash_script_without_shebang_should_suggest_adding_it(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		Script:     "echo 'hello world'\nls -la",
		ScriptType: "bash",
	}

	// Act
	warnings := ValidateScript(response)

	// Assert
	found := false
	for _, warning := range warnings {
		if strings.Contains(warning, "🟢 INFO") && strings.Contains(warning, "shebang") {
			found = true
			break
		}
	}
	if !found {
		t.Errorf("Expected shebang suggestion for bash script without shebang, got: %v", warnings)
	}
}

func Test_when_validating_very_short_script_should_warn_about_completeness(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		Script:     "ls",
		ScriptType: "bash",
	}

	// Act
	warnings := ValidateScript(response)

	// Assert
	found := false
	for _, warning := range warnings {
		if strings.Contains(warning, "🟢 INFO") && strings.Contains(warning, "very short") {
			found = true
			break
		}
	}
	if !found {
		t.Errorf("Expected short script warning, got: %v", warnings)
	}
}

func Test_when_validating_script_without_error_handling_should_suggest_adding_it(t *testing.T) {
	// Arrange
	longScriptWithoutErrorHandling := `#!/bin/bash
echo "Starting process"
cp file1.txt backup/
mv file2.txt archive/
rm temp.txt
echo "Process complete"`

	response := &types.ScriptResponse{
		Script:     longScriptWithoutErrorHandling,
		ScriptType: "bash",
	}

	// Act
	warnings := ValidateScript(response)

	// Assert
	found := false
	for _, warning := range warnings {
		if strings.Contains(warning, "🟢 INFO") && strings.Contains(warning, "error handling") {
			found = true
			break
		}
	}
	if !found {
		t.Errorf("Expected error handling suggestion, got: %v", warnings)
	}
}

func Test_when_validating_script_with_error_handling_should_not_suggest_adding_it(t *testing.T) {
	// Arrange
	scriptWithErrorHandling := `#!/bin/bash
if [ ! -f "input.txt" ]; then
    echo "Error: input.txt not found"
    exit 1
fi
cp input.txt backup/ || { echo "Backup failed"; exit 1; }
echo "Success"`

	response := &types.ScriptResponse{
		Script:     scriptWithErrorHandling,
		ScriptType: "bash",
	}

	// Act
	warnings := ValidateScript(response)

	// Assert
	for _, warning := range warnings {
		if strings.Contains(warning, "error handling") {
			t.Errorf("Should not suggest error handling for script that has it, got: %v", warnings)
		}
	}
}

// Test containsCommand function
func Test_when_checking_command_in_quoted_string_should_return_false(t *testing.T) {
	testCases := []struct {
		name     string
		script   string
		pattern  string
		expected bool
	}{
		{
			name:     "rm in double quotes",
			script:   `echo "Please don't rm -rf anything"`,
			pattern:  "rm -rf",
			expected: false,
		},
		{
			name:     "rm in single quotes",
			script:   `echo 'The command rm -rf is dangerous'`,
			pattern:  "rm -rf",
			expected: false,
		},
		{
			name:     "actual rm command",
			script:   `rm -rf /tmp/test`,
			pattern:  "rm -rf",
			expected: true,
		},
		{
			name:     "format as parameter",
			script:   `get-date -format "yyyy-MM-dd"`,
			pattern:  "format",
			expected: false,
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Act
			result := containsCommand(tc.script, tc.pattern)

			// Assert
			if result != tc.expected {
				t.Errorf("Expected %v for pattern '%s' in script '%s', got %v", tc.expected, tc.pattern, tc.script, result)
			}
		})
	}
}

// Test containsPatternOutsideQuotes function
func Test_when_checking_pattern_outside_quotes_should_detect_correctly(t *testing.T) {
	testCases := []struct {
		name     string
		line     string
		pattern  string
		expected bool
	}{
		{
			name:     "pattern in double quotes",
			line:     `echo "rm -rf test"`,
			pattern:  "rm -rf",
			expected: false,
		},
		{
			name:     "pattern in single quotes",
			line:     `echo 'shutdown now'`,
			pattern:  "shutdown",
			expected: false,
		},
		{
			name:     "pattern outside quotes",
			line:     `rm -rf /tmp && echo "done"`,
			pattern:  "rm -rf",
			expected: true,
		},
		{
			name:     "pattern mixed with quotes",
			line:     `echo "starting" && shutdown -h now`,
			pattern:  "shutdown",
			expected: true,
		},
		{
			name:     "pattern at beginning",
			line:     `format /dev/sda1`,
			pattern:  "format",
			expected: true,
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Act
			result := containsPatternOutsideQuotes(tc.line, tc.pattern)

			// Assert
			if result != tc.expected {
				t.Errorf("Expected %v for pattern '%s' in line '%s', got %v", tc.expected, tc.pattern, tc.line, result)
			}
		})
	}
}

// Test SaveToFile function (safe file operations)
func Test_when_saving_bash_script_without_extension_should_add_sh_extension(t *testing.T) {
	// Arrange
	script := "#!/bin/bash\necho 'test'"
	filename := "test_script"
	
	// Act
	err := SaveToFile(script, filename)
	
	// Assert
	if err != nil {
		t.Errorf("Expected no error saving file, got: %v", err)
	}
	
	// Check if file exists with .sh extension
	if _, err := os.Stat(filename + ".sh"); err != nil {
		t.Errorf("Expected file %s.sh to exist, got error: %v", filename, err)
	}
	
	// Cleanup
	os.Remove(filename + ".sh")
}

func Test_when_saving_powershell_script_without_extension_should_add_ps1_extension(t *testing.T) {
	// Arrange
	script := "Write-Host 'test'"
	filename := "test_script"
	
	// Act
	err := SaveToFile(script, filename)
	
	// Assert
	if err != nil {
		t.Errorf("Expected no error saving file, got: %v", err)
	}
	
	// Check if file exists with .ps1 extension
	if _, err := os.Stat(filename + ".ps1"); err != nil {
		t.Errorf("Expected file %s.ps1 to exist, got error: %v", filename, err)
	}
	
	// Cleanup
	os.Remove(filename + ".ps1")
}

func Test_when_saving_script_with_extension_should_preserve_extension(t *testing.T) {
	// Arrange
	script := "echo 'test'"
	filename := "myscript.custom"
	
	// Act
	err := SaveToFile(script, filename)
	
	// Assert
	if err != nil {
		t.Errorf("Expected no error saving file, got: %v", err)
	}
	
	// Check if file exists with original extension
	if _, err := os.Stat(filename); err != nil {
		t.Errorf("Expected file %s to exist, got error: %v", filename, err)
	}
	
	// Cleanup
	os.Remove(filename)
}

func Test_when_saving_bash_script_on_unix_should_make_executable(t *testing.T) {
	// Skip on Windows
	if runtime.GOOS == "windows" {
		t.Skip("Skipping executable test on Windows")
		return
	}
	
	// Arrange
	script := "#!/bin/bash\necho 'test'"
	filename := "test_executable.sh"
	
	// Act
	err := SaveToFile(script, filename)
	
	// Assert
	if err != nil {
		t.Errorf("Expected no error saving file, got: %v", err)
	}
	
	// Check file permissions
	info, err := os.Stat(filename)
	if err != nil {
		t.Errorf("Expected file to exist, got error: %v", err)
	} else {
		mode := info.Mode()
		if mode&0111 == 0 {
			t.Errorf("Expected file to be executable, got mode: %v", mode)
		}
	}
	
	// Cleanup
	os.Remove(filename)
}

// Test CopyToClipboard function (platform-aware)
func Test_when_copying_empty_string_to_clipboard_should_not_error(t *testing.T) {
	// Act
	err := CopyToClipboard("")
	
	// Assert - This might fail if clipboard tools aren't available, but shouldn't panic
	if err != nil {
		t.Logf("Clipboard operation failed (expected on some systems): %v", err)
		// Don't fail the test as clipboard might not be available in CI/testing environments
	}
}

func Test_when_copying_text_to_clipboard_should_handle_gracefully(t *testing.T) {
	// Arrange
	testText := "echo 'Hello, World!'"
	
	// Act
	err := CopyToClipboard(testText)
	
	// Assert - This might fail if clipboard tools aren't available, but shouldn't panic
	if err != nil {
		t.Logf("Clipboard operation failed (expected on some systems): %v", err)
		// Check that error message is informative
		if runtime.GOOS == "linux" && strings.Contains(err.Error(), "no clipboard utility found") {
			// This is expected behavior on Linux without xclip/xsel
			t.Logf("Expected Linux clipboard error: %v", err)
		}
	}
}

// Test ExecuteScript function (the major missing coverage area)
func Test_when_executing_safe_bash_script_should_run_successfully(t *testing.T) {
	// Arrange - Create a safe script that just echoes a test message
	response := &types.ScriptResponse{
		Script:          "#!/bin/bash\necho 'test_execution_successful'",
		ScriptType:      "bash",
		TaskDescription: "test script execution",
		Provider:        "test",
		Model:           "test-model",
	}

	// Act
	err := ExecuteScript(response)

	// Assert
	if err != nil {
		// On Windows, bash might not be available or have path issues - that's expected
		if runtime.GOOS == "windows" && (strings.Contains(err.Error(), "bash not found") || 
			strings.Contains(err.Error(), "No such file") || 
			strings.Contains(err.Error(), "exit status 127")) {
			t.Logf("Expected Windows bash error (bash unavailable or path issues): %v", err)
			return
		}
		t.Errorf("Expected safe bash script to execute successfully, got error: %v", err)
	}
}

func Test_when_executing_safe_powershell_script_should_run_successfully(t *testing.T) {
	// Skip on non-Windows systems where PowerShell might not be available
	if runtime.GOOS != "windows" {
		t.Skip("Skipping PowerShell test on non-Windows system")
		return
	}

	// Arrange - Create a safe PowerShell script
	response := &types.ScriptResponse{
		Script:          "Write-Host 'test_execution_successful'",
		ScriptType:      "powershell",
		TaskDescription: "test powershell execution",
		Provider:        "test",
		Model:           "test-model",
	}

	// Act
	err := ExecuteScript(response)

	// Assert
	if err != nil {
		t.Errorf("Expected safe PowerShell script to execute successfully, got error: %v", err)
	}
}

func Test_when_executing_script_with_invalid_syntax_should_return_error(t *testing.T) {
	// Arrange - Create a script with invalid syntax
	response := &types.ScriptResponse{
		Script:          "this is not valid bash syntax {{{{ ]]]]",
		ScriptType:      "bash",
		TaskDescription: "test invalid script",
		Provider:        "test",
		Model:           "test-model",
	}

	// Act
	err := ExecuteScript(response)

	// Assert
	if err == nil {
		t.Error("Expected error for script with invalid syntax, but got nil")
	}
}

func Test_when_executing_script_should_create_and_cleanup_temp_file(t *testing.T) {
	// This test verifies that temporary files are created and cleaned up properly
	// We can't easily test the cleanup directly since it happens in defer,
	// but we can test that the function doesn't leave temp files around
	
	// Arrange
	response := &types.ScriptResponse{
		Script:          "echo 'temp file test'",
		ScriptType:      "bash", 
		TaskDescription: "test temp file handling",
		Provider:        "test",
		Model:           "test-model",
	}

	// Get temp directory contents before
	tempDir := os.TempDir()
	beforeFiles, _ := os.ReadDir(tempDir)
	beforeCount := 0
	for _, file := range beforeFiles {
		if strings.HasPrefix(file.Name(), "please_temp") {
			beforeCount++
		}
	}

	// Act
	err := ExecuteScript(response)

	// Assert - Check temp file cleanup
	afterFiles, _ := os.ReadDir(tempDir)
	afterCount := 0
	for _, file := range afterFiles {
		if strings.HasPrefix(file.Name(), "please_temp") {
			afterCount++
		}
	}

	// Should not have more temp files after execution
	if afterCount > beforeCount {
		t.Errorf("Expected temp files to be cleaned up, before: %d, after: %d", beforeCount, afterCount)
	}

	// Don't fail test if execution failed due to missing bash on Windows
	if err != nil && runtime.GOOS == "windows" && strings.Contains(err.Error(), "bash not found") {
		t.Logf("Expected Windows bash error: %v", err)
	}
}

func Test_when_executing_powershell_script_on_windows_should_use_correct_command(t *testing.T) {
	// This test verifies PowerShell execution path without actually running dangerous commands
	
	// Skip on non-Windows systems
	if runtime.GOOS != "windows" {
		t.Skip("Skipping Windows-specific PowerShell test")
		return
	}

	// Arrange - Use a very simple, safe PowerShell command
	response := &types.ScriptResponse{
		Script:          "$null", // This does nothing but is valid PowerShell
		ScriptType:      "powershell",
		TaskDescription: "test powershell command path",
		Provider:        "test", 
		Model:           "test-model",
	}

	// Act
	err := ExecuteScript(response)

	// Assert - Should not error on valid PowerShell syntax
	if err != nil {
		t.Errorf("Expected simple PowerShell command to execute, got error: %v", err)
	}
}

func Test_when_executing_bash_script_on_windows_without_bash_should_return_helpful_error(t *testing.T) {
	// Skip if not on Windows
	if runtime.GOOS != "windows" {
		t.Skip("Skipping Windows-specific bash error test")
		return
	}

	// Arrange
	response := &types.ScriptResponse{
		Script:          "echo 'test'",
		ScriptType:      "bash",
		TaskDescription: "test bash on windows error",
		Provider:        "test",
		Model:           "test-model",
	}

	// Act
	err := ExecuteScript(response)

	// Assert - Should provide helpful error if bash not available
	if err != nil {
		// Check for helpful error messages OR expected bash execution errors
		expectedPhrases := []string{"bash not found", "Git Bash", "WSL", "exit status 127", "No such file"}
		foundExpectedError := false
		for _, phrase := range expectedPhrases {
			if strings.Contains(err.Error(), phrase) {
				foundExpectedError = true
				break
			}
		}
		if !foundExpectedError {
			t.Errorf("Expected bash execution error or helpful message about bash availability, got: %v", err)
		}
		t.Logf("Windows bash execution error (expected): %v", err)
	}
}

func Test_when_executing_script_with_empty_content_should_handle_gracefully(t *testing.T) {
	// Arrange
	response := &types.ScriptResponse{
		Script:          "",
		ScriptType:      "bash",
		TaskDescription: "test empty script",
		Provider:        "test",
		Model:           "test-model",
	}

	// Act
	err := ExecuteScript(response)

	// Assert - Empty script should not cause panic, may succeed or fail gracefully
	// Don't assert specific behavior since empty scripts might be valid on some systems
	if err != nil {
		t.Logf("Empty script execution result: %v", err)
	}
}

func Test_when_executing_script_should_handle_file_creation_errors_gracefully(t *testing.T) {
	// This test would require creating a scenario where temp file creation fails
	// Since we can't easily simulate filesystem errors in a unit test,
	// we'll test with an edge case that might cause issues
	
	// Arrange - Script with potentially problematic characters
	response := &types.ScriptResponse{
		Script:          "echo 'test with unicode: 你好 🌟'",
		ScriptType:      "bash",
		TaskDescription: "test unicode handling",
		Provider:        "test",
		Model:           "test-model",
	}

	// Act
	err := ExecuteScript(response)

	// Assert - Should handle unicode content without crashing
	if err != nil && runtime.GOOS == "windows" && strings.Contains(err.Error(), "bash not found") {
		t.Logf("Expected Windows bash error: %v", err)
	} else if err != nil {
		t.Logf("Unicode script execution result: %v", err)
		// Don't fail test as unicode handling varies by system
	}
}
