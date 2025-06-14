package models

import (
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
	}{
		{
			name:     "Coding task",
			taskType: "coding",
		},
		{
			name:     "General task", 
			taskType: "general",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := SelectAnthropicModel(tt.taskType)
			
			// Should return a non-empty model
			if result == "" {
				t.Errorf("SelectAnthropicModel should return a model for task type %s", tt.taskType)
			}
		})
	}
}
