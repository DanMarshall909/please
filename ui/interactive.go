package ui

import (
	"bufio"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"time"

	"please/config"
	"please/localization"
	"please/providers"
	"please/script"
	"please/types"
)

var locManager *localization.LocalizationManager // Global for now

// ShowMainMenu displays the main interactive menu when Please is run without arguments
func ShowMainMenu() {
	if locManager == nil {
		// In real use, pass config dir; here, use current dir for stub
		mgr, _ := localization.NewLocalizationManager(".")
		locManager = mgr
	}
	// Show banner
	fmt.Printf("╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                           🤖 Please Script Generator                         ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")
	for {
		fmt.Printf("%s%s%s\n\n", ColorBold+ColorCyan, locManager.System.Get("menus.main_prompt"), ColorReset)
		fmt.Printf("  %s1.%s %s📖 %s%s\n", ColorGreen, ColorReset, ColorCyan, locManager.System.Get("menus.show_help"), ColorReset)
		fmt.Printf("  %s2.%s %s✨ %s%s\n", ColorGreen, ColorReset, ColorYellow, locManager.System.Get("menus.generate_script"), ColorReset)
		fmt.Printf("  %s3.%s %s🔄 %s%s\n", ColorGreen, ColorReset, ColorMagenta, locManager.System.Get("menus.load_last"), ColorReset)
		fmt.Printf("  %s4.%s %s📚 %s%s\n", ColorGreen, ColorReset, ColorBlue, locManager.System.Get("menus.browse_history"), ColorReset)
		fmt.Printf("  %s5.%s %s⚙️  %s%s\n", ColorGreen, ColorReset, ColorPurple, locManager.System.Get("menus.show_config"), ColorReset)
		fmt.Printf("  %s6.%s %s🚪 %s%s\n\n", ColorGreen, ColorReset, ColorDim, locManager.System.Get("menus.exit"), ColorReset)
		fmt.Printf("%sPress 1-6: %s", ColorBold+ColorYellow, ColorReset)
		choice := getSingleKeyInput()
		fmt.Printf("%c\n", choice)
		if handleMainMenuChoice(string(choice)) {
			break
		}
	}
}

// handleMainMenuChoice processes the main menu selection and returns true if should exit
func handleMainMenuChoice(choice string) bool {
	// Handle Enter key as immediate exit
	if choice == "\r" || choice == "\n" {
		fmt.Printf("%s%s%s\n", ColorGreen, locManager.System.Get("success.exit_quick"), ColorReset)
		return true // Exit immediately on Enter
	}

	// Handle other special characters that should be ignored
	if len(choice) == 0 || choice == " " {
		return false // Ignore empty or space - continue showing menu
	}

	switch choice {
	case "1":
		ShowHelp()
		return false // Continue showing main menu
	case "2":
		generateNewScript()
		return false // Continue showing main menu
	case "3":
		loadLastScript()
		return false // Continue showing main menu
	case "4":
		browseHistory()
		return false // Continue showing main menu
	case "5":
		showConfiguration()
		return false // Continue showing main menu
	case "6":
		fmt.Printf("%s%s%s\n", ColorGreen, locManager.System.Get("success.exit"), ColorReset)
		return true // Exit
	default:
		fmt.Printf("%s%s%s\n", ColorRed, locManager.System.Get("errors.invalid_choice"), ColorReset)
		return false // Continue showing main menu
	}
}

// generateNewScript prompts for task description and generates a script
func generateNewScript() {
	fmt.Printf("\n%s✨ Generate New Script%s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("%s═══════════════════════════════════════%s\n\n", ColorCyan, ColorReset)

	fmt.Printf("%sDescribe what you want your script to do: %s", ColorYellow, ColorReset)
	reader := bufio.NewReader(os.Stdin)
	taskDescription, _ := reader.ReadString('\n')
	taskDescription = strings.TrimSpace(taskDescription)

	if taskDescription == "" {
		fmt.Printf("%s❌ No task description provided.%s\n", ColorRed, ColorReset)
		return
	}

	fmt.Printf("\n%s🚀 Generating script for: %s%s\n", ColorGreen, taskDescription, ColorReset)
	fmt.Printf("%s💭 This feature will be implemented to call the main script generation flow...%s\n", ColorDim, ColorReset)
	fmt.Printf("%s💡 For now, use: please %s%s\n", ColorDim, taskDescription, ColorReset)
}

// browseHistory shows the script history browser
func browseHistory() {
	fmt.Printf("\n%s📚 Script History Browser%s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("%s═══════════════════════════════════════%s\n\n", ColorCyan, ColorReset)
	fmt.Printf("%s🔄 History browser feature coming soon!%s\n", ColorPurple, ColorReset)
	fmt.Printf("%s💡 Will show last 20 scripts with timestamps and quick selection.%s\n", ColorDim, ColorReset)
}

// showConfiguration displays current Please configuration
func showConfiguration() {
	fmt.Printf("\n%s⚙️ Please Configuration%s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("%s═══════════════════════════════════════%s\n\n", ColorCyan, ColorReset)

	// Show basic configuration info
	fmt.Printf("%s🔧 Current Settings:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s• Config directory:%s ~/.please/\n", ColorDim, ColorReset)
	fmt.Printf("  %s• Default provider:%s %s\n", ColorDim, ColorReset, "ollama (auto-detected)")
	fmt.Printf("  %s• Default model:%s %s\n", ColorDim, ColorReset, "deepseek-coder:6.7b")
	fmt.Printf("  %s• Script type:%s %s\n", ColorDim, ColorReset, "powershell (Windows)")

	fmt.Printf("\n%s🔗 Environment Variables:%s\n", ColorBold+ColorYellow, ColorReset)

	pleaseProvider := os.Getenv("PLEASE_PROVIDER")
	oohllamaProvider := os.Getenv("OOHLAMA_PROVIDER")

	if pleaseProvider != "" {
		fmt.Printf("  %s• PLEASE_PROVIDER:%s %s\n", ColorDim, ColorReset, pleaseProvider)
	} else if oohllamaProvider != "" {
		fmt.Printf("  %s• OOHLAMA_PROVIDER:%s %s %s(legacy)%s\n", ColorDim, ColorReset, oohllamaProvider, ColorYellow, ColorReset)
	} else {
		fmt.Printf("  %s• No provider environment variables set%s\n", ColorDim, ColorReset)
	}

	fmt.Printf("\n%s💡 Tip: Set PLEASE_PROVIDER environment variable to change default provider%s\n", ColorDim, ColorReset)
}

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

// getSingleKeyWindows captures single key on Windows using getch-like functionality
func getSingleKeyWindows() rune {
	// Use PowerShell's Read-Host with single character mode
	cmd := exec.Command("powershell", "-Command", "$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown').Character")
	output, err := cmd.Output()
	if err != nil {
		// Fallback to regular input
		reader := bufio.NewReader(os.Stdin)
		input, _ := reader.ReadString('\n')
		if len(input) > 0 {
			return rune(input[0])
		}
		return '7'
	}

	if len(output) > 0 {
		return rune(output[0])
	}
	return '7'
}

// getSingleKeyUnix captures single key on Unix systems using stty
func getSingleKeyUnix() rune {
	// Save current terminal settings
	cmd := exec.Command("stty", "-g")
	originalSettings, err := cmd.Output()
	if err != nil {
		// Fallback to regular input
		reader := bufio.NewReader(os.Stdin)
		input, _ := reader.ReadString('\n')
		if len(input) > 0 {
			return rune(input[0])
		}
		return '7'
	}

	// Set terminal to raw mode (single character, no echo)
	exec.Command("stty", "cbreak", "-echo").Run()

	// Restore terminal settings when done
	defer func() {
		exec.Command("stty", string(originalSettings)).Run()
	}()

	// Read single character
	reader := bufio.NewReader(os.Stdin)
	char, _, err := reader.ReadRune()
	if err != nil {
		return '7'
	}

	return char
}

// handleUserChoice processes the user's menu selection and returns true if should exit
func handleUserChoice(choice string, response *types.ScriptResponse) bool {
	// Handle Enter key as immediate exit
	if choice == "\r" || choice == "\n" {
		fmt.Printf("%s✨ Quick exit! Thanks for using Please! 🎉%s\n", ColorGreen, ColorReset)
		return true // Exit immediately on Enter
	}

	// Handle other special characters that should be ignored
	if len(choice) == 0 || choice == " " {
		return false // Ignore empty or space - continue showing menu
	}

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

// executeScript executes the script with smart safety levels and automatic error recovery
func executeScript(response *types.ScriptResponse) {
	executed := false

	// Get script warnings and determine risk level
	warnings := script.ValidateScript(response)
	riskLevel := determineRiskLevel(warnings)

	switch riskLevel {
	case "green":
		// Low risk - execute immediately with brief message
		fmt.Printf("%s✅ Executing safe script...%s\n", ColorGreen, ColorReset)
		executed = true
		if err := script.ExecuteScript(response); err != nil {
			fmt.Printf("%s❌ Script execution failed: %v%s\n", ColorRed, err, ColorReset)
			// Attempt automatic fix
			tryAutoFix(response, err.Error())
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
			executed = true
			if err := script.ExecuteScript(response); err != nil {
				fmt.Printf("%s❌ Script execution failed: %v%s\n", ColorRed, err, ColorReset)
				// Attempt automatic fix
				tryAutoFix(response, err.Error())
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
			executed = true
			if err := script.ExecuteScript(response); err != nil {
				fmt.Printf("%s❌ Script execution failed: %v%s\n", ColorRed, err, ColorReset)
				// For high-risk scripts, ask before attempting auto-fix
				fmt.Printf("%s❓ Attempt automatic fix? Press 'y' to try or any other key to skip: %s", ColorBold+ColorYellow, ColorReset)
				fixChoice := getSingleKeyInput()
				fmt.Printf("%c\n", fixChoice)
				if fixChoice == 'y' || fixChoice == 'Y' {
					tryAutoFix(response, err.Error())
				}
			} else {
				fmt.Printf("%s✅ Script execution completed!%s\n", ColorGreen, ColorReset)
			}
		} else {
			fmt.Printf("%s🚫 Script execution cancelled for safety.%s\n", ColorYellow, ColorReset)
		}
	}

	// Save to history if executed (whether successful or not)
	if executed {
		saveToHistory(response)
	}
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
	fmt.Printf("%s✏️  Edit Script%s\n", ColorBold+ColorPurple, ColorReset)
	fmt.Printf("%s═══════════════════════════════════════%s\n\n", ColorPurple, ColorReset)

	// Offer editing options
	fmt.Printf("%sChoose editing method:%s\n\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s1.%s %sOpen in external editor (recommended)%s\n", ColorGreen, ColorReset, ColorCyan, ColorReset)
	fmt.Printf("  %s2.%s %sInline editing (line-by-line)%s\n", ColorGreen, ColorReset, ColorBlue, ColorReset)
	fmt.Printf("  %s3.%s %sCancel editing%s\n\n", ColorGreen, ColorReset, ColorDim, ColorReset)

	fmt.Printf("%sPress 1-3: %s", ColorBold+ColorYellow, ColorReset)
	choice := getSingleKeyInput()
	fmt.Printf("%c\n", choice)

	switch string(choice) {
	case "1":
		// External editor
		if editedResponse, err := script.EditScript(response); err != nil {
			fmt.Printf("%s❌ Editor failed: %v%s\n", ColorRed, err, ColorReset)
			fmt.Printf("%s💡 Tip: Set EDITOR environment variable or install VS Code%s\n", ColorDim, ColorReset)
		} else if editedResponse != response {
			// Script was modified, update the response and continue with new script
			*response = *editedResponse
			fmt.Printf("%s🎯 Updated script is now active in the menu%s\n", ColorGreen, ColorReset)
		}

	case "2":
		// Inline editing
		if editedResponse, err := script.OfferInlineEditing(response); err != nil {
			fmt.Printf("%s❌ Inline editing failed: %v%s\n", ColorRed, err, ColorReset)
		} else if editedResponse != response {
			// Script was modified, update the response
			*response = *editedResponse
			fmt.Printf("%s🎯 Updated script is now active in the menu%s\n", ColorGreen, ColorReset)
		}

	case "3":
		fmt.Printf("%s🚫 Editing cancelled%s\n", ColorYellow, ColorReset)

	default:
		fmt.Printf("%s❌ Invalid choice%s\n", ColorRed, ColorReset)
	}
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

// loadLastScript loads and displays the last generated script with execution options
func loadLastScript() {
	response := loadLastScriptData()
	if response == nil {
		return
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
	
	// Show the script menu for this loaded script
	ShowScriptMenu(response)
}

// loadLastScriptData loads the last script data and returns a ScriptResponse, or nil if not found
func loadLastScriptData() *types.ScriptResponse {
	configDir, err := getConfigDir()
	if err != nil {
		fmt.Printf("%s❌ Could not access config directory: %v%s\n", ColorRed, err, ColorReset)
		return nil
	}

	lastScriptPath := filepath.Join(configDir, "last_script.json")

	if _, err := os.Stat(lastScriptPath); os.IsNotExist(err) {
		fmt.Printf("%s📭 No previous script found.%s\n", ColorYellow, ColorReset)
		fmt.Printf("%s💡 Generate a script first, then use this option to reload it.%s\n", ColorDim, ColorReset)
		return nil
	}

	data, err := os.ReadFile(lastScriptPath)
	if err != nil {
		fmt.Printf("%s❌ Could not read last script: %v%s\n", ColorRed, err, ColorReset)
		return nil
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
		Script:          script,
		ScriptType:      scriptType,
		Model:           model,
		Provider:        provider,
	}

	return response
}

// runLastScript directly executes the last script with safety checks
func runLastScript() {
	response := loadLastScriptData()
	if response == nil {
		return
	}

	fmt.Printf("\n%s🚀 Running last script: %s%s\n", ColorGreen, response.TaskDescription, ColorReset)
	
	// Show a brief preview
	lines := strings.Split(response.Script, "\n")
	previewLines := 3
	if len(lines) > previewLines {
		fmt.Printf("%s💡 Script preview (first %d lines):%s\n", ColorDim, previewLines, ColorReset)
		for i := 0; i < previewLines; i++ {
			fmt.Printf("%s  %d│ %s%s\n", ColorDim, i+1, lines[i], ColorReset)
		}
		fmt.Printf("%s  ... (%d more lines)%s\n", ColorDim, len(lines)-previewLines, ColorReset)
	} else {
		fmt.Printf("%s💡 Script content:%s\n", ColorDim, ColorReset)
		for i, line := range lines {
			fmt.Printf("%s  %d│ %s%s\n", ColorDim, i+1, line, ColorReset)
		}
	}

	// Execute with safety validation
	executeScript(response)
}

// RunLastScriptFromCLI executes the last script directly from command line interface
func RunLastScriptFromCLI() {
	response := loadLastScriptData()
	if response == nil {
		return
	}

	fmt.Printf("╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                           🤖 Please Script Generator                         ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	fmt.Printf("%s🔄 Running last script from command line...%s\n", ColorMagenta, ColorReset)
	fmt.Printf("\n%s📝 Task:%s %s\n", ColorBold+ColorCyan, ColorReset, response.TaskDescription)
	fmt.Printf("%s🧠 Model:%s %s (%s)\n", ColorBold+ColorCyan, ColorReset, response.Model, response.Provider)
	fmt.Printf("%s🖥️  Platform:%s %s script\n", ColorBold+ColorCyan, ColorReset, response.ScriptType)

	// Show a brief preview of the script
	lines := strings.Split(response.Script, "\n")
	previewLines := 5
	fmt.Printf("\n%s💡 Script preview:%s\n", ColorDim, ColorReset)
	if len(lines) > previewLines {
		for i := 0; i < previewLines; i++ {
			fmt.Printf("%s  %d│ %s%s\n", ColorDim, i+1, lines[i], ColorReset)
		}
		fmt.Printf("%s  ... (%d more lines)%s\n", ColorDim, len(lines)-previewLines, ColorReset)
	} else {
		for i, line := range lines {
			fmt.Printf("%s  %d│ %s%s\n", ColorDim, i+1, line, ColorReset)
		}
	}

	fmt.Printf("\n%s🚀 Executing script...%s\n", ColorGreen, ColorReset)
	
	// Execute with safety validation
	executeScript(response)
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

// tryAutoFix attempts to automatically fix a failed script using AI
func tryAutoFix(originalResponse *types.ScriptResponse, errorMessage string) {
	fmt.Printf("%s🔧 Auto-fixing script...%s\n", ColorYellow, ColorReset)

	// Load config for provider
	cfg, err := config.Load()
	if err != nil {
		fmt.Printf("%s❌ Could not load config for auto-fix: %v%s\n", ColorRed, err, ColorReset)
		return
	}

	// Call the AI provider to generate a fixed script
	fixedScript, err := providers.GenerateFixedScript(
		originalResponse.Script,
		errorMessage,
		originalResponse.ScriptType,
		originalResponse.Model,
		originalResponse.Provider,
		cfg,
	)
	if err != nil {
		fmt.Printf("%s❌ Auto-fix failed: %v%s\n", ColorRed, err, ColorReset)
		return
	}
	fixedResponse := &types.ScriptResponse{
		TaskDescription: "Auto-fix for: " + originalResponse.TaskDescription,
		Script:          fixedScript,
		ScriptType:      originalResponse.ScriptType,
		Model:           originalResponse.Model,
		Provider:        originalResponse.Provider,
	}

	// Display the fixed script
	fmt.Printf("\n%s✨ Generated automatic fix:%s\n", ColorGreen, ColorReset)
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("─", 60), ColorReset)

	lines := strings.Split(fixedResponse.Script, "\n")
	for i, line := range lines {
		fmt.Printf("%s%3d│%s %s\n", ColorDim, i+1, ColorReset, line)
	}
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("─", 60), ColorReset)

	// Execute the fixed script automatically
	fmt.Printf("%s🚀 Testing fixed script...%s\n", ColorGreen, ColorReset)
	if err := script.ExecuteScript(fixedResponse); err != nil {
		fmt.Printf("%s❌ Fixed script also failed: %v%s\n", ColorRed, err, ColorReset)
		fmt.Printf("%s💡 Manual intervention may be required.%s\n", ColorDim, ColorReset)
	} else {
		fmt.Printf("%s🎉 Fixed script executed successfully!%s\n", ColorGreen, ColorReset)
		// Update the original response with the fixed version
		originalResponse.Script = fixedResponse.Script
		originalResponse.TaskDescription = fixedResponse.TaskDescription
	}
}

// saveToHistory saves the script to the execution history
func saveToHistory(response *types.ScriptResponse) {
	configDir, err := getConfigDir()
	if err != nil {
		return // Silently fail
	}

	historyDir := filepath.Join(configDir, "history")
	if err := os.MkdirAll(historyDir, 0755); err != nil {
		return // Silently fail
	}

	// Create filename with timestamp
	timestamp := fmt.Sprintf("%d", time.Now().Unix())
	historyFile := filepath.Join(historyDir, fmt.Sprintf("script_%s.json", timestamp))

	// Create enhanced JSON with timestamp and execution info
	jsonContent := fmt.Sprintf(`{
  "timestamp": %s,
  "task_description": "%s",
  "script": "%s",
  "script_type": "%s",
  "model": "%s",
  "provider": "%s",
  "executed_at": "%s"
}`,
		timestamp,
		strings.ReplaceAll(response.TaskDescription, `"`, `\"`),
		strings.ReplaceAll(response.Script, `"`, `\"`),
		response.ScriptType,
		response.Model,
		response.Provider,
		time.Now().Format("2006-01-02 15:04:05"))

	os.WriteFile(historyFile, []byte(jsonContent), 0644)
}
