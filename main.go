package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"please/config"
	"please/localization"
	"please/models"
	"please/providers"
	"please/script"
	"please/types"
	"please/ui"
)

func main() {
	lang := "en-us"
	theme := "default"
	for _, arg := range os.Args[1:] {
		if strings.HasPrefix(arg, "--language=") {
			lang = strings.SplitN(arg, "=", 2)[1]
		}
		if strings.HasPrefix(arg, "--theme=") {
			theme = strings.SplitN(arg, "=", 2)[1]
		}
	}

	locMgr, _ := localization.NewLocalizationManager(".")
	locMgr.LoadLanguage(lang, filepath.Join("themes", lang+".json"))
	themeData := types.Theme{Colors: map[string]string{"primary": "#00ff41"}}
	locMgr.LoadTheme(theme, themeData)
	locMgr.SetLanguage(lang)
	locMgr.SetTheme(theme)
	ui.SetLocalizationManager(locMgr)
	ui.SetLocalizationManagerForHelp(locMgr)

	// Check if we're being run as "pls" with special flags
	programName := filepath.Base(os.Args[0])
	if programName == "pls" || programName == "pls.exe" {
		// Running as the short alias
	}

	// Handle special commands
	if len(os.Args) >= 2 {
		switch os.Args[1] {
		case "--install-alias":
			installAlias()
			return
		case "--uninstall-alias":
			uninstallAlias()
			return
		case "--version":
			ui.ShowVersion()
			return
		case "--help", "-h":
			ui.ShowHelp()
			return
		case "--test-monitor", "--monitor-tests":
			runTestMonitor()
			return
		}
	}

	// If no arguments provided, show interactive main menu
	if len(os.Args) < 2 {
		ui.ShowMainMenu()
		return
	}

	// Join all arguments after the program name as the task description
	taskDescription := strings.Join(os.Args[1:], " ")

	// Check for natural language history/last script commands
	if isLastScriptCommand(taskDescription) {
		ui.RunLastScriptFromCLI()
		return
	}

	// Load configuration
	cfg, err := config.Load()
	if err != nil {
		// Create default config if none exists
		cfg = config.CreateDefault()
		config.Save(cfg) // Ignore errors for config saving
	}

	// Determine script type and provider
	scriptType := config.DetermineScriptType(cfg)
	provider := config.DetermineProvider(cfg)

	// Select the best model for the task
	model, err := models.SelectBestModel(cfg, taskDescription, provider)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Warning: Could not auto-select model (%v), using fallback\n", err)
		// Use fallback based on provider
		model = getFallbackModel(provider)
	}

	// Create the script request
	request := &types.ScriptRequest{
		TaskDescription: taskDescription,
		ScriptType:      scriptType,
		Provider:        provider,
		Model:           model,
	}

	// Generate script using the appropriate provider
	response, err := generateScript(cfg, request)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error: %v\n", err)
		os.Exit(1)
	}

	// Display the script with explanation and ask for confirmation
	displayScriptAndConfirm(response)
}

// generateScript creates a script using the appropriate provider
func generateScript(cfg *types.Config, request *types.ScriptRequest) (*types.ScriptResponse, error) {
	var provider providers.Provider

	switch request.Provider {
	case "ollama":
		provider = providers.NewOllamaProvider(cfg)
	case "openai":
		provider = providers.NewOpenAIProvider(cfg)
	case "anthropic":
		provider = providers.NewAnthropicProvider(cfg)
	default:
		return nil, fmt.Errorf("unsupported provider: %s", request.Provider)
	}

	if !provider.IsConfigured(cfg) {
		return nil, fmt.Errorf("provider %s is not properly configured", request.Provider)
	}

	// Show progress indication during script generation
	stopProgress := ui.ShowProviderProgress(request.Provider, "Generating script")
	defer stopProgress()

	return provider.GenerateScript(request)
}

// getFallbackModel returns a fallback model based on provider
func getFallbackModel(provider string) string {
	switch provider {
	case "openai":
		return "gpt-3.5-turbo"
	case "anthropic":
		return "claude-3-haiku-20240307"
	default:
		return "llama3.2"
	}
}

// displayScriptAndConfirm shows the generated script with explanation and interactive menu
func displayScriptAndConfirm(response *types.ScriptResponse) {
	fmt.Printf("╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                           🤖 Please Script Generator                         ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	// Get localized labels, with fallbacks for missing translations
	taskLabel := ui.GetLocalizedMessage("script_display.task_label")
	if taskLabel == "" {
		taskLabel = "📝 Task:"
	}
	
	modelLabel := ui.GetLocalizedMessage("script_display.model_label")
	if modelLabel == "" {
		modelLabel = "🧠 Model:"
	}
	
	platformLabel := ui.GetLocalizedMessage("script_display.platform_label")
	if platformLabel == "" {
		platformLabel = "🖥️ Platform:"
	}

	scriptHeader := ui.GetLocalizedMessage("script_display.script_header")
	if scriptHeader == "" {
		scriptHeader = "📋 Generated Script"
	}

	successMessage := ui.GetLocalizedMessage("script_display.success_message")
	if successMessage == "" {
		successMessage = "✅ Script generated successfully!"
	}

	fmt.Printf("%s %s\n", taskLabel, response.TaskDescription)
	fmt.Printf("%s %s (%s)\n", modelLabel, response.Model, response.Provider)
	fmt.Printf("%s %s script\n", platformLabel, response.ScriptType)

	fmt.Printf("\n╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                              %s                             ║\n", scriptHeader)
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	// Display the script with line numbers
	lines := strings.Split(response.Script, "\n")
	for i, line := range lines {
		lineNum := fmt.Sprintf("%3d", i+1)
		fmt.Printf("\033[90m%s│\033[0m %s\n", lineNum, line)
	}

	fmt.Printf("\n%s\n", successMessage)

	// Show interactive menu
	ui.ShowScriptMenu(response)
}

// installAlias creates the "pls" shortcut for the current platform
func installAlias() {
	ui.PrintRainbowBanner()
	fmt.Printf("\n%s🔧 Installing 'pls' alias...%s\n\n", ui.ColorBold+ui.ColorYellow, ui.ColorReset)

	// Get current executable path
	execPath, err := os.Executable()
	if err != nil {
		fmt.Printf("%s❌ Failed to get executable path: %v%s\n", ui.ColorRed, err, ui.ColorReset)
		return
	}

	dir := filepath.Dir(execPath)
	batContent := fmt.Sprintf(`@echo off
"%s" %%*
`, execPath)

	// Create pls.bat as the primary alias
	plsBatPath := filepath.Join(dir, "pls.bat")
	if err := os.WriteFile(plsBatPath, []byte(batContent), 0755); err != nil {
		fmt.Printf("%s❌ Failed to create pls.bat: %v%s\n", ui.ColorRed, err, ui.ColorReset)
		return
	}

	fmt.Printf("%s✅ Successfully created pls.bat!%s\n\n", ui.ColorGreen, ui.ColorReset)
	ui.PrintInstallationSuccess()
}

// uninstallAlias removes "pls" shortcut
func uninstallAlias() {
	ui.PrintRainbowBanner()
	fmt.Printf("\n%s🗑️  Removing aliases...%s\n\n", ui.ColorBold+ui.ColorYellow, ui.ColorReset)

	// Look for aliases in the same directory as the executable
	execPath, err := os.Executable()
	if err != nil {
		fmt.Printf("%s❌ Failed to get executable path: %v%s\n", ui.ColorRed, err, ui.ColorReset)
		return
	}

	dir := filepath.Dir(execPath)

	// Remove pls.bat
	plsBatPath := filepath.Join(dir, "pls.bat")
	if err := os.Remove(plsBatPath); err != nil {
		if os.IsNotExist(err) {
			fmt.Printf("%s💭 pls.bat not found%s\n", ui.ColorYellow, ui.ColorReset)
		} else {
			fmt.Printf("%s❌ Failed to remove pls.bat: %v%s\n", ui.ColorRed, err, ui.ColorReset)
		}
	} else {
		fmt.Printf("%s✅ Successfully removed pls.bat%s\n", ui.ColorGreen, ui.ColorReset)
	}
}

// isLastScriptCommand checks if the command is requesting to run the last script
func isLastScriptCommand(command string) bool {
	lower := strings.ToLower(command)

	// Check for various natural language patterns
	lastScriptPatterns := []string{
		"run my last script",
		"run last script",
		"execute my last script",
		"execute last script",
		"run the last script",
		"execute the last script",
		"run my previous script",
		"run previous script",
		"run last",
		"last script",
		"previous script",
		"run again",
		"do it again",
		"repeat last",
		"repeat",
	}

	for _, pattern := range lastScriptPatterns {
		if strings.Contains(lower, pattern) {
			return true
		}
	}

	return false
}

// runTestMonitor executes AI-powered test monitoring
func runTestMonitor() {
	// Load configuration
	cfg, err := config.Load()
	if err != nil {
		// Create default config if none exists
		cfg = config.CreateDefault()
		config.Save(cfg) // Ignore errors for config saving
	}

	// Determine provider
	providerName := config.DetermineProvider(cfg)

	// Create the appropriate provider
	var provider providers.Provider
	switch providerName {
	case "ollama":
		provider = providers.NewOllamaProvider(cfg)
	case "openai":
		provider = providers.NewOpenAIProvider(cfg)
	case "anthropic":
		provider = providers.NewAnthropicProvider(cfg)
	default:
		fmt.Fprintf(os.Stderr, "Error: Unsupported provider: %s\n", providerName)
		os.Exit(1)
	}

	if !provider.IsConfigured(cfg) {
		fmt.Fprintf(os.Stderr, "Error: Provider %s is not properly configured\n", providerName)
		os.Exit(1)
	}

	// Parse test pattern from command line arguments
	testPattern := ""
	if len(os.Args) >= 3 {
		testPattern = os.Args[2]
	}

	// Run the test monitor
	if err := script.RunMonitoredTests(provider, cfg, testPattern); err != nil {
		fmt.Fprintf(os.Stderr, "Error running monitored tests: %v\n", err)
		os.Exit(1)
	}
}
