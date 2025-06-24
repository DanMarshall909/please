package ui

import "testing"

// Test handleMainMenuChoice function
func Test_when_choice_is_enter_then_return_true_for_exit(t *testing.T) {
	// Arrange
	choice := "\r"

	// Act
	result := handleMainMenuChoice(choice)

	// Assert
	if !result {
		t.Error("Expected handleMainMenuChoice to return true for Enter key")
	}
}

func Test_when_choice_is_newline_then_return_true_for_exit(t *testing.T) {
	// Arrange
	choice := "\n"

	// Act
	result := handleMainMenuChoice(choice)

	// Assert
	if !result {
		t.Error("Expected handleMainMenuChoice to return true for newline")
	}
}

func Test_when_choice_is_empty_then_return_false_to_continue(t *testing.T) {
	// Arrange
	choice := ""

	// Act
	result := handleMainMenuChoice(choice)

	// Assert
	if result {
		t.Error("Expected handleMainMenuChoice to return false for empty choice")
	}
}

func Test_when_choice_is_space_then_return_false_to_continue(t *testing.T) {
	// Arrange
	choice := " "

	// Act
	result := handleMainMenuChoice(choice)

	// Assert
	if result {
		t.Error("Expected handleMainMenuChoice to return false for space")
	}
}

func Test_when_choice_is_6_then_return_true_for_exit(t *testing.T) {
	// Arrange
	choice := "6"

	// Act
	result := handleMainMenuChoice(choice)

	// Assert
	if !result {
		t.Error("Expected handleMainMenuChoice to return true for choice '6' (exit)")
	}
}

func Test_when_choice_is_1_then_return_false_to_continue(t *testing.T) {
	// Arrange
	choice := "1"

	// Act
	result := handleMainMenuChoice(choice)

	// Assert
	if result {
		t.Error("Expected handleMainMenuChoice to return false for choice '1' (help)")
	}
}

func Test_when_choice_is_invalid_then_return_false_to_continue(t *testing.T) {
	// Arrange
	choice := "invalid"

	// Act
	result := handleMainMenuChoice(choice)

	// Assert
	if result {
		t.Error("Expected handleMainMenuChoice to return false for invalid choice")
	}
}
