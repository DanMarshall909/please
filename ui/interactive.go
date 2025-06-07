package ui

import (
	"bufio"
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strings"

	"please/script"
	"please/types"
)

// ShowScriptMenu displays an interactive menu after script generation
func ShowScriptMenu(response *types.ScriptResponse) {
	for {
		fmt.Printf("\n%s🎯 What would you like to do with this script?%s\n\n", ColorBold+ColorCyan, ColorReset)

		// Show menu options
		fmt.Printf("  %s1.%s %s📋 Copy to clipboard%s\n", ColorGreen, ColorReset, ColorCyan, ColorReset)
		fmt.Printf("  %s2.%s %s▶️  Execute script now%s\n", ColorGreen, ColorReset, ColorYellow, ColorReset)
		fmt.Printf("  %s3.%s %s💾 Save to file%s\n", ColorGreen, ColorReset, ColorBlue, ColorReset)
		fmt.Printf("  %s4.%s %s✏️  Edit script%s\n", ColorGreen, ColorReset, ColorPurple, ColorReset)
		fmt.Printf("  %s5.%s %s📖 Show detailed explanation%s\n", ColorGreen, ColorReset, ColorWhite, ColorReset)
		fmt.Printf("  %s6.%s %s🔄 Load last script%s\n", ColorGreen, ColorReset, ColorMagenta, ColorReset)
		fmt.Printf("  %s7.%s %s🚪 Exit%s\n\n", ColorGreen, ColorReset, ColorDim, ColorReset)

		// Get user choice with single-key input
		fmt.Printf("%sPress 1-7: %s", ColorBold+ColorYellow, ColorReset)
		choice := getSingleKeyInput()
		fmt.Printf("%c\n", choice) // Echo the pressed key

		if handleUserChoice(string(choice), response) {
			break // Exit if user chose exit or completed an action
		}
	}
}

// getSingleKeyInput captures a single keypress without requiring Enter
func getSingleKeyInput() rune {
	if runtime.GOOS == "windows" {
		return getSingleKeyWindows()
	}
	return getSingleKeyUnix()
}

// getSingleKeyWindows captures single key on Windows
func getSingleKeyWindows() rune {
	// For Windows, we'll use a simple approach for now
	// In a production environment, you'd want to use Windows API calls
	reader := bufio.NewReader(os.Stdin)
	input, _ := reader.ReadString('\n')
	if len(input) > 0 {
		return rune(input[0])
	}
	return '7' // Default to exit
}

// getSingleKeyUnix captures single key on Unix systems
func getSingleKeyUnix() rune {
	// Simplified for now - in production, you'd use termios to set raw mode
	reader := bufio.NewReader(os.Stdin)
	input, _ := reader.ReadString('\n')
	if len(input) > 0 {
		return rune(input[0])
	}
	return '7' // Default to exit
}

// handleUserChoice processes the user's menu selection and returns true if should exit
func handleUserChoice(choice string, response *types.ScriptResponse) bool {
	switch choice {
	case "1":
		copyToClipboard(response)
		return false // Continue showing menu
	case "2":
		executeScript(response)
		return false // Continue showing menu
	case "3":
		saveToFile(response)
		return false // Continue showing menu
	case "4":
		editScript(response)
		return false // Continue showing menu
	case "5":
		showDetailedExplanation(response)
		return false // Continue showing menu
	case "6":
		loadLastScript()
		return false // Continue showing menu
	case "7":
		fmt.Printf("%s✨ Ta-da! Thanks for using Please! Happy scripting! 🎉%s\n", ColorGreen, ColorReset)
		return true // Exit
	default:
		fmt.Printf("%s❌ Invalid choice. Please try again.%s\n", ColorRed, ColorReset)
		return false // Continue showing menu
	}
}

// copyToClipboard copies the script to the system clipboard
func copyToClipboard(response *types.ScriptResponse) {
	fmt.Printf("%s📋 Copying script to clipboard...%s\n", ColorCyan, ColorReset)
	
	if err := script.CopyToClipboard(response.Script); err != nil {
		fmt.Printf("%s❌ Failed to copy to clipboard: %v%s\n", ColorRed, err, ColorReset)
		fmt.Printf("%s💡 You can manually copy the script above%s\n", ColorDim, ColorReset)
	} else {
		fmt.Printf("%s✅ Script copied to clipboard!%s\n", ColorGreen, ColorReset)
		fmt.Printf("%s💡 You can now paste it anywhere with Ctrl+V (Cmd+V on macOS)%s\n", ColorDim, ColorReset)
	}
}

// executeScript executes the script with smart safety levels
func executeScript(response *types.ScriptResponse) {
	// Get script warnings and determine risk level
	warnings := script.ValidateScript(response)
	riskLevel := determineRiskLevel(warnings)
	
	switch riskLevel {
	case "green":
		// Low risk - execute immediately with brief message
		fmt.Printf("%s✅ Executing safe script...%s\n", ColorGreen, ColorReset)
		if err := script.ExecuteScript(response); err != nil {
			fmt.Printf("%s❌ Script execution failed: %v%s\n", ColorRed, err, ColorReset)
		} else {
			fmt.Printf("%s✅ Script execution completed!%s\n", ColorGreen, ColorReset)
		}
		
	case "yellow":
		// Medium risk - single confirmation
		if len(warnings) > 0 {
			fmt.Printf("%s⚠️  Script has some warnings:%s\n", ColorYellow, ColorReset)
			for _, warning := range warnings {
				if strings.HasPrefix(warning, "🟡") {
					fmt.Printf("  %s%s%s\n", ColorYellow, warning, ColorReset)
				}
			}
		}
		fmt.Printf("%s❓ Press 'y' to continue or any other key to cancel: %s", ColorBold+ColorYellow, ColorReset)
		choice := getSingleKeyInput()
		fmt.Printf("%c\n", choice)
		
		if choice == 'y' || choice == 'Y' {
			fmt.Printf("%s▶️  Executing script...%s\n", ColorGreen, ColorReset)
			if err := script.ExecuteScript(response); err != nil {
				fmt.Printf("%s❌ Script execution failed: %v%s\n", ColorRed, err, ColorReset)
			} else {
				fmt.Printf("%s✅ Script execution completed!%s\n", ColorGreen, ColorReset)
			}
		} else {
			fmt.Printf("%s🚫 Script execution cancelled.%s\n", ColorYellow, ColorReset)
		}
		
	case "red":
		// High risk - detailed warning flow
		fmt.Printf("%s🚨 HIGH RISK SCRIPT DETECTED!%s\n", ColorRed+ColorBold, ColorReset)
		for _, warning := range warnings {
			if strings.HasPrefix(warning, "🔴") || strings.HasPrefix(warning, "⛔") {
				fmt.Printf("  %s%s%s\n", ColorRed, warning, ColorReset)
			}
		}
		fmt.Printf("\n%s🛡️  SAFETY WARNING: This script contains potentially dangerous operations!%s\n", ColorRed+ColorBold, ColorReset)
		fmt.Printf("%s❓ Type 'EXECUTE' to proceed or anything else to cancel: %s", ColorBold+ColorRed, ColorReset)
		
		reader := bufio.NewReader(os.Stdin)
		input, _ := reader.ReadString('\n')
		
		if strings.TrimSpace(input) == "EXECUTE" {
			fmt.Printf("%s⚠️  Executing high-risk script...%s\n", ColorRed, ColorReset)
			if err := script.ExecuteScript(response); err != nil {
				fmt.Printf("%s❌ Script execution failed: %v%s\n", ColorRed, err, ColorReset)
			} else {
				fmt.Printf("%s✅ Script execution completed!%s\n", ColorGreen, ColorReset)
			}
		} else {
			fmt.Printf("%s🚫 Script execution cancelled for safety.%s\n", ColorYellow, ColorReset)
		}
	}
	
	// Save as last script after execution attempt
	saveLastScript(response)
}

// determineRiskLevel analyzes warnings to determine overall risk level
func determineRiskLevel(warnings []string) string {
	hasRed := false
	hasYellow := false
	
	for _, warning := range warnings {
		if strings.HasPrefix(warning, "⛔") || strings.HasPrefix(warning, "🔴") {
			hasRed = true
		} else if strings.HasPrefix(warning, "🟡") {
			hasYellow = true
		}
	}
	
	if hasRed {
		return "red"
	} else if hasYellow {
		return "yellow"
	}
	return "green"
}

// saveToFile saves the script to a file
func saveToFile(response *types.ScriptResponse) {
	fmt.Printf("%s💾 Saving script to file...%s\n", ColorBlue, ColorReset)
	
	// Get suggested filename from script package
	defaultFilename := script.GetSuggestedFilename(response)
	fmt.Printf("%sEnter filename (press Enter for '%s'): %s", ColorYellow, defaultFilename, ColorReset)
	
	reader := bufio.NewReader(os.Stdin)
	input, _ := reader.ReadString('\n')
	filename := strings.TrimSpace(input)
	
	if filename == "" {
		filename = defaultFilename
	}
	
	// Save using script package
	if err := script.SaveToFile(response.Script, filename); err != nil {
		fmt.Printf("%s❌ Failed to save script: %v%s\n", ColorRed, err, ColorReset)
	} else {
		fmt.Printf("%s✅ Script saved as '%s'!%s\n", ColorGreen, filename, ColorReset)
		fmt.Printf("%s💡 File is ready to use%s\n", ColorDim, ColorReset)
	}
	
	// Save as last script
	saveLastScript(response)
}

// editScript allows the user to edit the script
func editScript(response *types.ScriptResponse) {
	fmt.Printf("%s✏️  Script editing feature coming soon!%s\n", ColorPurple, ColorReset)
	fmt.Printf("%s💡 For now, you can copy the script and edit it in your favorite editor.%s\n", ColorDim, ColorReset)
}

// showDetailedExplanation shows a detailed breakdown of the script
func showDetailedExplanation(response *types.ScriptResponse) {
	fmt.Printf("\n%s📖 Detailed Script Explanation%s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("%s═══════════════════════════════════════%s\n\n", ColorCyan, ColorReset)
	
	fmt.Printf("%s🎯 Task Analysis:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s• Original request:%s %s\n", ColorDim, ColorReset, response.TaskDescription)
	fmt.Printf("  %s• Script type:%s %s\n", ColorDim, ColorReset, response.ScriptType)
	fmt.Printf("  %s• AI model used:%s %s (%s)\n", ColorDim, ColorReset, response.Model, response.Provider)
	
	fmt.Printf("\n%s🔍 Script Analysis:%s\n", ColorBold+ColorYellow, ColorReset)
	
	lines := strings.Split(response.Script, "\n")
	commentCount := 0
	commandCount := 0
	
	for _, line := range lines {
		trimmed := strings.TrimSpace(line)
		if response.ScriptType == "powershell" {
			if strings.HasPrefix(trimmed, "#") {
				commentCount++
			} else if trimmed != "" {
				commandCount++
			}
		} else { // bash
			if strings.HasPrefix(trimmed, "#") {
				commentCount++
			} else if trimmed != "" {
				commandCount++
			}
		}
	}
	
	fmt.Printf("  %s• Total lines:%s %d\n", ColorDim, ColorReset, len(lines))
	fmt.Printf("  %s• Comment lines:%s %d\n", ColorDim, ColorReset, commentCount)
	fmt.Printf("  %s• Command lines:%s %d\n", ColorDim, ColorReset, commandCount)
	
	fmt.Printf("\n%s💡 Usage Tips:%s\n", ColorBold+ColorYellow, ColorReset)
	if response.ScriptType == "powershell" {
		fmt.Printf("  %s• Run in PowerShell with:%s ./script.ps1\n", ColorDim, ColorReset)
		fmt.Printf("  %s• May need to set execution policy:%s Set-ExecutionPolicy RemoteSigned\n", ColorDim, ColorReset)
	} else {
		fmt.Printf("  %s• Make executable:%s chmod +x script.sh\n", ColorDim, ColorReset)
		fmt.Printf("  %s• Run with:%s ./script.sh\n", ColorDim, ColorReset)
	}
	fmt.Printf("  %s• Always review scripts before execution%s\n", ColorDim, ColorReset)
}

// saveLastScript saves the current script as the last script
func saveLastScript(response *types.ScriptResponse) {
	configDir, err := getConfigDir()
	if err != nil {
		return // Silently fail
	}
	
	lastScriptPath := filepath.Join(configDir, "last_script.json")
	
	// Create a simple JSON representation
	jsonContent := fmt.Sprintf(`{
  "task_description": "%s",
  "script": "%s",
  "script_type": "%s",
  "model": "%s",
  "provider": "%s"
}`, 
		strings.ReplaceAll(response.TaskDescription, `"`, `\"`),
		strings.ReplaceAll(response.Script, `"`, `\"`),
		response.ScriptType,
		response.Model,
		response.Provider)
	
	os.WriteFile(lastScriptPath, []byte(jsonContent), 0644)
}

// loadLastScript loads and displays the last generated script
func loadLastScript() {
	configDir, err := getConfigDir()
	if err != nil {
		fmt.Printf("%s❌ Could not access config directory: %v%s\n", ColorRed, err, ColorReset)
		return
	}
	
	lastScriptPath := filepath.Join(configDir, "last_script.json")
	
	if _, err := os.Stat(lastScriptPath); os.IsNotExist(err) {
		fmt.Printf("%s📭 No previous script found.%s\n", ColorYellow, ColorReset)
		fmt.Printf("%s💡 Generate a script first, then use this option to reload it.%s\n", ColorDim, ColorReset)
		return
	}
	
	data, err := os.ReadFile(lastScriptPath)
	if err != nil {
		fmt.Printf("%s❌ Could not read last script: %v%s\n", ColorRed, err, ColorReset)
		return
	}
	
	// For simplicity, we'll parse this manually (in production, use proper JSON)
	content := string(data)
	
	// Extract fields (simplified parsing)
	taskDesc := extractJSONField(content, "task_description")
	script := extractJSONField(content, "script")
	scriptType := extractJSONField(content, "script_type")
	model := extractJSONField(content, "model")
	provider := extractJSONField(content, "provider")
	
	// Create response object
	response := &types.ScriptResponse{
		TaskDescription: taskDesc,
		Script:         script,
		ScriptType:     scriptType,
		Model:          model,
		Provider:       provider,
	}
	
	// Display the loaded script
	fmt.Printf("\n%s🔄 Loading last script...%s\n", ColorMagenta, ColorReset)
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("═", 78), ColorReset)
	fmt.Printf("\n%s📝 Task:%s %s\n", ColorBold+ColorCyan, ColorReset, response.TaskDescription)
	fmt.Printf("%s🧠 Model:%s %s (%s)\n", ColorBold+ColorCyan, ColorReset, response.Model, response.Provider)
	fmt.Printf("%s🖥️  Platform:%s %s script\n", ColorBold+ColorCyan, ColorReset, response.ScriptType)
	
	fmt.Printf("\n%s%s%s\n", ColorDim, strings.Repeat("═", 78), ColorReset)
	fmt.Printf("%s                              📋 Last Generated Script                             %s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("═", 78), ColorReset)
	
	// Display script with line numbers
	lines := strings.Split(response.Script, "\n")
	for i, line := range lines {
		fmt.Printf("%s%3d│%s %s\n", ColorDim, i+1, ColorReset, line)
	}
	
	fmt.Printf("\n%s✅ Last script loaded successfully!%s\n", ColorGreen, ColorReset)
}

// getConfigDir returns the configuration directory for Please
func getConfigDir() (string, error) {
	var configDir string

	switch runtime.GOOS {
	case "windows":
		appData := os.Getenv("APPDATA")
		if appData == "" {
			return "", fmt.Errorf("APPDATA environment variable not set")
		}
		configDir = filepath.Join(appData, "please")
	case "darwin":
		homeDir, err := os.UserHomeDir()
		if err != nil {
			return "", fmt.Errorf("could not get user home directory: %v", err)
		}
		configDir = filepath.Join(homeDir, "Library", "Application Support", "please")
	default: // Linux and others
		homeDir, err := os.UserHomeDir()
		if err != nil {
			return "", fmt.Errorf("could not get user home directory: %v", err)
		}
		configDir = filepath.Join(homeDir, ".config", "please")
	}

	// Create directory if it doesn't exist
	if err := os.MkdirAll(configDir, 0755); err != nil {
		return "", fmt.Errorf("failed to create config directory: %v", err)
	}

	return configDir, nil
}

// extractJSONField extracts a field value from a simple JSON string
func extractJSONField(content, field string) string {
	// Simple JSON field extraction (not production-ready)
	fieldPattern := `"` + field + `": "`
	start := strings.Index(content, fieldPattern)
	if start == -1 {
		return ""
	}
	start += len(fieldPattern)
	
	end := strings.Index(content[start:], `"`)
	if end == -1 {
		return ""
	}
	
	value := content[start : start+end]
	// Unescape quotes
	value = strings.ReplaceAll(value, `\"`, `"`)
	return value
}
