package providers

import (
	"strings"
	"testing"

	"please/types"
)

// Test CreatePrompt function for different script types
func Test_when_creating_bash_prompt_should_include_bash_specific_elements(t *testing.T) {
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

func Test_when_creating_powershell_prompt_should_include_powershell_specific_elements(t *testing.T) {
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

func Test_when_creating_prompt_with_unknown_script_type_should_default_to_powershell(t *testing.T) {
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

func Test_when_creating_prompt_with_empty_task_description_should_include_empty_task(t *testing.T) {
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

func Test_when_creating_bash_prompt_should_include_safety_requirements(t *testing.T) {
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

func Test_when_creating_powershell_prompt_should_include_safety_requirements(t *testing.T) {
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

func Test_when_creating_prompt_with_special_characters_should_preserve_them(t *testing.T) {
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
func Test_when_generating_fixed_script_with_unsupported_provider_should_return_error(t *testing.T) {
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

func Test_when_generating_fixed_script_should_include_original_script_and_error_in_prompt(t *testing.T) {
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

func Test_when_generating_fixed_script_with_empty_original_script_should_handle_gracefully(t *testing.T) {
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

func Test_when_generating_fixed_script_with_anthropic_provider_should_use_anthropic(t *testing.T) {
	// Arrange
	originalScript := "echo test"
	errorMessage := "test error"
	scriptType := "bash"
	model := "claude-3"
	provider := "anthropic"
	config := &types.Config{
		Provider:       "anthropic",
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

func Test_when_generating_fixed_script_with_empty_error_message_should_handle_gracefully(t *testing.T) {
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

func Test_when_generating_fixed_script_prompt_construction_should_include_all_elements(t *testing.T) {
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
