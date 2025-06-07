package ui

import (
	"bufio"
	"fmt"
	"os"
	"strings"

	"oohlama/script"
	"oohlama/types"
)

// ShowScriptMenu displays an interactive menu after script generation
func ShowScriptMenu(response *types.ScriptResponse) {
	fmt.Printf("\n%s🎯 What would you like to do with this script?%s\n\n", ColorBold+ColorCyan, ColorReset)

	// Show menu options
	fmt.Printf("  %s1.%s %s📋 Copy to clipboard%s\n", ColorGreen, ColorReset, ColorCyan, ColorReset)
	fmt.Printf("  %s2.%s %s▶️  Execute script now%s\n", ColorGreen, ColorReset, ColorYellow, ColorReset)
	fmt.Printf("  %s3.%s %s💾 Save to file%s\n", ColorGreen, ColorReset, ColorBlue, ColorReset)
	fmt.Printf("  %s4.%s %s✏️  Edit script%s\n", ColorGreen, ColorReset, ColorPurple, ColorReset)
	fmt.Printf("  %s5.%s %s📖 Show detailed explanation%s\n", ColorGreen, ColorReset, ColorWhite, ColorReset)
	fmt.Printf("  %s6.%s %s🚪 Exit%s\n\n", ColorGreen, ColorReset, ColorDim, ColorReset)

	// Get user choice
	choice := getUserChoice()
	handleUserChoice(choice, response)
}

// getUserChoice prompts for and returns user input
func getUserChoice() string {
	fmt.Printf("%sEnter your choice (1-6):%s ", ColorBold+ColorYellow, ColorReset)
	
	reader := bufio.NewReader(os.Stdin)
	input, err := reader.ReadString('\n')
	if err != nil {
		fmt.Printf("%s❌ Error reading input: %v%s\n", ColorRed, err, ColorReset)
		return "6" // Default to exit on error
	}

	return strings.TrimSpace(input)
}

// handleUserChoice processes the user's menu selection
func handleUserChoice(choice string, response *types.ScriptResponse) {
	switch choice {
	case "1":
		copyToClipboard(response)
	case "2":
		executeScript(response)
	case "3":
		saveToFile(response)
	case "4":
		editScript(response)
	case "5":
		showDetailedExplanation(response)
	case "6":
		fmt.Printf("%s👋 Thanks for using OohLama! Happy scripting!%s\n", ColorGreen, ColorReset)
		return
	default:
		fmt.Printf("%s❌ Invalid choice. Please try again.%s\n\n", ColorRed, ColorReset)
		ShowScriptMenu(response) // Show menu again
	}

	// After most actions, ask if they want to do something else
	if choice != "6" {
		fmt.Printf("\n%s💭 Would you like to do something else with this script? (y/n):%s ", ColorYellow, ColorReset)
		reader := bufio.NewReader(os.Stdin)
		input, _ := reader.ReadString('\n')
		if strings.ToLower(strings.TrimSpace(input)) == "y" {
			ShowScriptMenu(response)
		} else {
			fmt.Printf("%s👋 Thanks for using OohLama! Happy scripting!%s\n", ColorGreen, ColorReset)
		}
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

// executeScript executes the script with safety confirmation
func executeScript(response *types.ScriptResponse) {
	// Show script validation warnings if any
	warnings := script.ValidateScript(response)
	if len(warnings) > 0 {
		fmt.Printf("%s⚠️  Script Validation Warnings:%s\n", ColorYellow, ColorReset)
		for _, warning := range warnings {
			fmt.Printf("  %s%s%s\n", ColorYellow, warning, ColorReset)
		}
		fmt.Println()
	}

	fmt.Printf("%s⚠️  About to execute script...%s\n", ColorYellow, ColorReset)
	fmt.Printf("%s🛡️  Safety Warning: Review the script above before proceeding!%s\n", ColorRed, ColorReset)
	fmt.Printf("%s❓ Are you sure you want to execute this script? (yes/no):%s ", ColorBold+ColorYellow, ColorReset)

	reader := bufio.NewReader(os.Stdin)
	input, _ := reader.ReadString('\n')
	
	if strings.ToLower(strings.TrimSpace(input)) == "yes" {
		fmt.Printf("%s▶️  Executing script...%s\n", ColorGreen, ColorReset)
		if err := script.ExecuteScript(response); err != nil {
			fmt.Printf("%s❌ Script execution failed: %v%s\n", ColorRed, err, ColorReset)
		} else {
			fmt.Printf("%s✅ Script execution completed!%s\n", ColorGreen, ColorReset)
		}
	} else {
		fmt.Printf("%s🚫 Script execution cancelled for safety.%s\n", ColorYellow, ColorReset)
	}
}

// saveToFile saves the script to a file
func saveToFile(response *types.ScriptResponse) {
	fmt.Printf("%s💾 Saving script to file...%s\n", ColorBlue, ColorReset)
	
	// Get suggested filename from script package
	defaultFilename := script.GetSuggestedFilename(response)
	fmt.Printf("%sEnter filename (press Enter for '%s'):%s ", ColorYellow, defaultFilename, ColorReset)
	
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
