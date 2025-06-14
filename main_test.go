package main

import (
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
