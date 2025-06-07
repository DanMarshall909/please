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
	"runtime"
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
	PreferredModel   string                    `json:"preferred_model"`
	ModelOverrides   map[string]string         `json:"model_overrides"`
	Provider         string                    `json:"provider"`         // "ollama", "openai", "anthropic", etc.
	ScriptType       string                    `json:"script_type"`      // "auto", "powershell", "bash"
	OpenAIAPIKey     string                    `json:"openai_api_key"`
	AnthropicAPIKey  string                    `json:"anthropic_api_key"`
	OllamaURL        string                    `json:"ollama_url"`
	CustomProviders  map[string]ProviderConfig `json:"custom_providers"`
}

type ProviderConfig struct {
	URL     string            `json:"url"`
	APIKey  string            `json:"api_key"`
	Headers map[string]string `json:"headers"`
	Model   string            `json:"model"`
}

type OpenAIRequest struct {
	Model       string    `json:"model"`
	Messages    []Message `json:"messages"`
	Temperature float64   `json:"temperature"`
	MaxTokens   int       `json:"max_tokens"`
}

type Message struct {
	Role    string `json:"role"`
	Content string `json:"content"`
}

type OpenAIResponse struct {
	Choices []Choice `json:"choices"`
}

type Choice struct {
	Message Message `json:"message"`
}

type AnthropicRequest struct {
	Model       string    `json:"model"`
	MaxTokens   int       `json:"max_tokens"`
	Messages    []Message `json:"messages"`
	Temperature float64   `json:"temperature"`
}

type AnthropicResponse struct {
	Content []ContentBlock `json:"content"`
}

type ContentBlock struct {
	Type string `json:"type"`
	Text string `json:"text"`
}

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: oohlama <task description>")
		fmt.Println("Example: oohlama \"list all files in the current directory\"")
		os.Exit(1)
	}

	// Join all arguments after the program name as the task description
	taskDescription := strings.Join(os.Args[1:], " ")

	// Load configuration
	config, err := loadConfig()
	if err != nil {
		// Create default config if none exists
		config = createDefaultConfig()
		saveConfig(config) // Ignore errors for config saving
	}

	// Determine script type (auto-detect platform or use config override)
	scriptType := determineScriptType(config)

	// Determine provider and model
	provider := determineProvider(config)
	model, err := selectBestModel(config, taskDescription, provider)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Warning: Could not auto-select model (%v), using fallback\n", err)
		// Use fallback based on provider
		if provider == "openai" {
			model = "gpt-3.5-turbo"
		} else if provider == "anthropic" {
			model = "claude-3-haiku-20240307"
		} else {
			model = "llama3.2"
		}
	}

	// Generate script based on platform and provider
	script, err := generateScript(config, provider, model, taskDescription, scriptType)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error: %v\n", err)
		os.Exit(1)
	}

	// Display the script with explanation and ask for confirmation
	displayScriptAndConfirm(script, model, taskDescription, scriptType, provider)
}

// createDefaultConfig creates a default configuration
func createDefaultConfig() *Config {
	return &Config{
		Provider:        "ollama",
		ScriptType:      "auto",
		OllamaURL:       "http://localhost:11434",
		PreferredModel:  "",
		ModelOverrides:  make(map[string]string),
		CustomProviders: make(map[string]ProviderConfig),
	}
}

// determineScriptType determines what type of script to generate based on platform
func determineScriptType(config *Config) string {
	// Check if user has explicitly set script type
	if config.ScriptType != "" && config.ScriptType != "auto" {
		return config.ScriptType
	}

	// Auto-detect based on platform
	switch runtime.GOOS {
	case "windows":
		return "powershell"
	case "linux", "darwin":
		return "bash"
	default:
		return "bash" // Default to bash for unknown platforms
	}
}

// determineProvider determines which AI provider to use
func determineProvider(config *Config) string {
	// Check environment variable first
	if provider := os.Getenv("OOHLAMA_PROVIDER"); provider != "" {
		return provider
	}

	// Use config setting
	if config.Provider != "" {
		return config.Provider
	}

	// Default to ollama
	return "ollama"
}

// generateScript generates a script using the specified provider and parameters
func generateScript(config *Config, provider, model, taskDescription, scriptType string) (string, error) {
	switch provider {
	case "openai":
		return generateScriptWithOpenAI(config, model, taskDescription, scriptType)
	case "anthropic":
		return generateScriptWithAnthropic(config, model, taskDescription, scriptType)
	case "ollama":
		return generateScriptWithOllama(config, model, taskDescription, scriptType)
	default:
		// Check if it's a custom provider
		if providerConfig, exists := config.CustomProviders[provider]; exists {
			return generateScriptWithCustomProvider(providerConfig, model, taskDescription, scriptType)
		}
		return "", fmt.Errorf("unsupported provider: %s", provider)
	}
}

// generateScriptWithOllama generates a script using Ollama
func generateScriptWithOllama(config *Config, model, taskDescription, scriptType string) (string, error) {
	baseURL := config.OllamaURL
	if baseURL == "" {
		baseURL = "http://localhost:11434"
	}

	prompt := createPrompt(taskDescription, scriptType)

	request := OllamaRequest{
		Model:  model,
		Prompt: prompt,
		Stream: false,
		Options: map[string]interface{}{
			"temperature": 0.3,
			"top_p":       0.9,
		},
	}

	jsonData, err := json.Marshal(request)
	if err != nil {
		return "", fmt.Errorf("failed to marshal request: %v", err)
	}

	client := &http.Client{Timeout: 120 * time.Second}
	resp, err := client.Post(baseURL+"/api/generate", "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		return "", fmt.Errorf("failed to connect to Ollama at %s: %v\nMake sure Ollama is running", baseURL, err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return "", fmt.Errorf("Ollama API returned status %d: %s", resp.StatusCode, string(body))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", fmt.Errorf("failed to read response: %v", err)
	}

	var ollamaResp OllamaResponse
	if err := json.Unmarshal(body, &ollamaResp); err != nil {
		return "", fmt.Errorf("failed to parse response: %v", err)
	}

	script := strings.TrimSpace(ollamaResp.Response)
	return cleanScript(script), nil
}

// generateScriptWithOpenAI generates a script using OpenAI
func generateScriptWithOpenAI(config *Config, model, taskDescription, scriptType string) (string, error) {
	apiKey := config.OpenAIAPIKey
	if apiKey == "" {
		apiKey = os.Getenv("OPENAI_API_KEY")
	}
	if apiKey == "" {
		return "", fmt.Errorf("OpenAI API key not configured. Set OPENAI_API_KEY environment variable or configure in settings")
	}

	prompt := createPrompt(taskDescription, scriptType)

	request := OpenAIRequest{
		Model: model,
		Messages: []Message{
			{Role: "user", Content: prompt},
		},
		Temperature: 0.3,
		MaxTokens:   2000,
	}

	jsonData, err := json.Marshal(request)
	if err != nil {
		return "", fmt.Errorf("failed to marshal request: %v", err)
	}

	client := &http.Client{Timeout: 120 * time.Second}
	req, err := http.NewRequest("POST", "https://api.openai.com/v1/chat/completions", bytes.NewBuffer(jsonData))
	if err != nil {
		return "", fmt.Errorf("failed to create request: %v", err)
	}

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", "Bearer "+apiKey)

	resp, err := client.Do(req)
	if err != nil {
		return "", fmt.Errorf("failed to make OpenAI request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return "", fmt.Errorf("OpenAI API returned status %d: %s", resp.StatusCode, string(body))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", fmt.Errorf("failed to read response: %v", err)
	}

	var openaiResp OpenAIResponse
	if err := json.Unmarshal(body, &openaiResp); err != nil {
		return "", fmt.Errorf("failed to parse response: %v", err)
	}

	if len(openaiResp.Choices) == 0 {
		return "", fmt.Errorf("no response from OpenAI")
	}

	script := strings.TrimSpace(openaiResp.Choices[0].Message.Content)
	return cleanScript(script), nil
}

// generateScriptWithAnthropic generates a script using Anthropic
func generateScriptWithAnthropic(config *Config, model, taskDescription, scriptType string) (string, error) {
	apiKey := config.AnthropicAPIKey
	if apiKey == "" {
		apiKey = os.Getenv("ANTHROPIC_API_KEY")
	}
	if apiKey == "" {
		return "", fmt.Errorf("Anthropic API key not configured. Set ANTHROPIC_API_KEY environment variable or configure in settings")
	}

	prompt := createPrompt(taskDescription, scriptType)

	request := AnthropicRequest{
		Model: model,
		Messages: []Message{
			{Role: "user", Content: prompt},
		},
		Temperature: 0.3,
		MaxTokens:   2000,
	}

	jsonData, err := json.Marshal(request)
	if err != nil {
		return "", fmt.Errorf("failed to marshal request: %v", err)
	}

	client := &http.Client{Timeout: 120 * time.Second}
	req, err := http.NewRequest("POST", "https://api.anthropic.com/v1/messages", bytes.NewBuffer(jsonData))
	if err != nil {
		return "", fmt.Errorf("failed to create request: %v", err)
	}

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("X-API-Key", apiKey)
	req.Header.Set("anthropic-version", "2023-06-01")

	resp, err := client.Do(req)
	if err != nil {
		return "", fmt.Errorf("failed to make Anthropic request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return "", fmt.Errorf("Anthropic API returned status %d: %s", resp.StatusCode, string(body))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", fmt.Errorf("failed to read response: %v", err)
	}

	var anthropicResp AnthropicResponse
	if err := json.Unmarshal(body, &anthropicResp); err != nil {
		return "", fmt.Errorf("failed to parse response: %v", err)
	}

	if len(anthropicResp.Content) == 0 {
		return "", fmt.Errorf("no response from Anthropic")
	}

	script := strings.TrimSpace(anthropicResp.Content[0].Text)
	return cleanScript(script), nil
}

// generateScriptWithCustomProvider generates a script using a custom provider
func generateScriptWithCustomProvider(providerConfig ProviderConfig, model, taskDescription, scriptType string) (string, error) {
	prompt := createPrompt(taskDescription, scriptType)

	// Create a generic request structure
	requestBody := map[string]interface{}{
		"model":       model,
		"prompt":      prompt,
		"temperature": 0.3,
	}

	jsonData, err := json.Marshal(requestBody)
	if err != nil {
		return "", fmt.Errorf("failed to marshal request: %v", err)
	}

	client := &http.Client{Timeout: 120 * time.Second}
	req, err := http.NewRequest("POST", providerConfig.URL, bytes.NewBuffer(jsonData))
	if err != nil {
		return "", fmt.Errorf("failed to create request: %v", err)
	}

	req.Header.Set("Content-Type", "application/json")
	if providerConfig.APIKey != "" {
		req.Header.Set("Authorization", "Bearer "+providerConfig.APIKey)
	}

	// Add custom headers
	for key, value := range providerConfig.Headers {
		req.Header.Set(key, value)
	}

	resp, err := client.Do(req)
	if err != nil {
		return "", fmt.Errorf("failed to make request to custom provider: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return "", fmt.Errorf("custom provider returned status %d: %s", resp.StatusCode, string(body))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", fmt.Errorf("failed to read response: %v", err)
	}

	// Try to parse as a generic response
	var response map[string]interface{}
	if err := json.Unmarshal(body, &response); err != nil {
		return "", fmt.Errorf("failed to parse response: %v", err)
	}

	// Try common response fields
	if content, ok := response["response"].(string); ok {
		return cleanScript(content), nil
	}
	if content, ok := response["text"].(string); ok {
		return cleanScript(content), nil
	}
	if content, ok := response["content"].(string); ok {
		return cleanScript(content), nil
	}

	return "", fmt.Errorf("unable to extract response from custom provider")
}

// createPrompt creates the appropriate prompt based on script type
func createPrompt(taskDescription, scriptType string) string {
	if scriptType == "bash" {
		return fmt.Sprintf(`You are a Bash scripting expert. Generate a complete, working Bash script to accomplish the following task:

%s

Requirements:
- Write clean, well-commented Bash code
- Include error handling where appropriate
- Use Bash best practices
- Include proper shebang (#!/bin/bash)
- Do NOT include markdown code blocks, backticks, or formatting
- Do NOT include explanations or descriptions
- Return ONLY the raw Bash script code
- The script should be ready to run as-is
- Start directly with the shebang and Bash commands

Bash Script:`, taskDescription)
	} else {
		return fmt.Sprintf(`You are a PowerShell expert. Generate a complete, working PowerShell script to accomplish the following task:

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
	}
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
			strings.HasPrefix(trimmed, "Here's a Bash script") ||
			strings.HasPrefix(trimmed, "This script will") ||
			strings.HasPrefix(trimmed, "The following script") {
			continue
		}

		cleanedLines = append(cleanedLines, line)
	}

	return strings.Join(cleanedLines, "\n")
}

// selectBestModel automatically chooses the most appropriate model for the given task
func selectBestModel(config *Config, taskDescription, provider string) (string, error) {
	// Check if user has manually overridden via environment
	if m := os.Getenv("OLLAMA_MODEL"); m != "" && provider == "ollama" {
		return m, nil
	}

	// Check for task-specific overrides in config
	taskType := categorizeTask(taskDescription)
	if override, exists := config.ModelOverrides[taskType]; exists {
		return override, nil
	}

	// Provider-specific model selection
	switch provider {
	case "openai":
		return selectOpenAIModel(taskType), nil
	case "anthropic":
		return selectAnthropicModel(taskType), nil
	case "ollama":
		return selectOllamaModel(config, taskDescription, taskType)
	default:
		// Check custom providers
		if providerConfig, exists := config.CustomProviders[provider]; exists {
			if providerConfig.Model != "" {
				return providerConfig.Model, nil
			}
		}
		return "", fmt.Errorf("no model configured for provider: %s", provider)
	}
}

// selectOpenAIModel chooses the best OpenAI model for the task
func selectOpenAIModel(taskType string) string {
	if taskType == "coding" {
		return "gpt-4" // Best for coding tasks
	}
	return "gpt-3.5-turbo" // Good general purpose model
}

// selectAnthropicModel chooses the best Anthropic model for the task
func selectAnthropicModel(taskType string) string {
	if taskType == "coding" {
		return "claude-3-sonnet-20240229" // Good for coding
	}
	return "claude-3-haiku-20240307" // Fast and efficient
}

// selectOllamaModel chooses the best Ollama model for the task
func selectOllamaModel(config *Config, taskDescription, taskType string) (string, error) {
	baseURL := config.OllamaURL
	if baseURL == "" {
		baseURL = "http://localhost:11434"
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
	// Model preference ranking based on script generation capability
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

// getConfigPath returns the path to the configuration file
func getConfigPath() (string, error) {
	var configDir string

	// Cross-platform config directory
	switch runtime.GOOS {
	case "windows":
		appData := os.Getenv("APPDATA")
		if appData == "" {
			return "", fmt.Errorf("APPDATA environment variable not set")
		}
		configDir = filepath.Join(appData, "oohlama")
	case "darwin":
		homeDir, err := os.UserHomeDir()
		if err != nil {
			return "", fmt.Errorf("could not get user home directory: %v", err)
		}
		configDir = filepath.Join(homeDir, "Library", "Application Support", "oohlama")
	default: // Linux and others
		homeDir, err := os.UserHomeDir()
		if err != nil {
			return "", fmt.Errorf("could not get user home directory: %v", err)
		}
		configDir = filepath.Join(homeDir, ".config", "oohlama")
	}

	// Create directory if it doesn't exist
	if err := os.MkdirAll(configDir, 0755); err != nil {
		return "", fmt.Errorf("failed to create config directory: %v", err)
	}

	return filepath.Join(configDir, "config.json"), nil
}

// loadConfig loads the configuration from the appropriate location
func loadConfig() (*Config, error) {
	configPath, err := getConfigPath()
	if err != nil {
		return nil, err
	}

	data, err := os.ReadFile(configPath)
	if err != nil {
		if os.IsNotExist(err) {
			// Config file doesn't exist, return default config
			return createDefaultConfig(), nil
		}
		return nil, fmt.Errorf("failed to read config file: %v", err)
	}

	var config Config
	if err := json.Unmarshal(data, &config); err != nil {
		return nil, fmt.Errorf("failed to parse config file: %v", err)
	}

	// Ensure maps are initialized
	if config.ModelOverrides == nil {
		config.ModelOverrides = make(map[string]string)
	}
	if config.CustomProviders == nil {
		config.CustomProviders = make(map[string]ProviderConfig)
	}

	return &config, nil
}

// saveConfig saves the configuration to the appropriate location
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
func displayScriptAndConfirm(script, model, taskDescription, scriptType, provider string) {
	fmt.Printf("╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                           🤖 OohLama Script Generator                        ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n\n")

	fmt.Printf("📝 Task: %s\n", taskDescription)
	fmt.Printf("🧠 Model: %s (%s)\n", model, provider)
	fmt.Printf("🖥️  Platform: %s (%s script)\n", runtime.GOOS, scriptType)
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

	explainScript(script, taskDescription, scriptType)

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
			copyToClipboard(script, scriptType)
		case "2":
			executeScript(script, scriptType)
		case "3":
			saveScriptToFile(script, taskDescription, scriptType)
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
func explainScript(script, taskDescription, scriptType string) {
	fmt.Printf("📖 This %s script was generated to: %s\n\n", scriptType, taskDescription)

	// Analyze script content to provide specific insights
	scriptLower := strings.ToLower(script)
	explanations := []string{}

	if scriptType == "bash" {
		// Bash-specific analysis
		if strings.Contains(scriptLower, "ls") || strings.Contains(scriptLower, "find") {
			explanations = append(explanations, "🔍 Lists files and directories")
		}
		if strings.Contains(scriptLower, "cp") || strings.Contains(scriptLower, "rsync") {
			explanations = append(explanations, "📁 Copies files or folders")
		}
		if strings.Contains(scriptLower, "rm") || strings.Contains(scriptLower, "unlink") {
			explanations = append(explanations, "🗑️  Deletes files or folders")
		}
		if strings.Contains(scriptLower, "mkdir") || strings.Contains(scriptLower, "touch") {
			explanations = append(explanations, "📂 Creates new files or directories")
		}
		if strings.Contains(scriptLower, "ps") || strings.Contains(scriptLower, "pgrep") {
			explanations = append(explanations, "⚙️  Monitors running processes")
		}
		if strings.Contains(scriptLower, "systemctl") || strings.Contains(scriptLower, "service") {
			explanations = append(explanations, "🔧 Manages system services")
		}
		if strings.Contains(scriptLower, "curl") || strings.Contains(scriptLower, "wget") {
			explanations = append(explanations, "🌐 Makes web requests or downloads content")
		}
		if strings.Contains(scriptLower, "if") && strings.Contains(scriptLower, "then") {
			explanations = append(explanations, "🛡️  Includes error handling and conditionals")
		}
	} else {
		// PowerShell-specific analysis
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
	}

	if len(explanations) > 0 {
		fmt.Printf("🔧 Key operations:\n")
		for _, explanation := range explanations {
			fmt.Printf("   • %s\n", explanation)
		}
	}

	fmt.Printf("\n✅ The script follows %s best practices and includes appropriate error handling.", scriptType)
}

// copyToClipboard copies the script to the system clipboard
func copyToClipboard(script, scriptType string) {
	var cmd *exec.Cmd
	
	switch runtime.GOOS {
	case "windows":
		cmd = exec.Command("clip")
	case "darwin":
		cmd = exec.Command("pbcopy")
	case "linux":
		// Try xclip first, then xsel
		if _, err := exec.LookPath("xclip"); err == nil {
			cmd = exec.Command("xclip", "-selection", "clipboard")
		} else if _, err := exec.LookPath("xsel"); err == nil {
			cmd = exec.Command("xsel", "--clipboard", "--input")
		} else {
			fmt.Printf("❌ No clipboard utility found. Install xclip or xsel.\n")
			fmt.Printf("📋 Here's the script for manual copying:\n\n%s\n", script)
			return
		}
	default:
		fmt.Printf("❌ Clipboard not supported on this platform.\n")
		fmt.Printf("📋 Here's the script for manual copying:\n\n%s\n", script)
		return
	}

	cmd.Stdin = strings.NewReader(script)

	if err := cmd.Run(); err != nil {
		fmt.Printf("❌ Failed to copy to clipboard: %v\n", err)
		fmt.Printf("📋 Here's the script for manual copying:\n\n%s\n", script)
	} else {
		if scriptType == "bash" {
			fmt.Printf("✅ Script copied to clipboard! You can paste it into a terminal.\n")
		} else {
			fmt.Printf("✅ Script copied to clipboard! You can paste it into PowerShell.\n")
		}
	}
}

// executeScript runs the script directly
func executeScript(script, scriptType string) {
	fmt.Printf("▶️  Executing script...\n\n")

	var cmd *exec.Cmd

	if scriptType == "bash" {
		switch runtime.GOOS {
		case "windows":
			// On Windows, try to use WSL bash
			cmd = exec.Command("wsl", "bash", "-c", script)
		default:
			cmd = exec.Command("bash", "-c", script)
		}
	} else {
		cmd = exec.Command("powershell", "-Command", script)
	}

	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr

	if err := cmd.Run(); err != nil {
		fmt.Printf("\n❌ Script execution failed: %v\n", err)
	} else {
		fmt.Printf("\n✅ Script executed successfully!\n")
	}
}

// saveScriptToFile saves the script to a file
func saveScriptToFile(script, taskDescription, scriptType string) {
	// Generate a filename based on the task description
	filename := strings.ReplaceAll(strings.ToLower(taskDescription), " ", "_")
	filename = strings.ReplaceAll(filename, "\"", "")
	filename = strings.ReplaceAll(filename, "'", "")

	// Limit filename length and add timestamp
	if len(filename) > 30 {
		filename = filename[:30]
	}

	timestamp := time.Now().Format("20060102_150405")
	
	var extension string
	if scriptType == "bash" {
		extension = "sh"
	} else {
		extension = "ps1"
	}
	
	filename = fmt.Sprintf("oohlama_%s_%s.%s", filename, timestamp, extension)

	// Add header comment to the script
	var commentChar string
	if scriptType == "bash" {
		commentChar = "#"
	} else {
		commentChar = "#"
	}

	header := fmt.Sprintf(`%s Generated by OohLama - %s
%s Task: %s
%s Generated on: %s
%s 
%s WARNING: Review this script before execution!
%s

`, commentChar, time.Now().Format("2006-01-02 15:04:05"), 
   commentChar, taskDescription, 
   commentChar, time.Now().Format("2006-01-02 15:04:05"),
   commentChar, commentChar, commentChar)

	fullScript := header + script

	if err := os.WriteFile(filename, []byte(fullScript), 0755); err != nil {
		fmt.Printf("❌ Failed to save script: %v\n", err)
		fmt.Printf("📋 Here's the script for manual saving:\n\n%s\n", script)
	} else {
		fmt.Printf("✅ Script saved to: %s\n", filename)
		if scriptType == "bash" {
			fmt.Printf("💡 You can run it with: chmod +x %s && ./%s\n", filename, filename)
		} else {
			fmt.Printf("💡 You can run it with: powershell -ExecutionPolicy Bypass -File %s\n", filename)
		}
	}
}
