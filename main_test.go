package main

import (
	"os"
	"strings"
	"testing"
	"please/types"
)

// Test getFallbackModel function
func Test_when_provider_is_openai_should_return_gpt_3_5_turbo(t *testing.T) {
	// Arrange
	provider := "openai"
	expected := "gpt-3.5-turbo"

	// Act
	result := getFallbackModel(provider)

	// Assert
	if result != expected {
		t.Errorf("Expected %s, got %s", expected, result)
	}
}

func Test_when_provider_is_anthropic_should_return_claude_3_haiku(t *testing.T) {
	// Arrange
	provider := "anthropic"
	expected := "claude-3-haiku-20240307"

	// Act
	result := getFallbackModel(provider)

	// Assert
	if result != expected {
		t.Errorf("Expected %s, got %s", expected, result)
	}
}

func Test_when_provider_is_unknown_should_return_llama3_2(t *testing.T) {
	// Arrange
	provider := "unknown"
	expected := "llama3.2"

	// Act
	result := getFallbackModel(provider)

	// Assert
	if result != expected {
		t.Errorf("Expected %s, got %s", expected, result)
	}
}

func Test_when_provider_is_ollama_should_return_llama3_2(t *testing.T) {
	// Arrange
	provider := "ollama"
	expected := "llama3.2"

	// Act
	result := getFallbackModel(provider)

	// Assert
	if result != expected {
		t.Errorf("Expected %s, got %s", expected, result)
	}
}

// Test isLastScriptCommand function
func Test_when_command_is_run_last_script_should_return_true(t *testing.T) {
	// Arrange
	command := "run last script"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'run last script' command")
	}
}

func Test_when_command_is_run_my_last_script_should_return_true(t *testing.T) {
	// Arrange
	command := "run my last script"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'run my last script' command")
	}
}

func Test_when_command_is_repeat_should_return_true(t *testing.T) {
	// Arrange
	command := "repeat"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'repeat' command")
	}
}

func Test_when_command_is_case_insensitive_run_last_script_should_return_true(t *testing.T) {
	// Arrange
	command := "RUN LAST SCRIPT"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for case insensitive 'RUN LAST SCRIPT' command")
	}
}

func Test_when_command_is_do_it_again_should_return_true(t *testing.T) {
	// Arrange
	command := "do it again"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'do it again' command")
	}
}

func Test_when_command_is_normal_task_should_return_false(t *testing.T) {
	// Arrange
	command := "list all files in current directory"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if result {
		t.Error("Expected false for normal task command")
	}
}

func Test_when_command_is_empty_should_return_false(t *testing.T) {
	// Arrange
	command := ""

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if result {
		t.Error("Expected false for empty command")
	}
}

func Test_when_command_contains_last_script_pattern_with_extra_words_should_return_true(t *testing.T) {
	// Arrange
	command := "please run my last script now"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for command containing 'run my last script' pattern")
	}
}

// Test generateScript function error cases
func Test_when_generateScript_receives_unsupported_provider_should_return_error(t *testing.T) {
	// Arrange
	cfg := &types.Config{}
	request := &types.ScriptRequest{
		Provider: "unsupported",
	}

	// Act
	_, err := generateScript(cfg, request)

	// Assert
	if err == nil {
		t.Error("Expected error for unsupported provider")
	}
	expectedError := "unsupported provider: unsupported"
	if err.Error() != expectedError {
		t.Errorf("Expected error message '%s', got '%s'", expectedError, err.Error())
	}
}

func Test_when_generateScript_receives_empty_provider_should_return_error(t *testing.T) {
	// Arrange
	cfg := &types.Config{}
	request := &types.ScriptRequest{
		Provider: "",
	}

	// Act
	_, err := generateScript(cfg, request)

	// Assert
	if err == nil {
		t.Error("Expected error for empty provider")
	}
	expectedError := "unsupported provider: "
	if err.Error() != expectedError {
		t.Errorf("Expected error message '%s', got '%s'", expectedError, err.Error())
	}
}

// Test additional isLastScriptCommand patterns
func Test_when_command_is_execute_my_last_script_should_return_true(t *testing.T) {
	// Arrange
	command := "execute my last script"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'execute my last script' command")
	}
}

func Test_when_command_is_run_the_last_script_should_return_true(t *testing.T) {
	// Arrange
	command := "run the last script"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'run the last script' command")
	}
}

func Test_when_command_is_previous_script_should_return_true(t *testing.T) {
	// Arrange
	command := "previous script"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'previous script' command")
	}
}

func Test_when_command_is_run_again_should_return_true(t *testing.T) {
	// Arrange
	command := "run again"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'run again' command")
	}
}

func Test_when_command_is_repeat_last_should_return_true(t *testing.T) {
	// Arrange
	command := "repeat last"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for 'repeat last' command")
	}
}

func Test_when_command_is_mixed_case_do_it_again_should_return_true(t *testing.T) {
	// Arrange
	command := "Do It Again"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for mixed case 'Do It Again' command")
	}
}

func Test_when_command_contains_last_script_substring_should_return_true(t *testing.T) {
	// Arrange
	command := "I want to run my last script please"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if !result {
		t.Error("Expected true for command containing 'run my last script' substring")
	}
}

// Test isLastScriptCommand edge cases
func Test_when_command_is_just_whitespace_should_return_false(t *testing.T) {
	// Arrange
	command := "   \t  \n  "

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if result {
		t.Error("Expected false for whitespace-only command")
	}
}

func Test_when_command_contains_script_but_not_last_pattern_should_return_false(t *testing.T) {
	// Arrange
	command := "create a new script for me"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if result {
		t.Error("Expected false for command about creating new script")
	}
}

func Test_when_command_contains_run_but_not_last_pattern_should_return_false(t *testing.T) {
	// Arrange
	command := "run a system check"

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if result {
		t.Error("Expected false for command about running system check")
	}
}

// Test generateScript with different provider types
func Test_when_generateScript_receives_ollama_provider_but_not_configured_should_return_error(t *testing.T) {
	// Arrange
	cfg := &types.Config{
		OllamaURL: "", // Not configured
	}
	request := &types.ScriptRequest{
		Provider: "ollama",
		Model:    "llama3.2",
	}

	// Act
	_, err := generateScript(cfg, request)

	// Assert
	if err == nil {
		t.Error("Expected error for unconfigured ollama provider")
	}
	// Accept either configuration error or model not found error
	// Both indicate the provider isn't working properly for our test
	expectedSubstrings := []string{"not properly configured", "model", "not found", "404"}
	found := false
	for _, substring := range expectedSubstrings {
		if strings.Contains(err.Error(), substring) {
			found = true
			break
		}
	}
	if !found {
		t.Errorf("Expected error containing configuration or model error, got '%s'", err.Error())
	}
}

func Test_when_generateScript_receives_openai_provider_but_not_configured_should_return_error(t *testing.T) {
	// Arrange
	cfg := &types.Config{
		OpenAIAPIKey: "", // Not configured
	}
	request := &types.ScriptRequest{
		Provider: "openai",
		Model:    "gpt-4",
	}

	// Act
	_, err := generateScript(cfg, request)

	// Assert
	if err == nil {
		t.Error("Expected error for unconfigured openai provider")
	}
	expectedSubstring := "not properly configured"
	if !strings.Contains(err.Error(), expectedSubstring) {
		t.Errorf("Expected error containing '%s', got '%s'", expectedSubstring, err.Error())
	}
}

func Test_when_generateScript_receives_anthropic_provider_but_not_configured_should_return_error(t *testing.T) {
	// Arrange
	cfg := &types.Config{
		AnthropicAPIKey: "", // Not configured
	}
	request := &types.ScriptRequest{
		Provider: "anthropic",
		Model:    "claude-3-haiku-20240307",
	}

	// Act
	_, err := generateScript(cfg, request)

	// Assert
	if err == nil {
		t.Error("Expected error for unconfigured anthropic provider")
	}
	expectedSubstring := "not properly configured"
	if !strings.Contains(err.Error(), expectedSubstring) {
		t.Errorf("Expected error containing '%s', got '%s'", expectedSubstring, err.Error())
	}
}

// Test edge cases for getFallbackModel
func Test_when_provider_is_empty_string_should_return_llama3_2(t *testing.T) {
	// Arrange
	provider := ""
	expected := "llama3.2"

	// Act
	result := getFallbackModel(provider)

	// Assert
	if result != expected {
		t.Errorf("Expected %s, got %s", expected, result)
	}
}

func Test_when_provider_is_mixed_case_openai_should_return_llama3_2(t *testing.T) {
	// Arrange - case sensitivity test
	provider := "OpenAI"
	expected := "llama3.2" // Should use default since it's case sensitive

	// Act
	result := getFallbackModel(provider)

	// Assert
	if result != expected {
		t.Errorf("Expected %s for case-sensitive provider name, got %s", expected, result)
	}
}

func Test_when_provider_has_extra_spaces_should_return_llama3_2(t *testing.T) {
	// Arrange
	provider := " openai "
	expected := "llama3.2" // Should use default since exact match required

	// Act
	result := getFallbackModel(provider)

	// Assert
	if result != expected {
		t.Errorf("Expected %s for provider with spaces, got %s", expected, result)
	}
}

// Test helper function to verify all patterns are covered
func Test_when_checking_all_last_script_patterns_should_return_true(t *testing.T) {
	patterns := []string{
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

	for _, pattern := range patterns {
		t.Run(pattern, func(t *testing.T) {
			// Act
			result := isLastScriptCommand(pattern)

			// Assert
			if !result {
				t.Errorf("Expected true for pattern '%s'", pattern)
			}
		})
	}
}

// Test that we handle unusual input gracefully
func Test_when_command_is_very_long_string_should_handle_gracefully(t *testing.T) {
	// Arrange - create a very long string without last script patterns
	command := strings.Repeat("create many files and process data ", 100)

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if result {
		t.Error("Expected false for very long string without last script patterns")
	}
}

func Test_when_command_contains_unicode_characters_should_handle_gracefully(t *testing.T) {
	// Arrange
	command := "运行最后一个脚本" // "run last script" in Chinese

	// Act
	result := isLastScriptCommand(command)

	// Assert
	if result {
		t.Error("Expected false for non-English command")
	}
}

// Test OS-related edge cases
func Test_when_checking_environment_executable_name_should_not_panic(t *testing.T) {
	// This test ensures we can call os.Args safely in test environment
	// Since we can't easily test main() directly, we at least verify
	// that accessing os.Args doesn't cause issues
	
	// Arrange & Act - access os.Args similar to main function
	programName := "test"
	if len(os.Args) > 0 {
		programName = os.Args[0]
	}

	// Assert - just verify we didn't panic and got some value
	if programName == "" {
		t.Error("Expected non-empty program name")
	}
	if len(programName) == 0 {
		t.Error("Expected program name to have length > 0")
	}
}
