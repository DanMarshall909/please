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

// UIService encapsulates dependencies for UI operations
type UIService struct {
	LocManager *localization.LocalizationManager
}

// NewUIService creates a new UI service with initialized dependencies
func NewUIService(configDir string) (*UIService, error) {
	locManager, err := localization.NewLocalizationManager(configDir)
	if err != nil {
		// Fallback to current directory if config dir fails
		locManager, err = localization.NewLocalizationManager(".")
		if err != nil {
			return nil, fmt.Errorf("failed to initialize localization manager: %v", err)
		}
	}
	
	return &UIService{
		LocManager: locManager,
	}, nil
}


// ShowMainMenu displays the main interactive menu when Please is run without arguments
func ShowMainMenu() {
	// Create UI service - in production this would come from main
	configDir, _ := getConfigDir()
	uiService, err := NewUIService(configDir)
	if err != nil {
		// Fallback to current directory if config dir fails
		mgr, _ := localization.NewLocalizationManager(".")
		uiService = &UIService{LocManager: mgr}
	}
	
	uiService.ShowMainMenuWithService()
}

// ShowMainMenuWithService displays the main interactive menu using dependency injection
func (ui *UIService) ShowMainMenuWithService() {
	// Show banner
	fmt.Printf("╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                           🤖 Please Script Generator                         ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")
	items := []MenuItem{
		{Label: ui.LocManager.System.Get("menus.show_help"), Icon: "📖", Color: ColorCyan, Action: func() bool { ShowHelp(); return false }},
		{Label: ui.LocManager.System.Get("menus.generate_script"), Icon: "✨", Color: ColorYellow, Action: func() bool { generateNewScript(); return false }},
		{Label: ui.LocManager.System.Get("menus.load_last"), Icon: "🔄", Color: ColorMagenta, Action: func() bool { loadLastScript(); return false }},
		{Label: ui.LocManager.System.Get("menus.browse_history"), Icon: "📚", Color: ColorBlue, Action: func() bool { browseHistory(); return false }},
		{Label: ui.LocManager.System.Get("menus.show_config"), Icon: "⚙️ ", Color: ColorPurple, Action: func() bool { showConfiguration(); return false }},
		{Label: ui.LocManager.System.Get("menus.exit"), Icon: "🚪", Color: ColorDim, Action: func() bool {
			fmt.Printf("%s%s%s\n", ColorGreen, ui.LocManager.System.Get("success.exit"), ColorReset)
			return true
		}},
	}
	renderMenu(ui.LocManager.System.Get("menus.main_prompt"), "Press 1-6: ", items, nil)
}

// handleMainMenuChoice processes the main menu selection and returns true if should exit
func handleMainMenuChoice(choice string) bool {
	// Create localization manager for this function
	locManager, _ := localization.NewLocalizationManager(".")

	// Handle Enter key as immediate exit
	if choice == "\r" || choice == "\n" {
		fmt.Printf("%s%s%s\n", ColorGreen, locManager.System.Get("success.exit_quick"), ColorReset)
		return true // Exit immediately on Enter
	}

	// Handle other special characters that should be ignored
	if len(choice) == 0 || choice == " " {
		return false // Ignore empty or space - continue showing menu
	}

	actions := map[string]func() bool{
		"1": func() bool { ShowHelp(); return false },
		"2": func() bool { generateNewScript(); return false },
		"3": func() bool { loadLastScript(); return false },
		"4": func() bool { browseHistory(); return false },
		"5": func() bool { showConfiguration(); return false },
		"6": func() bool {
			fmt.Printf("%s%s%s\n", ColorGreen, locManager.System.Get("success.exit"), ColorReset)
			return true
		},
	}

	if action, ok := actions[choice]; ok {
		return action()
	}

	fmt.Printf("%s%s%s\n", ColorRed, locManager.System.Get("errors.invalid_choice"), ColorReset)
	return false // Continue showing main menu
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

	if pleaseProvider != "" {
		fmt.Printf("  %s• PLEASE_PROVIDER:%s %s\n", ColorDim, ColorReset, pleaseProvider)
	} else {
		fmt.Printf("  %s• PLEASE_PROVIDER:%s ollama (default)\n", ColorDim, ColorReset)
	}

	fmt.Printf("\n%s💡 Tip: Set PLEASE_PROVIDER environment variable to change default provider%s\n", ColorDim, ColorReset)
}

// ShowScriptMenu displays an interactive menu after script generation
func ShowScriptMenu(response *types.ScriptResponse) {
	items := []MenuItem{
		{Label: "Copy to clipboard", Icon: "📋", Color: ColorCyan, Action: func() bool { copyToClipboard(response); return false }},
		{Label: "Execute script now", Icon: "▶️ ", Color: ColorYellow, Action: func() bool { executeScript(response); return false }},
		{Label: "Save to file", Icon: "💾", Color: ColorBlue, Action: func() bool { saveToFile(response); return false }},
		{Label: "Edit script", Icon: "✏️ ", Color: ColorPurple, Action: func() bool { editScript(response); return false }},
		{Label: "Refine script with AI", Icon: "🧠", Color: ColorMagenta, Action: func() bool { refineScript(response); return false }},
		{Label: "Show detailed explanation", Icon: "📖", Color: ColorWhite, Action: func() bool { showDetailedExplanation(response); return false }},
		{Label: "Load last script", Icon: "🔄", Color: ColorMagenta, Action: func() bool { loadLastScript(); return false }},
		{Label: "Exit", Icon: "🚪", Color: ColorDim, Action: func() bool {
			fmt.Printf("%s✨ Ta-da! Thanks for using Please! Happy scripting! 🎉%s\n", ColorGreen, ColorReset)
			return true
		}},
	}
	renderMenu("🎯 What would you like to do with this script?", "Press 1-8: ", items, nil)
}

// renderMenu displays a menu from a slice of MenuItem and handles user input
// getInput allows injection of input behaviour for testability; if nil, uses getSingleKeyInput
func renderMenu(title, prompt string, items []MenuItem, getInput func() rune) {
	if getInput == nil {
		getInput = getSingleKeyInput
	}
	for {
		fmt.Printf("\n%s%s%s\n\n", ColorBold+ColorCyan, title, ColorReset)
		for idx, item := range items {
			fmt.Printf("  %s%d.%s %s%s %s%s\n", ColorGreen, idx+1, ColorReset, item.Color, item.Icon, item.Label, ColorReset)
		}
		fmt.Printf("\n%s%s%s", ColorBold+ColorYellow, prompt, ColorReset)
		choice := getInput()
		fmt.Printf("%c\n", choice)
		if choice == '\r' || choice == '\n' {
			fmt.Printf("%s✨ Quick exit!%s\n", ColorGreen, ColorReset)
			return
		}
		if choice >= '1' && int(choice-'1') < len(items) {
			shouldExit := items[int(choice-'1')].Action()
			if shouldExit {
				return
			}
		} else {
			fmt.Printf("%s❌ Invalid choice. Please try again.%s\n", ColorRed, ColorReset)
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
	items := []MenuItem{
		{Label: "Open in external editor (recommended)", Icon: "", Color: ColorCyan, Action: func() bool {
			if editedResponse, err := script.EditScript(response); err != nil {
				fmt.Printf("%s❌ Editor failed: %v%s\n", ColorRed, err, ColorReset)
				fmt.Printf("%s💡 Tip: Set EDITOR environment variable or install VS Code%s\n", ColorDim, ColorReset)
			} else if editedResponse != response {
				*response = *editedResponse
				fmt.Printf("%s🎯 Updated script is now active in the menu%s\n", ColorGreen, ColorReset)
			}
			return true
		}},
		{Label: "Inline editing (line-by-line)", Icon: "🔤", Color: ColorBlue, Action: func() bool {
			if editedResponse, err := script.OfferInlineEditing(response); err != nil {
				fmt.Printf("%s❌ Inline editing failed: %v%s\n", ColorRed, err, ColorReset)
			} else if editedResponse != response {
				*response = *editedResponse
				fmt.Printf("%s🎯 Updated script is now active in the menu%s\n", ColorGreen, ColorReset)
			}
			return true
		}},
		{Label: "Cancel editing", Icon: "🚫", Color: ColorDim, Action: func() bool { fmt.Printf("%s🚫 Editing cancelled%s\n", ColorYellow, ColorReset); return true }},
	}
	renderMenu("✏️  Edit Script", "Press 1-3: ", items, nil)
}

// Stub for refineScript to allow menu to compile and work
func refineScript(response *types.ScriptResponse) {
	fmt.Printf("%s🧠 Refine script with AI feature coming soon!%s\n", ColorMagenta, ColorReset)
}

// showDetailedExplanation shows a detailed breakdown of the script
func showDetailedExplanation(response *types.ScriptResponse) {
	fmt.Printf("\n%s📖 Detailed Script Explanation%s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("%s%s%s\n\n", ColorCyan, strings.Repeat("═", 50), ColorReset)

	fmt.Printf("%s🎯 Task Analysis:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s• Original request:%s %s\n", ColorDim, ColorReset, response.TaskDescription)
	fmt.Printf("  %s• Script type:%s %s\n", ColorDim, ColorReset, response.ScriptType)
	fmt.Printf("  %s• AI model used:%s %s (%s)\n", ColorDim, ColorReset, response.Model, response.Provider)

	fmt.Printf("\n%s🔍 Script Analysis:%s\n", ColorBold+ColorYellow, ColorReset)

	lines := strings.Split(response.Script, "\n")
	commentCount, commandCount := 0, 0

	for _, line := range lines {
		trimmed := strings.TrimSpace(line)
		if trimmed == "" {
			continue
		}
		if strings.HasPrefix(trimmed, "#") {
			commentCount++
		} else {
			commandCount++
		}
	}

	totalLines := len(lines)
	fmt.Printf("  %s• Total lines:%s %d\n", ColorDim, ColorReset, totalLines)
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

	fmt.Printf("\n%sScript Preview:%s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("─", 50), ColorReset)
	for i, line := range lines {
		fmt.Printf("%s%3d│%s %s\n", ColorDim, i+1, ColorReset, line)
	}
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("─", 50), ColorReset)
}

// saveToHistory saves the executed script to history log
func saveToHistory(response *types.ScriptResponse) {
	configDir, err := getConfigDir()
	if err != nil {
		return // Silently fail
	}

	historyPath := filepath.Join(configDir, "script_history.json")
	
	// Create new history entry with timestamp
	timestamp := fmt.Sprintf("%d", time.Now().Unix())
	historyEntry := fmt.Sprintf(`{
  "timestamp": "%s",
  "task_description": "%s",
  "script": "%s",
  "script_type": "%s",
  "model": "%s",
  "provider": "%s"
}`,
		timestamp,
		strings.ReplaceAll(response.TaskDescription, `"`, `\"`),
		strings.ReplaceAll(response.Script, `"`, `\"`),
		response.ScriptType,
		response.Model,
		response.Provider)

	// Load existing history or create new
	var historyContent string
	if data, err := os.ReadFile(historyPath); err == nil {
		existingHistory := strings.TrimSpace(string(data))
		if existingHistory == "" || existingHistory == "[]" {
			historyContent = fmt.Sprintf("[\n%s\n]", historyEntry)
		} else {
			// Remove closing bracket and add new entry
			existingHistory = strings.TrimSuffix(existingHistory, "]")
			historyContent = fmt.Sprintf("%s,\n%s\n]", existingHistory, historyEntry)
		}
	} else {
		// Create new history file
		historyContent = fmt.Sprintf("[\n%s\n]", historyEntry)
	}

	os.WriteFile(historyPath, []byte(historyContent), 0644)
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

	// Find the end quote, handling escaped quotes
	pos := start
	for pos < len(content) {
		if content[pos] == '"' {
			// Check if this quote is escaped
			if pos > 0 && content[pos-1] == '\\' {
				// Count consecutive backslashes to determine if quote is actually escaped
				backslashCount := 0
				for i := pos - 1; i >= 0 && content[i] == '\\'; i-- {
					backslashCount++
				}
				// If odd number of backslashes, the quote is escaped
				if backslashCount%2 == 1 {
					pos++
					continue
				}
			}
			// Found unescaped closing quote
			break
		}
		pos++
	}

	if pos >= len(content) {
		return ""
	}

	value := content[start:pos]
	// Unescape quotes and backslashes
	value = strings.ReplaceAll(value, `\"`, `"`)
	value = strings.ReplaceAll(value, `\\`, `\`)
	return value
}

// Show clear success/failure summaries and actionable suggestions after auto-fix
func tryAutoFix(originalResponse *types.ScriptResponse, errorMessage string) {
	cfg, err := config.Load()
	if err != nil {
		printAutoFixError(err, originalResponse)
		return
	}

	stopProgress := ShowProviderProgress(originalResponse.Provider, "Auto-fixing script")
	defer stopProgress()

	fixedScript, err := providers.GenerateFixedScript(
		originalResponse.Script,
		errorMessage,
		originalResponse.ScriptType,
		originalResponse.Model,
		originalResponse.Provider,
		cfg,
	)

	stopProgress()

	if err != nil {
		printAutoFixError(err, originalResponse)
		return
	}
	fixedResponse := &types.ScriptResponse{
		TaskDescription: "Auto-fix for: " + originalResponse.TaskDescription,
		Script:          fixedScript,
		ScriptType:      originalResponse.ScriptType,
		Model:           originalResponse.Model,
		Provider:        originalResponse.Provider,
	}

	printAutoFixSuccess(fixedResponse)

	if err := script.ExecuteScript(fixedResponse); err != nil {
		printAutoFixError(err, fixedResponse)
	} else {
		fmt.Printf("%s✅ Fixed script executed successfully!%s\n", ColorGreen, ColorReset)
		originalResponse.Script = fixedResponse.Script
		originalResponse.TaskDescription = fixedResponse.TaskDescription
	}
}

// Print auto-fix error and suggestions, then show next-step menu
func printAutoFixError(err error, response *types.ScriptResponse) {
	fmt.Printf("%s❌ Auto-fix failed: %v%s\n", ColorRed, err, ColorReset)
	fmt.Printf("%s💡 Suggestions:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  • Try editing the script manually\n")
	fmt.Printf("  • Ask AI to explain the error\n")
	fmt.Printf("  • View documentation or help\n")
	showPostActionMenu(response)
}

// Print auto-fix success and show the fixed script
func printAutoFixSuccess(fixedResponse *types.ScriptResponse) {
	fmt.Printf("\n%s✨ Auto-fix applied. Please review the new script below.%s\n", ColorGreen, ColorReset)
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("─", 60), ColorReset)
	lines := strings.Split(fixedResponse.Script, "\n")
	for i, line := range lines {
		fmt.Printf("%s%3d│%s %s\n", ColorDim, i+1, ColorReset, line)
	}
	fmt.Printf("%s%s%s\n", ColorDim, strings.Repeat("─", 60), ColorReset)
	fmt.Printf("%s🚀 Testing fixed script...%s\n", ColorGreen, ColorReset)
}

// Show a next-step menu after auto-fix or error
func showPostActionMenu(response *types.ScriptResponse) {
	items := []MenuItem{
		{Label: "Retry", Icon: "", Color: ColorGreen, Action: func() bool { executeScript(response); return true }},
		{Label: "Edit script", Icon: "", Color: ColorGreen, Action: func() bool { editScript(response); return true }},
		{Label: "Return to main menu", Icon: "", Color: ColorGreen, Action: func() bool { ShowMainMenu(); return true }},
		{Label: "Exit", Icon: "", Color: ColorGreen, Action: func() bool { fmt.Printf("%s✨ Goodbye!%s\n", ColorGreen, ColorReset); os.Exit(0); return true }},
	}
	renderMenu("What would you like to do next?", "Press 1-4: ", items, nil)
}
