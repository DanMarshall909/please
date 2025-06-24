package ui

import "testing"

// Test determineRiskLevel function
func Test_when_warnings_contain_red_prefix_then_return_red(t *testing.T) {
	// Arrange
	warnings := []string{
		"🟡 Medium risk operation",
		"⛔ CRITICAL: High risk detected",
		"🟡 Another medium warning",
	}

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "red" {
		t.Errorf("Expected 'red', got '%s'", result)
	}
}

func Test_when_warnings_contain_only_red_color_prefix_then_return_red(t *testing.T) {
	// Arrange
	warnings := []string{
		"🔴 High risk operation detected",
	}

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "red" {
		t.Errorf("Expected 'red', got '%s'", result)
	}
}

func Test_when_warnings_contain_only_yellow_prefix_then_return_yellow(t *testing.T) {
	// Arrange
	warnings := []string{
		"🟡 Medium risk operation",
		"🟡 Another medium warning",
	}

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "yellow" {
		t.Errorf("Expected 'yellow', got '%s'", result)
	}
}

func Test_when_warnings_contain_no_risk_prefixes_then_return_green(t *testing.T) {
	// Arrange
	warnings := []string{
		"✅ Safe operation",
		"ℹ️ Information message",
		"This is a normal warning",
	}

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "green" {
		t.Errorf("Expected 'green', got '%s'", result)
	}
}

func Test_when_warnings_is_empty_then_return_green(t *testing.T) {
	// Arrange
	warnings := []string{}

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "green" {
		t.Errorf("Expected 'green', got '%s'", result)
	}
}

func Test_when_warnings_is_nil_then_return_green(t *testing.T) {
	// Arrange
	var warnings []string = nil

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "green" {
		t.Errorf("Expected 'green', got '%s'", result)
	}
}

func Test_when_warnings_contain_both_red_and_yellow_then_return_red(t *testing.T) {
	// Arrange
	warnings := []string{
		"🟡 Medium risk operation",
		"⛔ CRITICAL: High risk detected",
		"🟡 Another medium warning",
		"🔴 High risk operation",
	}

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "red" {
		t.Errorf("Expected 'red' when both red and yellow present, got '%s'", result)
	}
}
