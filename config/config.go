package config

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"runtime"

	"please/types"
)

// Load loads the configuration from the appropriate platform-specific location
func Load() (*types.Config, error) {
	configPath, err := getConfigPath()
	if err != nil {
		return nil, err
	}

	data, err := os.ReadFile(configPath)
	if err != nil {
		if os.IsNotExist(err) {
			// Config file doesn't exist, return default config
			return CreateDefault(), nil
		}
		return nil, fmt.Errorf("failed to read config file: %v", err)
	}

	var config types.Config
	if err := json.Unmarshal(data, &config); err != nil {
		return nil, fmt.Errorf("failed to parse config file: %v", err)
	}

	// Ensure maps are initialized
	if config.ModelOverrides == nil {
		config.ModelOverrides = make(map[string]string)
	}
	if config.CustomProviders == nil {
		config.CustomProviders = make(map[string]types.ProviderConfig)
	}

	// Override with environment variables if present
	overrideWithEnvironment(&config)

	return &config, nil
}

// Save saves the configuration to the appropriate platform-specific location
func Save(config *types.Config) error {
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

// CreateDefault creates a default configuration
func CreateDefault() *types.Config {
	return &types.Config{
		Provider:        "ollama",
		ScriptType:      "auto",
		OllamaURL:       "http://localhost:11434",
		PreferredModel:  "",
		ModelOverrides:  make(map[string]string),
		CustomProviders: make(map[string]types.ProviderConfig),
	}
}

// getConfigPath returns the path to the configuration file based on the platform
func getConfigPath() (string, error) {
	var configDir string

	// Cross-platform config directory
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

	return filepath.Join(configDir, "config.json"), nil
}

// DetermineScriptType determines what type of script to generate based on platform and config
func DetermineScriptType(config *types.Config) string {
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

// DetermineProvider determines which AI provider to use based on config and environment
func DetermineProvider(config *types.Config) string {
	// Check environment variable first (new and legacy names)
	if provider := os.Getenv("PLEASE_PROVIDER"); provider != "" {
		return provider
	}
	if provider := os.Getenv("OOHLAMA_PROVIDER"); provider != "" {
		return provider // Legacy compatibility
	}

	// Use config setting
	if config.Provider != "" {
		return config.Provider
	}

	// Default to ollama
	return "ollama"
}

// overrideWithEnvironment overrides configuration values with environment variables if present
func overrideWithEnvironment(config *types.Config) {
	// Override API keys from environment variables
	if apiKey := os.Getenv("OPENAI_API_KEY"); apiKey != "" {
		config.OpenAIAPIKey = apiKey
	}
	
	if apiKey := os.Getenv("ANTHROPIC_API_KEY"); apiKey != "" {
		config.AnthropicAPIKey = apiKey
	}
	
	// Override Ollama URL if set
	if ollamaURL := os.Getenv("OLLAMA_URL"); ollamaURL != "" {
		config.OllamaURL = ollamaURL
	}
	
	// Override script type if set
	if scriptType := os.Getenv("PLEASE_SCRIPT_TYPE"); scriptType != "" {
		config.ScriptType = scriptType
	} else if scriptType := os.Getenv("OOHLAMA_SCRIPT_TYPE"); scriptType != "" {
		config.ScriptType = scriptType // Legacy compatibility
	}
}
