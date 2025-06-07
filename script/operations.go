package script

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"

	"oohlama/types"
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
		tempFile = filepath.Join(tempDir, "oohlama_temp.ps1")
		
		// Save script to temp file
		if err := SaveToFile(response.Script, tempFile); err != nil {
			return fmt.Errorf("failed to create temporary script file: %v", err)
		}
		defer os.Remove(tempFile) // Clean up

		// Execute with PowerShell
		cmd = exec.Command("powershell", "-ExecutionPolicy", "Bypass", "-File", tempFile)
	} else {
		tempFile = filepath.Join(tempDir, "oohlama_temp.sh")
		
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
		baseName = "oohlama_script"
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

// ValidateScript performs basic validation on the generated script
func ValidateScript(response *types.ScriptResponse) []string {
	warnings := []string{}
	script := response.Script
	
	// Check for potentially dangerous commands
	dangerousCommands := []string{
		"rm -rf /", "del /", "format", "mkfs", "dd if=", 
		"shutdown", "reboot", "halt", "init 0", "init 6",
		"chmod 777", "chown root", "sudo su", "su -",
	}
	
	for _, dangerous := range dangerousCommands {
		if strings.Contains(strings.ToLower(script), strings.ToLower(dangerous)) {
			warnings = append(warnings, fmt.Sprintf("⚠️  Contains potentially dangerous command: %s", dangerous))
		}
	}
	
	// Check for missing shebangs in bash scripts
	if response.ScriptType == "bash" && !strings.HasPrefix(script, "#!") {
		warnings = append(warnings, "💡 Consider adding a shebang line (#!/bin/bash) at the top")
	}
	
	// Check for very short scripts (might be incomplete)
	if len(strings.TrimSpace(script)) < 20 {
		warnings = append(warnings, "🤔 Script seems very short - it might be incomplete")
	}
	
	return warnings
}
