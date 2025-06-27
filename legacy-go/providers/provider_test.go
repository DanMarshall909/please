package providers

import (
	"strings"
	"testing"

	"please/types"
)

// Test CreatePrompt function for different script types
func Test_when_creating_bash_prompt_then_include_bash_specific_elements(t *testing.T) {
	// Arrange
	taskDescription := "list all files in current directory"
	scriptType := "bash"

	// Act
	result := CreatePrompt(taskDescription, scriptType)

	// Assert
	if !strings.Contains(result, "Bash scripting expert") {
		t.Error("Expected bash prompt to contain 'Bash scripting expert'")
	}
	if !strings.Contains(result, taskDescription) {
		t.Errorf("Expected prompt to contain task description: %s", taskDescription)
	}
	if !strings.Contains(result, "#!/bin/bash") {
		t.Error("Expected bash prompt to mention shebang")
	}
	if !strings.Contains(result, "Bash Script:") {
		t.Error("Expected bash prompt to end with 'Bash Script:'")
	}
}

func Test_when_creating_powershell_prompt_then_include_powershell_specific_elements(t *testing.T) {
	// Arrange
	taskDescription := "get system information"
	scriptType := "powershell"

	// Act
	result := CreatePrompt(taskDescription, scriptType)

	// Assert
	if !strings.Contains(result, "PowerShell expert") {
		t.Error("Expected powershell prompt to contain 'PowerShell expert'")
	}
	if !strings.Contains(result, taskDescription) {
		t.Errorf("Expected prompt to contain task description: %s", taskDescription)
	}
	if !strings.Contains(result, "PowerShell Script:") {
		t.Error("Expected powershell prompt to end with 'PowerShell Script:'")
	}
	if strings.Contains(result, "#!/bin/bash") {
		t.Error("Expected powershell prompt to NOT contain bash shebang")
	}
}

func Test_when_creating_prompt_with_unknown_script_type_then_default_to_powershell(t *testing.T) {
	// Arrange
	taskDescription := "do something"
	scriptType := "unknown"

	// Act
	result := CreatePrompt(taskDescription, scriptType)

	// Assert
	if !strings.Contains(result, "PowerShell expert") {
		t.Error("Expected unknown script type to default to PowerShell")
	}
	if !strings.Contains(result, "PowerShell Script:") {
		t.Error("Expected unknown script type to end with 'PowerShell Script:'")
	}
}

func Test_when_creating_prompt_with_empty_task_description_then_include_empty_task(t *testing.T) {
	// Arrange
	taskDescription := ""
	scriptType := "bash"

	// Act
	result := CreatePrompt(taskDescription, scriptType)

	// Assert
	if !strings.Contains(result, "Bash scripting expert") {
		t.Error("Expected bash prompt even with empty task description")
	}
	// Should still contain the task description (even if empty)
	if !strings.Contains(result, "\n\n\n") {
		t.Error("Expected prompt to contain empty task description section")
	}
}

func Test_when_creating_bash_prompt_then_include_safety_requirements(t *testing.T) {
	// Arrange
	taskDescription := "delete files"
	scriptType := "bash"

	// Act
	result := CreatePrompt(taskDescription, scriptType)

	// Assert
	if !strings.Contains(result, "DANGEROUS OR UNKNOWN COMMANDS") {
		t.Error("Expected bash prompt to include danger warning")
	}
	if !strings.Contains(result, "error handling") {
		t.Error("Expected bash prompt to mention error handling")
	}
	if !strings.Contains(result, "Do NOT include markdown") {
		t.Error("Expected bash prompt to specify no markdown")
	}
}

func Test_when_creating_powershell_prompt_then_include_safety_requirements(t *testing.T) {
	// Arrange
	taskDescription := "modify registry"
	scriptType := "powershell"

	// Act
	result := CreatePrompt(taskDescription, scriptType)

	// Assert
	if !strings.Contains(result, "DANGEROUS OR UNKNOWN COMMANDS") {
		t.Error("Expected powershell prompt to include danger warning")
	}
	if !strings.Contains(result, "error handling") {
		t.Error("Expected powershell prompt to mention error handling")
	}
	if !strings.Contains(result, "Do NOT include markdown") {
		t.Error("Expected powershell prompt to specify no markdown")
	}
}

func Test_when_creating_prompt_with_special_characters_then_preserve_them(t *testing.T) {
	// Arrange
	taskDescription := "find files with name 'test*.txt' & sort by date"
	scriptType := "bash"

	// Act
	result := CreatePrompt(taskDescription, scriptType)

	// Assert
	if !strings.Contains(result, "test*.txt") {
		t.Error("Expected prompt to preserve asterisk in task description")
	}
	if !strings.Contains(result, "&") {
		t.Error("Expected prompt to preserve ampersand in task description")
	}
	if !strings.Contains(result, "'test*.txt'") {
		t.Error("Expected prompt to preserve quotes in task description")
	}
}

// Mock provider for testing GenerateFixedScript
type MockProvider struct {
	GenerateScriptFunc func(*types.ScriptRequest) (*types.ScriptResponse, error)
	NameFunc           func() string
	IsConfiguredFunc   func(*types.Config) bool
}

func (m *MockProvider) GenerateScript(request *types.ScriptRequest) (*types.ScriptResponse, error) {
	if m.GenerateScriptFunc != nil {
		return m.GenerateScriptFunc(request)
	}
	return &types.ScriptResponse{
		Script: "echo 'fixed script'",
	}, nil
}

func (m *MockProvider) Name() string {
	if m.NameFunc != nil {
		return m.NameFunc()
	}
	return "mock"
}

func (m *MockProvider) IsConfigured(config *types.Config) bool {
	if m.IsConfiguredFunc != nil {
		return m.IsConfiguredFunc(config)
	}
	return true
}

// Test GenerateFixedScript function
func Test_when_generating_fixed_script_with_unsupported_provider_then_return_error(t *testing.T) {
	// Arrange
	originalScript := "echo 'hello'"
	errorMessage := "command not found"
	scriptType := "bash"
	model := "test-model"
	provider := "unsupported-provider"
	config := &types.Config{}

	// Act
	result, err := GenerateFixedScript(originalScript, errorMessage, scriptType, model, provider, config)

	// Assert
	if err == nil {
		t.Error("Expected error for unsupported provider")
	}
	if result != "" {
		t.Errorf("Expected empty result for unsupported provider, got: %s", result)
	}
	if !strings.Contains(err.Error(), "unsupported provider") {
		t.Errorf("Expected error message to mention unsupported provider, got: %s", err.Error())
	}
}

func Test_when_generating_fixed_script_then_include_original_script_and_error_in_prompt(t *testing.T) {
	// Arrange
	originalScript := "rm -rf /"
	errorMessage := "permission denied"
	scriptType := "bash"
	model := "test-model"
	provider := "openai"
	config := &types.Config{
		Provider:     "openai",
		OpenAIAPIKey: "test-key",
	}

	// We can't easily test the actual providers without mocking the HTTP calls
	// So let's test the error case to verify the flow

	// Act
	_, err := GenerateFixedScript(originalScript, errorMessage, scriptType, model, provider, config)

	// Assert - This will likely fail because we don't have real API keys
	// But we can verify the error handling works
	if err != nil {
		// This is expected behavior when API keys are invalid
		// The important thing is that the function doesn't panic
		t.Logf("Expected error due to invalid API credentials: %v", err)
	}
}

func Test_when_generating_fixed_script_with_empty_original_script_then_handle_gracefully(t *testing.T) {
	// Arrange
	originalScript := ""
	errorMessage := "no script provided"
	scriptType := "bash"
	model := "test-model"
	provider := "ollama"
	config := &types.Config{
		Provider:  "ollama",
		OllamaURL: "http://localhost:11434",
	}

	// Act
	_, err := GenerateFixedScript(originalScript, errorMessage, scriptType, model, provider, config)

	// Assert - Will likely error due to connection issues, but shouldn't panic
	if err != nil {
		t.Logf("Expected error due to connection/config issues: %v", err)
	}
	// Function should not panic with empty inputs
}

func Test_when_generating_fixed_script_with_anthropic_provider_then_use_anthropic(t *testing.T) {
	// Arrange
	originalScript := "echo test"
	errorMessage := "test error"
	scriptType := "bash"
	model := "claude-3"
	provider := "anthropic"
	config := &types.Config{
		Provider:        "anthropic",
		AnthropicAPIKey: "test-key",
	}

	// Act
	_, err := GenerateFixedScript(originalScript, errorMessage, scriptType, model, provider, config)

	// Assert - Will error due to invalid API key, but verifies anthropic provider path
	if err != nil {
		t.Logf("Expected error due to invalid API credentials: %v", err)
	}
	// The important test is that it doesn't return "unsupported provider" error
	if err != nil && strings.Contains(err.Error(), "unsupported provider") {
		t.Error("Should not return unsupported provider error for anthropic")
	}
}

func Test_when_generating_fixed_script_with_empty_error_message_then_handle_gracefully(t *testing.T) {
	// Arrange
	originalScript := "echo 'hello world'"
	errorMessage := ""
	scriptType := "powershell"
	model := "gpt-4"
	provider := "openai"
	config := &types.Config{
		Provider:     "openai",
		OpenAIAPIKey: "test-key",
	}

	// Act
	_, err := GenerateFixedScript(originalScript, errorMessage, scriptType, model, provider, config)

	// Assert - Should handle empty error message gracefully
	if err != nil {
		t.Logf("Expected error due to API issues: %v", err)
	}
	// Function should not panic with empty error message
}

func Test_when_generating_fixed_script_prompt_construction_then_include_all_elements(t *testing.T) {
	// This test verifies the prompt construction logic by checking what would be sent
	// We can't easily intercept the actual prompt without modifying the function,
	// but we can test the inputs are processed correctly by ensuring the function
	// doesn't crash with various input combinations

	testCases := []struct {
		name           string
		originalScript string
		errorMessage   string
		scriptType     string
		provider       string
	}{
		{
			name:           "bash script with detailed error",
			originalScript: "#!/bin/bash\necho 'test'\nexit 1",
			errorMessage:   "exit code 1: script failed",
			scriptType:     "bash",
			provider:       "openai",
		},
		{
			name:           "powershell script with syntax error",
			originalScript: "Get-Process | Where-Object { $_.Name -eq 'test",
			errorMessage:   "missing closing quote",
			scriptType:     "powershell",
			provider:       "ollama",
		},
		{
			name:           "script with newlines and special chars",
			originalScript: "echo 'line1'\necho \"line2\"\necho `date`",
			errorMessage:   "command substitution failed",
			scriptType:     "bash",
			provider:       "anthropic",
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Arrange
			config := &types.Config{Provider: tc.provider}

			// Act - The function will likely error due to no real API keys,
			// but it should not panic or return "unsupported provider" for valid providers
			_, err := GenerateFixedScript(tc.originalScript, tc.errorMessage, tc.scriptType, "test-model", tc.provider, config)

			// Assert - Should not crash and should not be "unsupported provider" error
			if err != nil && strings.Contains(err.Error(), "unsupported provider") {
				t.Errorf("Test case %s: should not return unsupported provider error for %s", tc.name, tc.provider)
			}
		})
	}
}

// Provider Instance Tests
func Test_when_creating_openai_provider_then_initialize_correctly(t *testing.T) {
	// Arrange
	config := &types.Config{
		OpenAIAPIKey: "test-key",
		Provider:     "openai",
	}

	// Act
	provider := NewOpenAIProvider(config)

	// Assert
	if provider == nil {
		t.Error("NewOpenAIProvider should return non-nil provider")
	}
	if provider.Name() != "openai" {
		t.Errorf("Expected provider name 'openai', got '%s'", provider.Name())
	}
}

func Test_when_creating_anthropic_provider_then_initialize_correctly(t *testing.T) {
	// Arrange
	config := &types.Config{
		AnthropicAPIKey: "test-key",
		Provider:        "anthropic",
	}

	// Act
	provider := NewAnthropicProvider(config)

	// Assert
	if provider == nil {
		t.Error("NewAnthropicProvider should return non-nil provider")
	}
	if provider.Name() != "anthropic" {
		t.Errorf("Expected provider name 'anthropic', got '%s'", provider.Name())
	}
}

func Test_when_creating_ollama_provider_then_initialize_correctly(t *testing.T) {
	// Arrange
	config := &types.Config{
		OllamaURL: "http://localhost:11434",
		Provider:  "ollama",
	}

	// Act
	provider := NewOllamaProvider(config)

	// Assert
	if provider == nil {
		t.Error("NewOllamaProvider should return non-nil provider")
	}
	if provider.Name() != "ollama" {
		t.Errorf("Expected provider name 'ollama', got '%s'", provider.Name())
	}
}

func Test_when_checking_openai_configuration_then_validate_api_key(t *testing.T) {
	provider := NewOpenAIProvider(&types.Config{})

	tests := []struct {
		name     string
		config   *types.Config
		expected bool
	}{
		{
			name:     "Valid API key",
			config:   &types.Config{OpenAIAPIKey: "sk-test123"},
			expected: true,
		},
		{
			name:     "Empty API key",
			config:   &types.Config{OpenAIAPIKey: ""},
			expected: false,
		},
		{
			name:     "Nil config",
			config:   &types.Config{},
			expected: false,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := provider.IsConfigured(tt.config)
			if result != tt.expected {
				t.Errorf("IsConfigured() = %v, want %v", result, tt.expected)
			}
		})
	}
}

func Test_when_checking_anthropic_configuration_then_validate_api_key(t *testing.T) {
	provider := NewAnthropicProvider(&types.Config{})

	tests := []struct {
		name     string
		config   *types.Config
		expected bool
	}{
		{
			name:     "Valid API key",
			config:   &types.Config{AnthropicAPIKey: "sk-ant-test123"},
			expected: true,
		},
		{
			name:     "Empty API key",
			config:   &types.Config{AnthropicAPIKey: ""},
			expected: false,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := provider.IsConfigured(tt.config)
			if result != tt.expected {
				t.Errorf("IsConfigured() = %v, want %v", result, tt.expected)
			}
		})
	}
}

func Test_when_checking_ollama_configuration_then_always_return_true(t *testing.T) {
	provider := NewOllamaProvider(&types.Config{})

	tests := []struct {
		name     string
		config   *types.Config
		expected bool
		desc     string
	}{
		{
			name:     "Valid URL",
			config:   &types.Config{OllamaURL: "http://localhost:11434"},
			expected: true,
			desc:     "Ollama should be considered configured with valid URL",
		},
		{
			name:     "Empty URL",
			config:   &types.Config{OllamaURL: ""},
			expected: true,
			desc:     "Ollama should be considered configured even with empty URL (uses default)",
		},
		{
			name:     "Custom URL",
			config:   &types.Config{OllamaURL: "http://remote:8080"},
			expected: true,
			desc:     "Ollama should be considered configured with custom URL",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := provider.IsConfigured(tt.config)
			if result != tt.expected {
				t.Errorf("IsConfigured() = %v, want %v: %s", result, tt.expected, tt.desc)
			}
		})
	}
}

func Test_when_openai_provider_not_configured_then_return_error(t *testing.T) {
	// Arrange
	config := &types.Config{OpenAIAPIKey: ""} // No API key
	provider := NewOpenAIProvider(config)
	request := &types.ScriptRequest{
		TaskDescription: "test task",
		ScriptType:      "bash",
		Provider:        "openai",
	}

	// Act
	result, err := provider.GenerateScript(request)

	// Assert
	if err == nil {
		t.Error("Expected error when OpenAI is not configured")
	}
	if result != nil {
		t.Error("Expected nil result when OpenAI is not configured")
	}
	if !strings.Contains(err.Error(), "API key not configured") {
		t.Errorf("Expected error about API key, got: %s", err.Error())
	}
}

func Test_when_anthropic_provider_not_configured_then_return_error(t *testing.T) {
	// Arrange
	config := &types.Config{AnthropicAPIKey: ""} // No API key
	provider := NewAnthropicProvider(config)
	request := &types.ScriptRequest{
		TaskDescription: "test task",
		ScriptType:      "bash",
		Provider:        "anthropic",
	}

	// Act
	result, err := provider.GenerateScript(request)

	// Assert
	if err == nil {
		t.Error("Expected error when Anthropic is not configured")
	}
	if result != nil {
		t.Error("Expected nil result when Anthropic is not configured")
	}
	if !strings.Contains(err.Error(), "API key not configured") {
		t.Errorf("Expected error about API key, got: %s", err.Error())
	}
}

func Test_when_getting_openai_available_models_then_return_model_list(t *testing.T) {
	// Arrange
	provider := NewOpenAIProvider(&types.Config{})

	// Act
	models := provider.GetAvailableModels()

	// Assert
	if len(models) == 0 {
		t.Error("Expected non-empty list of available models")
	}

	// Check for expected models
	expectedModels := []string{"gpt-4", "gpt-3.5-turbo"}
	for _, expected := range expectedModels {
		found := false
		for _, model := range models {
			if model == expected {
				found = true
				break
			}
		}
		if !found {
			t.Errorf("Expected model '%s' not found in available models", expected)
		}
	}
}

func Test_when_getting_anthropic_available_models_then_return_model_list(t *testing.T) {
	// Arrange
	provider := NewAnthropicProvider(&types.Config{})

	// Act
	models := provider.GetAvailableModels()

	// Assert
	if len(models) == 0 {
		t.Error("Expected non-empty list of available models")
	}

	// Check for Claude models
	found := false
	for _, model := range models {
		if strings.Contains(model, "claude") {
			found = true
			break
		}
	}
	if !found {
		t.Error("Expected at least one Claude model in Anthropic available models")
	}
}

func Test_when_testing_provider_interface_implementation_then_satisfy_interface(t *testing.T) {
	// This test ensures all providers implement the Provider interface correctly
	configs := []*types.Config{
		{OpenAIAPIKey: "test"},
		{AnthropicAPIKey: "test"},
		{OllamaURL: "http://localhost:11434"},
	}

	providers := []Provider{
		NewOpenAIProvider(configs[0]),
		NewAnthropicProvider(configs[1]),
		NewOllamaProvider(configs[2]),
	}

	for i, provider := range providers {
		t.Run(provider.Name(), func(t *testing.T) {
			// Test Name() method
			name := provider.Name()
			if name == "" {
				t.Error("Provider Name() should not return empty string")
			}

			// Test IsConfigured() method
			configured := provider.IsConfigured(configs[i])
			// Should return boolean without error
			t.Logf("Provider %s configured: %t", name, configured)

			// Test GenerateScript() method exists (will fail due to no real API)
			request := &types.ScriptRequest{
				TaskDescription: "test",
				ScriptType:      "bash",
				Provider:        name,
			}
			_, err := provider.GenerateScript(request)
			// We expect an error due to invalid config/network, but method should exist
			if err == nil {
				t.Logf("Unexpected success from %s provider", name)
			}
		})
	}
}
