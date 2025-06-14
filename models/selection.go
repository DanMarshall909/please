package models

import (
	"fmt"
	"os"
	"strings"

	"please/providers"
	"please/types"
)

// SelectBestModel automatically chooses the most appropriate model for the given task
func SelectBestModel(config *types.Config, taskDescription, provider string) (string, error) {
	// Check if user has manually overridden via environment
	if m := os.Getenv("OLLAMA_MODEL"); m != "" && provider == "ollama" {
		return m, nil
	}

	// Check for task-specific overrides in config
	taskType := CategorizeTask(taskDescription)
	if override, exists := config.ModelOverrides[taskType]; exists {
		return override, nil
	}

	// Provider-specific model selection
	switch provider {
	case "openai":
		return SelectOpenAIModel(taskType), nil
	case "anthropic":
		return SelectAnthropicModel(taskType), nil
	case "ollama":
		return SelectOllamaModel(config, taskDescription, taskType)
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

// SelectOpenAIModel chooses the best OpenAI model for the task
func SelectOpenAIModel(taskType string) string {
	if taskType == "coding" {
		return "gpt-4" // Best for coding tasks
	}
	return "gpt-3.5-turbo" // Good general purpose model
}

// SelectAnthropicModel chooses the best Anthropic model for the task
func SelectAnthropicModel(taskType string) string {
	if taskType == "coding" {
		return "claude-3-sonnet-20240229" // Good for coding
	}
	return "claude-3-haiku-20240307" // Fast and efficient
}

// SelectOllamaModel chooses the best Ollama model for the task
func SelectOllamaModel(config *types.Config, taskDescription, taskType string) (string, error) {
	// Create Ollama provider to get models
	ollamaProvider := providers.NewOllamaProvider(config)
	
	// Get available models from Ollama
	models, err := ollamaProvider.GetAvailableModels()
	if err != nil {
		return "", fmt.Errorf("failed to get available models: %v", err)
	}

	if len(models) == 0 {
		return "", fmt.Errorf("no models available in Ollama")
	}

	// Rank models by suitability for the task
	bestModel := RankModels(models, taskDescription, taskType)

	// Update config with the selected model if we don't have a preferred one
	if config.PreferredModel == "" {
		config.PreferredModel = bestModel
		// Note: We don't save config here to avoid import cycles
		// This should be handled by the caller
	}

	return bestModel, nil
}

// CategorizeTask determines the type of task to help with model selection
func CategorizeTask(description string) string {
	desc := strings.ToLower(description)

	// Code-related tasks
	if strings.Contains(desc, "script") || strings.Contains(desc, "function") ||
		strings.Contains(desc, "code") || strings.Contains(desc, "program") {
		return "coding"
	}

	// Network/web related tasks (check before file management since download can involve files)
	if strings.Contains(desc, "web") || strings.Contains(desc, "http") ||
		strings.Contains(desc, "url") || strings.Contains(desc, "download") ||
		strings.Contains(desc, "network") || strings.Contains(desc, "api") {
		return "network"
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

	return "general"
}
