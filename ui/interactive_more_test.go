package ui

import (
	"strings"
	"testing"

	"please/types"
)

func validateTaskInput(input string) types.ValidationResult {
	trimmed := strings.TrimSpace(input)
	if trimmed == "" {
		return types.ValidationResult{
			IsValid: false,
			IsEmpty: true,
			Message: "❌ No task description provided. Please enter a task.",
		}
	}
	return types.ValidationResult{
		IsValid: true,
		IsEmpty: false,
		Message: generateStatusMessage(trimmed, false),
	}
}

func generateStatusMessage(task string, isEmpty bool) string {
	if isEmpty {
		return "❌ No task description provided. Please enter a task."
	}
	return "🔄 Generating script for: " + task
}

func TestGivenEmptyTaskInput_WhenValidating_ThenReturnsWarningMessage(t *testing.T) {
	result := validateTaskInput("")
	expected := "❌ No task description provided. Please enter a task."
	if result.Message != expected {
		t.Errorf("Expected warning message, got: %s", result.Message)
	}
	if result.IsValid {
		t.Error("Expected IsValid to be false for empty input")
	}
	if !result.IsEmpty {
		t.Error("Expected IsEmpty to be true for empty input")
	}
}

func TestGivenValidTaskInput_WhenValidating_ThenReturnsSuccessMessage(t *testing.T) {
	result := validateTaskInput("test task")
	if !result.IsValid {
		t.Error("Expected valid task to be accepted")
	}
	expected := "🔄 Generating script for: test task"
	if result.Message != expected {
		t.Errorf("Expected success message, got: %s", result.Message)
	}
	if result.IsEmpty {
		t.Error("Expected IsEmpty to be false for valid input")
	}
}

func TestGivenEmptyTask_WhenGeneratingStatus_ThenShowsWarning(t *testing.T) {
	msg := generateStatusMessage("", true)
	if !strings.Contains(msg, "No task description provided") {
		t.Errorf("Expected warning message, got: %s", msg)
	}
}

func TestGivenValidTask_WhenGeneratingStatus_ThenShowsGenerationMessage(t *testing.T) {
	msg := generateStatusMessage("my task", false)
	if !strings.Contains(msg, "Generating script for: my task") {
		t.Errorf("Expected generation message, got: %s", msg)
	}
}

// --- Menu choice parsing logic and tests ---
func parseMenuChoice(choice rune) types.MenuAction {
	switch choice {
	case '1':
		return types.MenuAction{Type: "generate", Valid: true}
	case '2':
		return types.MenuAction{Type: "run_last", Valid: true}
	case '3':
		return types.MenuAction{Type: "help", Valid: true}
	case '4':
		return types.MenuAction{Type: "exit", Valid: true}
	default:
		return types.MenuAction{Type: "invalid", Valid: false}
	}
}

func TestGivenMenuChoice1_WhenParsing_ThenReturnsGenerateAction(t *testing.T) {
	action := parseMenuChoice('1')
	if action.Type != "generate" || !action.Valid {
		t.Errorf("Expected valid generate action, got %+v", action)
	}
}

func TestGivenMenuChoice2_WhenParsing_ThenReturnsRunLastAction(t *testing.T) {
	action := parseMenuChoice('2')
	if action.Type != "run_last" || !action.Valid {
		t.Errorf("Expected valid run_last action, got %+v", action)
	}
}

func TestGivenInvalidMenuChoice_WhenParsing_ThenReturnsInvalidAction(t *testing.T) {
	action := parseMenuChoice('x')
	if action.Type != "invalid" || action.Valid {
		t.Errorf("Expected invalid action, got %+v", action)
	}
}
