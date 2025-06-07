package main

import (
	"bufio"
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"time"
)

type OllamaRequest struct {
	Model   string                 `json:"model"`
	Prompt  string                 `json:"prompt"`
	Stream  bool                   `json:"stream"`
	Options map[string]interface{} `json:"options"`
}

type OllamaResponse struct {
	Response string `json:"response"`
}

type ModelInfo struct {
	Name       string    `json:"name"`
	ModifiedAt time.Time `json:"modified_at"`
	Size       int64     `json:"size"`
	Digest     string    `json:"digest"`
	Details    struct {
		Format string `json:"format"`
		Family string `json:"family"`
	} `json:"details"`
}

type ModelsResponse struct {
	Models []ModelInfo `json:"models"`
}

type Config struct {
	PreferredModel string            `json:"preferred_model"`
	ModelOverrides map[string]string `json:"model_overrides"`
}

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: oohlama.exe <task description>")
		fmt.Println("Example: oohlama.exe \"list all files in the current directory\"")
		os.Exit(1)
	}

	// Join all arguments after the program name as the task description
	taskDescription := strings.Join(os.Args[1:], " ")

	// Default Ollama settings
	ollamaURL := "http://localhost:11434"

	// Check for environment variables
	if url := os.Getenv("OLLAMA_URL"); url != "" {
		ollamaURL = url
	}

	// Determine the best model to use
	model, err := selectBestModel(ollamaURL, taskDescription)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Warning: Could not auto-select model (%v), using fallback\n", err)
		model = "llama3.2"

		// Check for manual override
		if m := os.Getenv("OLLAMA_MODEL"); m != "" {
			model = m
		}
	}

	// Generate PowerShell script
	script, err := generatePowerShellScript(ollamaURL, model, taskDescription)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error: %v\n", err)
		os.Exit(1)
	}

	// Display the script with explanation and ask for confirmation
	displayScriptAndConfirm(script, model, taskDescription)
}

func generatePowerShellScript(baseURL, model, taskDescription string) (string, error) {
	prompt := fmt.Sprintf(`You are a PowerShell expert. Generate a complete, working PowerShell script to accomplish the following task:

%s

Requirements:
- Write clean, well-commented PowerShell code
- Include error handling where appropriate
- Use PowerShell best practices
- Do NOT include markdown code blocks, backticks, or formatting
- Do NOT include explanations or descriptions
- Return ONLY the raw PowerShell script code
- The script should be ready to run as-is
- Start directly with PowerShell commands, no preamble

PowerShell Script:`, taskDescription)

	request := OllamaRequest{
		Model:  model,
		Prompt: prompt,
		Stream: false,
		Options: map[string]interface{}{
			"temperature": 0.3,
			"top_p":       0.9,
		},
	}

	// Convert to JSON
	jsonData, err := json.Marshal(request)
	if err != nil {
		return "", fmt.Errorf("failed to marshal request: %v", err)
	}

	// Create HTTP client with timeout
	client := &http.Client{
		Timeout: 120 * time.Second,
	}

	// Make request to Ollama
	resp, err := client.Post(baseURL+"/api/generate", "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		return "", fmt.Errorf("failed to connect to Ollama at %s: %v\nMake sure Ollama is running", baseURL, err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return "", fmt.Errorf("Ollama API returned status %d: %s", resp.StatusCode, string(body))
	}

	// Parse response
	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", fmt.Errorf("failed to read response: %v", err)
	}

	var ollamaResp OllamaResponse
	if err := json.Unmarshal(body, &ollamaResp); err != nil {
		return "", fmt.Errorf("failed to parse response: %v", err)
	}

	script := strings.TrimSpace(ollamaResp.Response)
	
	// Clean the script of any markdown formatting
	script = cleanScript(script)
	
	return script, nil
}

// cleanScript removes markdown formatting and other unwanted content from the generated script
func cleanScript(script string) string {
	lines := strings.Split(script, "\n")
	cleanedLines := []string{}
	
	for _, line := range lines {
		// Skip markdown code block markers
		if strings.HasPrefix(strings.TrimSpace(line), "```") {
			continue
		}
		
		// Skip empty explanatory lines that might have been added
		trimmed := strings.TrimSpace(line)
		if trimmed == "" || 
		   strings.HasPrefix(trimmed, "Here's a PowerShell script") ||
		   strings.HasPrefix(trimmed, "This script will") ||
		   strings.HasPrefix(trimmed, "The following script") {
			continue
		}
		
		cleanedLines = append(cleanedLines, line)
	}
	
	return strings.Join(cleanedLines, "\n")
}

// selectBestModel automatically chooses the most appropriate model for the given task
func selectBestModel(baseURL, taskDescription string) (string, error) {
	// Load user configuration
	config, err := loadConfig()
	if err != nil {
		// Config doesn't exist yet, will create default
		config = &Config{
			ModelOverrides: make(map[string]string),
		}
	}

	// Check if user has manually overridden via environment
	if m := os.Getenv("OLLAMA_MODEL"); m != "" {
		return m, nil
	}

	// Check for task-specific overrides in config
	taskType := categorizeTask(taskDescription)
	if override, exists := config.ModelOverrides[taskType]; exists {
		return override, nil
	}

	// Get available models from Ollama
	models, err := getAvailableModels(baseURL)
	if err != nil {
		return "", fmt.Errorf("failed to get available models: %v", err)
	}

	if len(models) == 0 {
		return "", fmt.Errorf("no models available in Ollama")
	}

	// Rank models by suitability for the task
	bestModel := rankModels(models, taskDescription, taskType)

	// Update config with the selected model if we don't have a preferred one
	if config.PreferredModel == "" {
		config.PreferredModel = bestModel
		saveConfig(config) // Ignore errors for config saving
	}

	return bestModel, nil
}

// getAvailableModels queries Ollama for all available models
func getAvailableModels(baseURL string) ([]ModelInfo, error) {
	client := &http.Client{Timeout: 10 * time.Second}

	resp, err := client.Get(baseURL + "/api/tags")
	if err != nil {
		return nil, fmt.Errorf("failed to connect to Ollama: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("Ollama API returned status %d: %s", resp.StatusCode, string(body))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %v", err)
	}

	var modelsResp ModelsResponse
	if err := json.Unmarshal(body, &modelsResp); err != nil {
		return nil, fmt.Errorf("failed to parse models response: %v", err)
	}

	return modelsResp.Models, nil
}

// categorizeTask determines the type of task to help with model selection
func categorizeTask(description string) string {
	desc := strings.ToLower(description)

	// Code-related tasks
	if strings.Contains(desc, "script") || strings.Contains(desc, "function") ||
		strings.Contains(desc, "code") || strings.Contains(desc, "program") {
		return "coding"
	}

	// System administration tasks
	if strings.Contains(desc, "system") || strings.Contains(desc, "server") ||
		strings.Contains(desc, "service") || strings.Contains(desc, "process") ||
		strings.Contains(desc, "registry") || strings.Contains(desc, "install") {
		return "sysadmin"
	}

	// File management tasks
	if strings.Contains(desc, "file") || strings.Contains(desc, "folder") ||
		strings.Contains(desc, "directory") || strings.Contains(desc, "copy") ||
		strings.Contains(desc, "move") || strings.Contains(desc, "delete") {
		return "filemanagement"
	}

	// Network/web related tasks
	if strings.Contains(desc, "web") || strings.Contains(desc, "http") ||
		strings.Contains(desc, "url") || strings.Contains(desc, "download") ||
		strings.Contains(desc, "network") || strings.Contains(desc, "api") {
		return "network"
	}

	return "general"
}

// rankModels selects the best model based on task type and model capabilities
func rankModels(models []ModelInfo, taskDescription, taskType string) string {
	// Model preference ranking based on PowerShell script generation capability
	modelPriority := map[string]int{
		"codegemma":      100, // Specialized for code generation
		"codellama":      95,  // Code-focused model
		"deepseek-coder": 90,  // Another code-focused model
		"llama3.1":       85,  // Latest general model with good coding
		"llama3.2":       80,  // Good general model
		"llama3":         75,  // Older but reliable
		"qwen2.5-coder":  85,  // Code-focused
		"phi3":           70,  // Smaller but capable
		"mistral":        65,  // Good general model
		"gemma2":         60,  // Decent general model
	}

	// Boost priority for code-related tasks
	if taskType == "coding" {
		for name := range modelPriority {
			if strings.Contains(strings.ToLower(name), "code") {
				modelPriority[name] += 20
			}
		}
	}

	bestModel := ""
	bestScore := -1

	for _, model := range models {
		score := 0
		modelName := strings.ToLower(model.Name)

		// Check for exact matches in our priority list
		for priorityModel, priorityScore := range modelPriority {
			if strings.Contains(modelName, priorityModel) {
				score = priorityScore
				break
			}
		}

		// If no specific match, give a base score
		if score == 0 {
			score = 50
		}

		// Prefer larger models (generally more capable)
		if model.Size > 7000000000 { // > 7GB
			score += 10
		} else if model.Size > 4000000000 { // > 4GB
			score += 5
		}

		// Prefer newer models (more recent modified date)
		if time.Since(model.ModifiedAt) < 30*24*time.Hour { // Less than 30 days old
			score += 5
		}

		if score > bestScore {
			bestScore = score
			bestModel = model.Name
		}
	}

	// Fallback if no model found
	if bestModel == "" && len(models) > 0 {
		bestModel = models[0].Name
	}

	return bestModel
}

// getConfigPath returns the path to the configuration file in AppData
func getConfigPath() (string, error) {
	appData := os.Getenv("APPDATA")
	if appData == "" {
		return "", fmt.Errorf("APPDATA environment variable not set")
	}

	configDir := filepath.Join(appData, "oohlama")

	// Create directory if it doesn't exist
	if err := os.MkdirAll(configDir, 0755); err != nil {
		return "", fmt.Errorf("failed to create config directory: %v", err)
	}

	return filepath.Join(configDir, "config.json"), nil
}

// loadConfig loads the configuration from AppData
func loadConfig() (*Config, error) {
	configPath, err := getConfigPath()
	if err != nil {
		return nil, err
	}

	data, err := os.ReadFile(configPath)
	if err != nil {
		if os.IsNotExist(err) {
			// Config file doesn't exist, return default config
			return &Config{
				ModelOverrides: make(map[string]string),
			}, nil
		}
		return nil, fmt.Errorf("failed to read config file: %v", err)
	}

	var config Config
	if err := json.Unmarshal(data, &config); err != nil {
		return nil, fmt.Errorf("failed to parse config file: %v", err)
	}

	// Ensure ModelOverrides is initialized
	if config.ModelOverrides == nil {
		config.ModelOverrides = make(map[string]string)
	}

	return &config, nil
}

// saveConfig saves the configuration to AppData
func saveConfig(config *Config) error {
	configPath, err := getConfigPath()
	if err != nil {
		return err
	}

	data, err := json.MarshalIndent(config, "", "  ")
	if err != nil {
		return fmt.Errorf("failed to marshal config: %v", err)
	}

	if err := os.WriteFile(configPath, data, 0644); err != nil {
		return fmt.Errorf("failed to write config file: %v", err)
	}

	return nil
}

// displayScriptAndConfirm shows the generated script with explanation and asks for user confirmation
func displayScriptAndConfirm(script, model, taskDescription string) {
	fmt.Printf("╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                           🤖 OohLama Script Generator                        ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	fmt.Printf("📝 Task: %s\n", taskDescription)
	fmt.Printf("🧠 Model: %s\n", model)
	fmt.Printf("📅 Generated: %s\n\n", time.Now().Format("2006-01-02 15:04:05"))

	fmt.Printf("╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                              📋 Generated Script                             ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	// Display the script with syntax highlighting-like formatting
	lines := strings.Split(script, "\n")
	for i, line := range lines {
		lineNum := fmt.Sprintf("%3d", i+1)
		fmt.Printf("\033[90m%s│\033[0m %s\n", lineNum, line)
	}

	fmt.Printf("\n╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                              💡 Script Explanation                           ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	explainScript(script, taskDescription)

	fmt.Printf("\n╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                                  ⚠️  Warning                                 ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	fmt.Printf("⚠️  Always review scripts before execution!\n")
	fmt.Printf("⚠️  This script will perform system operations that may modify files or settings.\n")
	fmt.Printf("⚠️  Make sure you understand what the script does before running it.\n\n")

	// Ask for user confirmation
	fmt.Printf("🤔 What would you like to do?\n")
	fmt.Printf("   [1] 📋 Copy script to clipboard\n")
	fmt.Printf("   [2] ▶️  Execute script now\n")
	fmt.Printf("   [3] 💾 Save script to file\n")
	fmt.Printf("   [4] 📄 Display script only (no action)\n")
	fmt.Printf("   [5] ❌ Cancel\n\n")

	fmt.Printf("Enter your choice (1-5): ")

	scanner := bufio.NewScanner(os.Stdin)
	if scanner.Scan() {
		choice := strings.TrimSpace(scanner.Text())

		switch choice {
		case "1":
			copyToClipboard(script)
		case "2":
			executeScript(script)
		case "3":
			saveScriptToFile(script, taskDescription)
		case "4":
			fmt.Printf("\n📄 Script content:\n\n%s\n", script)
		case "5":
			fmt.Printf("❌ Operation cancelled.\n")
			os.Exit(0)
		default:
			fmt.Printf("❌ Invalid choice. Displaying script only:\n\n%s\n", script)
		}
	}
}

// explainScript provides a simple explanation of what the script does
func explainScript(script, taskDescription string) {
	fmt.Printf("📖 This PowerShell script was generated to: %s\n\n", taskDescription)

	// Analyze script content to provide specific insights
	scriptLower := strings.ToLower(script)
	explanations := []string{}

	if strings.Contains(scriptLower, "get-childitem") || strings.Contains(scriptLower, "gci") {
		explanations = append(explanations, "🔍 Lists files and directories")
	}
	if strings.Contains(scriptLower, "copy-item") || strings.Contains(scriptLower, "copy") {
		explanations = append(explanations, "📁 Copies files or folders")
	}
	if strings.Contains(scriptLower, "remove-item") || strings.Contains(scriptLower, "delete") {
		explanations = append(explanations, "🗑️  Deletes files or folders")
	}
	if strings.Contains(scriptLower, "new-item") || strings.Contains(scriptLower, "mkdir") {
		explanations = append(explanations, "📂 Creates new files or directories")
	}
	if strings.Contains(scriptLower, "get-process") {
		explanations = append(explanations, "⚙️  Monitors running processes")
	}
	if strings.Contains(scriptLower, "get-service") {
		explanations = append(explanations, "🔧 Manages Windows services")
	}
	if strings.Contains(scriptLower, "invoke-webrequest") || strings.Contains(scriptLower, "wget") {
		explanations = append(explanations, "🌐 Makes web requests or downloads content")
	}
	if strings.Contains(scriptLower, "get-wmiobject") || strings.Contains(scriptLower, "get-ciminstance") {
		explanations = append(explanations, "💻 Queries system information")
	}
	if strings.Contains(scriptLower, "try") && strings.Contains(scriptLower, "catch") {
		explanations = append(explanations, "🛡️  Includes error handling")
	}

	if len(explanations) > 0 {
		fmt.Printf("🔧 Key operations:\n")
		for _, explanation := range explanations {
			fmt.Printf("   • %s\n", explanation)
		}
	}

	fmt.Printf("\n✅ The script follows PowerShell best practices and includes appropriate error handling.")
}

// copyToClipboard copies the script to the system clipboard
func copyToClipboard(script string) {
	// Try to use clip.exe (Windows built-in)
	cmd := exec.Command("clip")
	cmd.Stdin = strings.NewReader(script)

	if err := cmd.Run(); err != nil {
		fmt.Printf("❌ Failed to copy to clipboard: %v\n", err)
		fmt.Printf("📋 Here's the script for manual copying:\n\n%s\n", script)
	} else {
		fmt.Printf("✅ Script copied to clipboard! You can paste it into PowerShell.\n")
	}
}

// executeScript runs the PowerShell script directly
func executeScript(script string) {
	fmt.Printf("▶️  Executing script...\n\n")

	cmd := exec.Command("powershell", "-Command", script)
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr

	if err := cmd.Run(); err != nil {
		fmt.Printf("\n❌ Script execution failed: %v\n", err)
	} else {
		fmt.Printf("\n✅ Script executed successfully!\n")
	}
}

// saveScriptToFile saves the script to a .ps1 file
func saveScriptToFile(script, taskDescription string) {
	// Generate a filename based on the task description
	filename := strings.ReplaceAll(strings.ToLower(taskDescription), " ", "_")
	filename = strings.ReplaceAll(filename, "\"", "")
	filename = strings.ReplaceAll(filename, "'", "")

	// Limit filename length and add timestamp
	if len(filename) > 30 {
		filename = filename[:30]
	}

	timestamp := time.Now().Format("20060102_150405")
	filename = fmt.Sprintf("oohlama_%s_%s.ps1", filename, timestamp)

	// Add header comment to the script
	header := fmt.Sprintf(`# Generated by OohLama - %s
# Task: %s
# Generated on: %s
# 
# WARNING: Review this script before execution!
#

`, time.Now().Format("2006-01-02 15:04:05"), taskDescription, time.Now().Format("2006-01-02 15:04:05"))

	fullScript := header + script

	if err := os.WriteFile(filename, []byte(fullScript), 0644); err != nil {
		fmt.Printf("❌ Failed to save script: %v\n", err)
		fmt.Printf("📋 Here's the script for manual saving:\n\n%s\n", script)
	} else {
		fmt.Printf("✅ Script saved to: %s\n", filename)
		fmt.Printf("💡 You can run it with: powershell -ExecutionPolicy Bypass -File %s\n", filename)
	}
}
