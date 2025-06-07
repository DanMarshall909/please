package script

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"

	"please/types"
)

// CopyToClipboard copies the script content to the system clipboard
func CopyToClipboard(script string) error {
	var cmd *exec.Cmd

	switch runtime.GOOS {
	case "windows":
		cmd = exec.Command("cmd", "/c", "clip")
	case "darwin":
		cmd = exec.Command("pbcopy")
	case "linux":
		// Try xclip first, then xsel as fallback
		if _, err := exec.LookPath("xclip"); err == nil {
			cmd = exec.Command("xclip", "-selection", "clipboard")
		} else if _, err := exec.LookPath("xsel"); err == nil {
			cmd = exec.Command("xsel", "--clipboard", "--input")
		} else {
			return fmt.Errorf("no clipboard utility found (install xclip or xsel)")
		}
	default:
		return fmt.Errorf("clipboard not supported on %s", runtime.GOOS)
	}

	cmd.Stdin = strings.NewReader(script)
	return cmd.Run()
}

// SaveToFile saves the script to a file with the given filename
func SaveToFile(script, filename string) error {
	// Ensure the filename has the correct extension
	if !strings.Contains(filename, ".") {
		if strings.Contains(script, "#!/bin/bash") || strings.Contains(script, "#!/bin/sh") {
			filename += ".sh"
		} else {
			filename += ".ps1"
		}
	}

	// Create the file
	file, err := os.Create(filename)
	if err != nil {
		return fmt.Errorf("failed to create file: %v", err)
	}
	defer file.Close()

	// Write the script content
	if _, err := file.WriteString(script); err != nil {
		return fmt.Errorf("failed to write script to file: %v", err)
	}

	// Make executable on Unix systems
	if runtime.GOOS != "windows" && (strings.HasSuffix(filename, ".sh") || strings.Contains(script, "#!/bin/")) {
		if err := os.Chmod(filename, 0755); err != nil {
			return fmt.Errorf("failed to make script executable: %v", err)
		}
	}

	return nil
}

// ExecuteScript executes the script safely with platform-appropriate commands
func ExecuteScript(response *types.ScriptResponse) error {
	// Create a temporary file for the script
	tempDir := os.TempDir()
	var tempFile string
	var cmd *exec.Cmd

	if response.ScriptType == "powershell" {
		tempFile = filepath.Join(tempDir, "please_temp.ps1")
		
		// Save script to temp file
		if err := SaveToFile(response.Script, tempFile); err != nil {
			return fmt.Errorf("failed to create temporary script file: %v", err)
		}
		defer os.Remove(tempFile) // Clean up

		// Execute with PowerShell
		cmd = exec.Command("powershell", "-ExecutionPolicy", "Bypass", "-File", tempFile)
	} else {
		tempFile = filepath.Join(tempDir, "please_temp.sh")
		
		// Save script to temp file
		if err := SaveToFile(response.Script, tempFile); err != nil {
			return fmt.Errorf("failed to create temporary script file: %v", err)
		}
		defer os.Remove(tempFile) // Clean up

		// Execute with bash/sh
		if runtime.GOOS == "windows" {
			// On Windows, try bash from Git Bash or WSL
			if _, err := exec.LookPath("bash"); err == nil {
				cmd = exec.Command("bash", tempFile)
			} else {
				return fmt.Errorf("bash not found on Windows - install Git Bash or WSL")
			}
		} else {
			cmd = exec.Command("bash", tempFile)
		}
	}

	// Set up command to show output in real-time
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Stdin = os.Stdin

	// Execute the command
	return cmd.Run()
}

// GetSuggestedFilename returns a suggested filename based on the task and script type
func GetSuggestedFilename(response *types.ScriptResponse) string {
	// Create a base name from the task description
	baseName := strings.ToLower(response.TaskDescription)
	
	// Remove special characters and replace spaces with underscores
	baseName = strings.ReplaceAll(baseName, " ", "_")
	baseName = strings.ReplaceAll(baseName, "-", "_")
	
	// Remove common words and limit length
	words := strings.Fields(strings.ReplaceAll(baseName, "_", " "))
	filteredWords := []string{}
	commonWords := map[string]bool{
		"a": true, "an": true, "the": true, "and": true, "or": true,
		"but": true, "in": true, "on": true, "at": true, "to": true,
		"for": true, "of": true, "with": true, "by": true, "from": true,
		"up": true, "about": true, "into": true, "through": true, "during": true,
		"before": true, "after": true, "above": true, "below": true, "between": true,
		"is": true, "are": true, "was": true, "were": true, "be": true,
		"have": true, "has": true, "had": true, "do": true, "does": true, "did": true,
		"will": true, "would": true, "could": true, "should": true, "may": true, "might": true,
	}
	
	for _, word := range words {
		if !commonWords[word] && len(word) > 2 {
			filteredWords = append(filteredWords, word)
		}
		if len(filteredWords) >= 3 { // Limit to 3 meaningful words
			break
		}
	}
	
	if len(filteredWords) == 0 {
		baseName = "please_script"
	} else {
		baseName = strings.Join(filteredWords, "_")
	}
	
	// Limit total length
	if len(baseName) > 30 {
		baseName = baseName[:30]
	}
	
	// Add appropriate extension
	if response.ScriptType == "powershell" {
		return baseName + ".ps1"
	} else {
		return baseName + ".sh"
	}
}

// ValidateScript performs intelligent validation on the generated script with severity levels
func ValidateScript(response *types.ScriptResponse) []string {
	warnings := []string{}
	script := strings.ToLower(response.Script)
	lines := strings.Split(response.Script, "\n")
	
	// Critical dangers - These will definitely cause problems
	criticalPatterns := map[string]string{
		`rm -rf /`:        "⛔ CRITICAL: Attempts to delete entire filesystem",
		`rm -rf /*`:       "⛔ CRITICAL: Attempts to delete entire filesystem", 
		`del /s /q c:\*`:  "⛔ CRITICAL: Attempts to delete entire C: drive",
		`format c:`:       "⛔ CRITICAL: Attempts to format C: drive",
		`format /dev/`:    "⛔ CRITICAL: Attempts to format system devices",
		`dd if=/dev/zero`: "⛔ CRITICAL: Attempts to overwrite data with zeros",
		`mkfs`:           "⛔ CRITICAL: Attempts to create new filesystem (destroys data)",
	}
	
	// High risk warnings - Potentially dangerous but context matters
	highRiskPatterns := map[string]string{
		`shutdown`:     "🔴 WARNING: Will shutdown the system",
		`reboot`:       "🔴 WARNING: Will restart the system", 
		`halt`:         "🔴 WARNING: Will halt the system",
		`init 0`:       "🔴 WARNING: Will shutdown the system",
		`init 6`:       "🔴 WARNING: Will restart the system",
		`sudo su`:      "🔴 WARNING: Escalates to root privileges",
		`chmod 777`:    "🔴 WARNING: Makes files world-writable (security risk)",
		`chown root`:   "🔴 WARNING: Changes ownership to root",
	}
	
	// Medium risk - Things to be cautious about
	mediumRiskPatterns := map[string]string{
		`rm -rf`:       "🟡 CAUTION: Recursive deletion - verify target path carefully",
		`del /s /q`:    "🟡 CAUTION: Recursive deletion - verify target path carefully",
		`crontab -r`:   "🟡 CAUTION: Removes all cron jobs",
		`systemctl stop`: "🟡 CAUTION: Stops system services",
		`service stop`: "🟡 CAUTION: Stops system services",
	}
	
	// Check critical patterns first
	for pattern, warning := range criticalPatterns {
		if containsCommand(script, pattern) {
			warnings = append(warnings, warning)
		}
	}
	
	// Check high risk patterns
	for pattern, warning := range highRiskPatterns {
		if containsCommand(script, pattern) {
			warnings = append(warnings, warning)
		}
	}
	
	// Check medium risk patterns
	for pattern, warning := range mediumRiskPatterns {
		if containsCommand(script, pattern) {
			warnings = append(warnings, warning)
		}
	}
	
	// Info level checks
	if response.ScriptType == "bash" && !strings.HasPrefix(response.Script, "#!") {
		warnings = append(warnings, "🟢 INFO: Consider adding a shebang line (#!/bin/bash) at the top")
	}
	
	// Check for very short scripts (might be incomplete)
	if len(strings.TrimSpace(response.Script)) < 20 {
		warnings = append(warnings, "🟢 INFO: Script seems very short - it might be incomplete")
	}
	
	// Check for scripts with no error handling
	hasErrorHandling := false
	for _, line := range lines {
		lowerLine := strings.TrimSpace(strings.ToLower(line))
		if strings.Contains(lowerLine, "try {") || 
		   strings.Contains(lowerLine, "catch") ||
		   strings.Contains(lowerLine, "trap") ||
		   strings.Contains(lowerLine, "|| ") ||
		   strings.Contains(lowerLine, "&& ") ||
		   strings.Contains(lowerLine, "if [ $? -") {
			hasErrorHandling = true
			break
		}
	}
	
	if !hasErrorHandling && len(lines) > 5 {
		warnings = append(warnings, "🟢 INFO: Script has no error handling - consider adding try/catch or error checks")
	}
	
	return warnings
}

// containsCommand checks if a command appears as an actual command, not as part of a parameter or string
func containsCommand(script, pattern string) bool {
	// Skip if it's clearly a parameter (preceded by -)
	if strings.Contains(script, "-"+pattern) {
		return false
	}
	
	// Skip PowerShell format parameters
	if strings.Contains(script, "-format") && pattern == "format" {
		return false
	}
	
	// More sophisticated quoted string detection
	lines := strings.Split(script, "\n")
	for _, line := range lines {
		line = strings.TrimSpace(line)
		
		// Skip empty lines and comments
		if line == "" || strings.HasPrefix(line, "#") || strings.HasPrefix(line, "//") {
			continue
		}
		
		// Check if the pattern appears outside of quotes
		if containsPatternOutsideQuotes(line, pattern) {
			return true
		}
	}
	
	return false
}

// containsPatternOutsideQuotes checks if a pattern appears outside of quoted strings
func containsPatternOutsideQuotes(line, pattern string) bool {
	inDoubleQuotes := false
	inSingleQuotes := false
	
	for i := 0; i < len(line)-len(pattern)+1; i++ {
		char := line[i]
		
		// Toggle quote states
		if char == '"' && !inSingleQuotes {
			inDoubleQuotes = !inDoubleQuotes
			continue
		}
		if char == '\'' && !inDoubleQuotes {
			inSingleQuotes = !inSingleQuotes
			continue
		}
		
		// If we're not in quotes, check for pattern match
		if !inDoubleQuotes && !inSingleQuotes {
			if i+len(pattern) <= len(line) && strings.ToLower(line[i:i+len(pattern)]) == pattern {
				return true
			}
		}
	}
	
	return false
}
