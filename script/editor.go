package script

import (
	"bufio"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"time"

	"please/types"
)

// EditScript allows the user to edit a script using their preferred editor
func EditScript(response *types.ScriptResponse) (*types.ScriptResponse, error) {
	// Create a temporary file for editing
	tempDir := os.TempDir()
	timestamp := fmt.Sprintf("%d", time.Now().Unix())
	
	var tempFile string
	if response.ScriptType == "powershell" {
		tempFile = filepath.Join(tempDir, fmt.Sprintf("please_edit_%s.ps1", timestamp))
	} else {
		tempFile = filepath.Join(tempDir, fmt.Sprintf("please_edit_%s.sh", timestamp))
	}

	// Write the current script to the temporary file
	if err := os.WriteFile(tempFile, []byte(response.Script), 0644); err != nil {
		return nil, fmt.Errorf("failed to create temporary file: %v", err)
	}

	// Ensure cleanup
	defer func() {
		os.Remove(tempFile)
	}()

	// Detect and launch the appropriate editor
	editor, err := detectEditor()
	if err != nil {
		return response, fmt.Errorf("no suitable editor found: %v", err)
	}

	fmt.Printf("🔧 Opening script in %s...\n", editor.Name)
	fmt.Printf("💡 Save and close the editor when you're done editing\n\n")

	// Launch the editor
	if err := launchEditor(editor, tempFile); err != nil {
		return response, fmt.Errorf("failed to launch editor: %v", err)
	}

	// Read the modified script
	modifiedContent, err := os.ReadFile(tempFile)
	if err != nil {
		return response, fmt.Errorf("failed to read modified script: %v", err)
	}

	modifiedScript := string(modifiedContent)

	// Check if the script was actually modified
	if modifiedScript == response.Script {
		fmt.Printf("📝 No changes detected in the script\n")
		return response, nil
	}

	// Create a new response with the modified script
	editedResponse := &types.ScriptResponse{
		TaskDescription: response.TaskDescription + " (edited)",
		Script:         modifiedScript,
		ScriptType:     response.ScriptType,
		Model:          response.Model,
		Provider:       response.Provider,
	}

	// Show the changes
	showScriptChanges(response.Script, modifiedScript)

	// Re-validate the modified script
	fmt.Printf("🔍 Re-validating modified script...\n")
	warnings := ValidateScript(editedResponse)
	if len(warnings) > 0 {
		fmt.Printf("⚠️  Modified script has warnings:\n")
		for _, warning := range warnings {
			fmt.Printf("  %s\n", warning)
		}
	} else {
		fmt.Printf("✅ Modified script looks safe!\n")
	}

	fmt.Printf("\n✨ Script editing completed!\n")
	return editedResponse, nil
}

// EditorInfo contains information about an available editor
type EditorInfo struct {
	Name        string
	Command     string
	Args        []string
	Description string
}

// detectEditor finds the best available editor on the system
func detectEditor() (*EditorInfo, error) {
	// Check for user-configured editor in environment
	if editorEnv := os.Getenv("EDITOR"); editorEnv != "" {
		if isCommandAvailable(editorEnv) {
			return &EditorInfo{
				Name:        filepath.Base(editorEnv),
				Command:     editorEnv,
				Args:        []string{},
				Description: "User configured editor",
			}, nil
		}
	}

	// Platform-specific editor detection
	editors := getAvailableEditors()
	
	for _, editor := range editors {
		if isCommandAvailable(editor.Command) {
			return &editor, nil
		}
	}

	return nil, fmt.Errorf("no suitable editor found")
}

// getAvailableEditors returns a list of editors to try, in order of preference
func getAvailableEditors() []EditorInfo {
	switch runtime.GOOS {
	case "windows":
		return []EditorInfo{
			{
				Name:        "VS Code",
				Command:     "code",
				Args:        []string{"--wait"},
				Description: "Visual Studio Code",
			},
			{
				Name:        "Notepad++",
				Command:     "notepad++",
				Args:        []string{},
				Description: "Notepad++",
			},
			{
				Name:        "Notepad",
				Command:     "notepad",
				Args:        []string{},
				Description: "Windows Notepad",
			},
		}
	case "darwin":
		return []EditorInfo{
			{
				Name:        "VS Code",
				Command:     "code",
				Args:        []string{"--wait"},
				Description: "Visual Studio Code",
			},
			{
				Name:        "nano",
				Command:     "nano",
				Args:        []string{},
				Description: "nano text editor",
			},
			{
				Name:        "vim",
				Command:     "vim",
				Args:        []string{},
				Description: "Vim editor",
			},
			{
				Name:        "TextEdit",
				Command:     "open",
				Args:        []string{"-a", "TextEdit", "-W"},
				Description: "macOS TextEdit",
			},
		}
	default: // Linux and others
		return []EditorInfo{
			{
				Name:        "VS Code",
				Command:     "code",
				Args:        []string{"--wait"},
				Description: "Visual Studio Code",
			},
			{
				Name:        "nano",
				Command:     "nano",
				Args:        []string{},
				Description: "nano text editor",
			},
			{
				Name:        "vim",
				Command:     "vim",
				Args:        []string{},
				Description: "Vim editor",
			},
			{
				Name:        "gedit",
				Command:     "gedit",
				Args:        []string{},
				Description: "GNOME Text Editor",
			},
		}
	}
}

// isCommandAvailable checks if a command is available in the system PATH
func isCommandAvailable(command string) bool {
	_, err := exec.LookPath(command)
	return err == nil
}

// launchEditor starts the specified editor with the given file
func launchEditor(editor *EditorInfo, filePath string) error {
	args := append(editor.Args, filePath)
	cmd := exec.Command(editor.Command, args...)
	
	// For terminal-based editors, we need to connect stdin/stdout
	if isTerminalEditor(editor.Command) {
		cmd.Stdin = os.Stdin
		cmd.Stdout = os.Stdout
		cmd.Stderr = os.Stderr
	}

	return cmd.Run()
}

// isTerminalEditor returns true if the editor runs in the terminal
func isTerminalEditor(command string) bool {
	terminalEditors := []string{"nano", "vim", "vi", "emacs", "joe", "micro"}
	baseName := filepath.Base(command)
	
	for _, editor := range terminalEditors {
		if baseName == editor {
			return true
		}
	}
	return false
}

// showScriptChanges displays a comparison of the original and modified scripts
func showScriptChanges(original, modified string) {
	fmt.Printf("\n📝 Script Changes Detected:\n")
	fmt.Printf("═══════════════════════════════════════\n\n")

	originalLines := strings.Split(original, "\n")
	modifiedLines := strings.Split(modified, "\n")

	// Simple diff - count lines added/removed/modified
	addedLines := 0
	removedLines := 0
	modifiedLinesCount := 0

	maxLines := len(originalLines)
	if len(modifiedLines) > maxLines {
		maxLines = len(modifiedLines)
	}

	for i := 0; i < maxLines; i++ {
		var origLine, modLine string
		
		if i < len(originalLines) {
			origLine = originalLines[i]
		}
		if i < len(modifiedLines) {
			modLine = modifiedLines[i]
		}

		if origLine == "" && modLine != "" {
			addedLines++
		} else if origLine != "" && modLine == "" {
			removedLines++
		} else if origLine != modLine {
			modifiedLinesCount++
		}
	}

	fmt.Printf("📊 Change Summary:\n")
	if addedLines > 0 {
		fmt.Printf("  ✅ Lines added: %d\n", addedLines)
	}
	if removedLines > 0 {
		fmt.Printf("  ❌ Lines removed: %d\n", removedLines)
	}
	if modifiedLinesCount > 0 {
		fmt.Printf("  ✏️  Lines modified: %d\n", modifiedLinesCount)
	}

	// Show a preview of significant changes (first few different lines)
	changeCount := 0
	maxPreview := 3

	fmt.Printf("\n🔍 Preview of Changes:\n")
	for i := 0; i < maxLines && changeCount < maxPreview; i++ {
		var origLine, modLine string
		
		if i < len(originalLines) {
			origLine = strings.TrimSpace(originalLines[i])
		}
		if i < len(modifiedLines) {
			modLine = strings.TrimSpace(modifiedLines[i])
		}

		if origLine != modLine && (origLine != "" || modLine != "") {
			changeCount++
			fmt.Printf("  Line %d:\n", i+1)
			if origLine != "" {
				fmt.Printf("    - %s\n", origLine)
			}
			if modLine != "" {
				fmt.Printf("    + %s\n", modLine)
			}
		}
	}

	if maxLines-changeCount > maxPreview {
		remaining := 0
		for i := maxPreview; i < maxLines; i++ {
			var origLine, modLine string
			if i < len(originalLines) {
				origLine = strings.TrimSpace(originalLines[i])
			}
			if i < len(modifiedLines) {
				modLine = strings.TrimSpace(modifiedLines[i])
			}
			if origLine != modLine {
				remaining++
			}
		}
		if remaining > 0 {
			fmt.Printf("    ... and %d more changes\n", remaining)
		}
	}
}

// OfferInlineEditing provides a simple line-by-line editing interface
func OfferInlineEditing(response *types.ScriptResponse) (*types.ScriptResponse, error) {
	fmt.Printf("\n✏️  Inline Script Editor\n")
	fmt.Printf("═══════════════════════════════════════\n")
	fmt.Printf("💡 Type 'done' to finish, 'cancel' to abort, or line number to edit\n\n")

	lines := strings.Split(response.Script, "\n")
	
	// Display current script with line numbers
	for i, line := range lines {
		fmt.Printf("%3d│ %s\n", i+1, line)
	}

	fmt.Printf("\n")
	
	reader := bufio.NewReader(os.Stdin)
	modified := false

	for {
		fmt.Printf("Edit> ")
		input, _ := reader.ReadString('\n')
		input = strings.TrimSpace(input)

		switch strings.ToLower(input) {
		case "done":
			if modified {
				newScript := strings.Join(lines, "\n")
				editedResponse := &types.ScriptResponse{
					TaskDescription: response.TaskDescription + " (inline edited)",
					Script:         newScript,
					ScriptType:     response.ScriptType,
					Model:          response.Model,
					Provider:       response.Provider,
				}
				fmt.Printf("✅ Inline editing completed!\n")
				return editedResponse, nil
			} else {
				fmt.Printf("📝 No changes made\n")
				return response, nil
			}
		case "cancel":
			fmt.Printf("🚫 Inline editing cancelled\n")
			return response, nil
		default:
			// Try to parse as line number
			if lineNum := parseLineNumber(input); lineNum > 0 && lineNum <= len(lines) {
				fmt.Printf("Current line %d: %s\n", lineNum, lines[lineNum-1])
				fmt.Printf("New content (or press Enter to keep): ")
				newContent, _ := reader.ReadString('\n')
				newContent = strings.TrimRight(newContent, "\n\r")
				
				if newContent != "" {
					lines[lineNum-1] = newContent
					modified = true
					fmt.Printf("✅ Line %d updated\n", lineNum)
				} else {
					fmt.Printf("📝 Line %d unchanged\n", lineNum)
				}
			} else {
				fmt.Printf("❌ Invalid command. Enter line number, 'done', or 'cancel'\n")
			}
		}
	}
}

// parseLineNumber attempts to parse a string as a line number
func parseLineNumber(input string) int {
	// Simple integer parsing
	lineNum := 0
	for _, char := range input {
		if char >= '0' && char <= '9' {
			lineNum = lineNum*10 + int(char-'0')
		} else {
			return 0 // Invalid number
		}
	}
	return lineNum
}
