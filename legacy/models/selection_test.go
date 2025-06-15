package models

import (
	"strings"
	"testing"

	"please/types"
)

func TestSelectBestModel(t *testing.T) {
	tests := []struct {
		name           string
		provider       string
		taskDesc       string
		expectedResult bool // true if should succeed, false if should error
	}{
		{
			name:           "OpenAI provider general task",
			provider:       "openai",
			taskDesc:       "create a script",
			expectedResult: true,
		},
		{
			name:           "Anthropic provider coding task",
			provider:       "anthropic",
			taskDesc:       "write a function",
			expectedResult: true,
		},
		{
			name:           "Unknown provider should error",
			provider:       "unknown",
			taskDesc:       "general task",
			expectedResult: false,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			cfg := &types.Config{
				Provider:        tt.provider,
				ModelOverrides:  make(map[string]string),
				CustomProviders: make(map[string]types.ProviderConfig),
			}

			result, err := SelectBestModel(cfg, tt.taskDesc, tt.provider)

			if tt.expectedResult {
				if err != nil {
					t.Errorf("SelectBestModel should not error for %s provider: %v", tt.provider, err)
				}
				if result == "" {
					t.Errorf("SelectBestModel should return a model for %s provider", tt.provider)
				}
			} else {
				if err == nil {
					t.Errorf("SelectBestModel should error for %s provider", tt.provider)
				}
			}
		})
	}
}

func TestSelectBestModelWithOverrides(t *testing.T) {
	// Test when config has model overrides
	cfg := &types.Config{
		Provider: "openai",
		ModelOverrides: map[string]string{
			"coding": "custom-coding-model",
		},
		CustomProviders: make(map[string]types.ProviderConfig),
	}

	result, err := SelectBestModel(cfg, "write a script to test", "openai")

	if err != nil {
		t.Fatalf("SelectBestModel should not error: %v", err)
	}

	// Should use the override for coding tasks
	if result != "custom-coding-model" {
		t.Errorf("SelectBestModel should use override model, got %s", result)
	}
}

func TestCategorizeTask(t *testing.T) {
	tests := []struct {
		name        string
		description string
		expected    string
	}{
		{
			name:        "Coding task",
			description: "write a script to parse files",
			expected:    "coding",
		},
		{
			name:        "System admin task",
			description: "install a service on the server",
			expected:    "sysadmin",
		},
		{
			name:        "File management task",
			description: "copy files from folder A to folder B",
			expected:    "filemanagement",
		},
		{
			name:        "Network task",
			description: "download files from a web URL",
			expected:    "network",
		},
		{
			name:        "General task",
			description: "help me with something",
			expected:    "general",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := CategorizeTask(tt.description)

			if result != tt.expected {
				t.Errorf("CategorizeTask(%s) = %s, want %s", tt.description, result, tt.expected)
			}
		})
	}
}

func TestSelectOpenAIModel(t *testing.T) {
	tests := []struct {
		name     string
		taskType string
		expected string
	}{
		{
			name:     "Coding task uses GPT-4",
			taskType: "coding",
			expected: "gpt-4",
		},
		{
			name:     "General task uses GPT-3.5-turbo",
			taskType: "general",
			expected: "gpt-3.5-turbo",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := SelectOpenAIModel(tt.taskType)

			if result != tt.expected {
				t.Errorf("SelectOpenAIModel(%s) = %s, want %s", tt.taskType, result, tt.expected)
			}
		})
	}
}

func TestSelectAnthropicModel(t *testing.T) {
	tests := []struct {
		name     string
		taskType string
		expected string
	}{
		{
			name:     "Coding task uses Claude Sonnet",
			taskType: "coding",
			expected: "claude-3-sonnet-20240229",
		},
		{
			name:     "General task uses Claude Haiku",
			taskType: "general",
			expected: "claude-3-haiku-20240307",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := SelectAnthropicModel(tt.taskType)

			if result != tt.expected {
				t.Errorf("SelectAnthropicModel(%s) = %s, want %s", tt.taskType, result, tt.expected)
			}
		})
	}
}

func Test_when_ranking_models_then_return_best_match_for_task_type(t *testing.T) {
	models := []types.ModelInfo{
		{Name: "llama3.2:latest"},
		{Name: "deepseek-coder:6.7b"},
		{Name: "codellama:7b"},
		{Name: "mistral:7b"},
	}

	// Test coding task should prefer coding-optimized models
	result := RankModels(models, "write a Python script", "coding")

	// Should prefer coding-specific models
	if !strings.Contains(result, "coder") && !strings.Contains(result, "codellama") {
		t.Errorf("Expected coding-optimized model for coding task, got: %s", result)
	}
}

func Test_when_ranking_models_with_empty_list_then_return_empty_string(t *testing.T) {
	models := []types.ModelInfo{}

	result := RankModels(models, "any task", "general")

	if result != "" {
		t.Errorf("Expected empty string for empty model list, got: %s", result)
	}
}

func Test_when_ranking_models_then_handle_general_tasks(t *testing.T) {
	models := []types.ModelInfo{
		{Name: "llama3.2:latest"},
		{Name: "mistral:7b"},
	}

	result := RankModels(models, "help me organize files", "general")

	// Should return a non-empty model name
	if result == "" {
		t.Error("Expected non-empty model name for general task")
	}

	// Should be one of the available models
	found := false
	for _, model := range models {
		if model.Name == result {
			found = true
			break
		}
	}
	if !found {
		t.Errorf("Result %s should be one of the available models", result)
	}
}

func Test_when_environment_variable_set_then_override_model_selection(t *testing.T) {
	// Set environment variable
	t.Setenv("OLLAMA_MODEL", "custom-env-model")

	cfg := &types.Config{
		Provider:        "ollama",
		ModelOverrides:  make(map[string]string),
		CustomProviders: make(map[string]types.ProviderConfig),
	}

	result, err := SelectBestModel(cfg, "any task", "ollama")

	if err != nil {
		t.Fatalf("SelectBestModel should not error: %v", err)
	}

	if result != "custom-env-model" {
		t.Errorf("Expected environment variable override 'custom-env-model', got '%s'", result)
	}
}

func Test_when_custom_provider_with_model_then_use_configured_model(t *testing.T) {
	cfg := &types.Config{
		Provider:       "custom-provider",
		ModelOverrides: make(map[string]string),
		CustomProviders: map[string]types.ProviderConfig{
			"custom-provider": {
				URL:    "http://localhost:8080",
				APIKey: "test-key",
				Model:  "custom-model",
			},
		},
	}

	result, err := SelectBestModel(cfg, "test task", "custom-provider")

	if err != nil {
		t.Fatalf("SelectBestModel should not error for custom provider: %v", err)
	}

	if result != "custom-model" {
		t.Errorf("Expected custom provider model 'custom-model', got '%s'", result)
	}
}

func Test_when_custom_provider_without_model_then_error(t *testing.T) {
	cfg := &types.Config{
		Provider:       "custom-provider",
		ModelOverrides: make(map[string]string),
		CustomProviders: map[string]types.ProviderConfig{
			"custom-provider": {
				URL:    "http://localhost:8080",
				APIKey: "test-key",
				// No Model specified
			},
		},
	}

	_, err := SelectBestModel(cfg, "test task", "custom-provider")

	if err == nil {
		t.Error("SelectBestModel should error when custom provider has no model configured")
	}

	if !strings.Contains(err.Error(), "no model configured") {
		t.Errorf("Error should mention 'no model configured', got: %v", err)
	}
}

func Test_when_ranking_models_then_prefer_larger_models(t *testing.T) {
	models := []types.ModelInfo{
		{Name: "small-model", Size: 2000000000},  // 2GB
		{Name: "large-model", Size: 8000000000},  // 8GB
		{Name: "medium-model", Size: 5000000000}, // 5GB
	}

	result := RankModels(models, "general task", "general")

	// Should prefer the larger model
	if result != "large-model" {
		t.Errorf("Expected larger model to be preferred, got: %s", result)
	}
}

func Test_when_ranking_models_then_boost_coding_models_for_coding_tasks(t *testing.T) {
	models := []types.ModelInfo{
		{Name: "llama3.2", Size: 7000000000},
		{Name: "codellama", Size: 6000000000}, // Smaller but coding-focused
	}

	result := RankModels(models, "write a function", "coding")

	// Should prefer the coding model despite smaller size
	if result != "codellama" {
		t.Errorf("Expected coding model to be preferred for coding task, got: %s", result)
	}
}

func Test_when_categorizing_task_then_handle_case_insensitive_matching(t *testing.T) {
	tests := []struct {
		description string
		expected    string
	}{
		{"Write a SCRIPT in Python", "coding"},
		{"COPY Files from one location", "filemanagement"},
		{"Install SERVICE on SERVER", "sysadmin"},
		{"Download from HTTP URL", "network"},
	}

	for _, tt := range tests {
		t.Run(tt.description, func(t *testing.T) {
			result := CategorizeTask(tt.description)
			if result != tt.expected {
				t.Errorf("CategorizeTask(%s) = %s, want %s", tt.description, result, tt.expected)
			}
		})
	}
}

func Test_when_categorizing_task_then_prioritize_network_over_file_for_download(t *testing.T) {
	// Download tasks should be categorized as network, not file management
	result := CategorizeTask("download files from the web server")

	if result != "network" {
		t.Errorf("Download tasks should be categorized as network, got: %s", result)
	}
}
