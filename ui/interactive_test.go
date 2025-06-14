package ui

import (
	"testing"
)

// Test determineRiskLevel function
func Test_when_warnings_contain_red_prefix_should_return_red(t *testing.T) {
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

func Test_when_warnings_contain_only_red_color_prefix_should_return_red(t *testing.T) {
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

func Test_when_warnings_contain_only_yellow_prefix_should_return_yellow(t *testing.T) {
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

func Test_when_warnings_contain_no_risk_prefixes_should_return_green(t *testing.T) {
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

func Test_when_warnings_is_empty_should_return_green(t *testing.T) {
	// Arrange
	warnings := []string{}

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "green" {
		t.Errorf("Expected 'green', got '%s'", result)
	}
}

func Test_when_warnings_is_nil_should_return_green(t *testing.T) {
	// Arrange
	var warnings []string = nil

	// Act
	result := determineRiskLevel(warnings)

	// Assert
	if result != "green" {
		t.Errorf("Expected 'green', got '%s'", result)
	}
}

func Test_when_warnings_contain_both_red_and_yellow_should_return_red(t *testing.T) {
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

// Test extractJSONField function
func Test_when_extracting_valid_field_should_return_correct_value(t *testing.T) {
	// Arrange
	content := `{
  "task_description": "list all files",
  "script": "ls -la",
  "script_type": "bash"
}`
	field := "task_description"
	expected := "list all files"

	// Act
	result := extractJSONField(content, field)

	// Assert
	if result != expected {
		t.Errorf("Expected '%s', got '%s'", expected, result)
	}
}

func Test_when_extracting_script_field_should_return_correct_value(t *testing.T) {
	// Arrange
	content := `{
  "task_description": "create backup",
  "script": "cp /home/user /backup",
  "script_type": "bash"
}`
	field := "script"
	expected := "cp /home/user /backup"

	// Act
	result := extractJSONField(content, field)

	// Assert
	if result != expected {
		t.Errorf("Expected '%s', got '%s'", expected, result)
	}
}

func Test_when_extracting_nonexistent_field_should_return_empty_string(t *testing.T) {
	// Arrange
	content := `{
  "task_description": "list all files",
  "script": "ls -la"
}`
	field := "nonexistent_field"

	// Act
	result := extractJSONField(content, field)

	// Assert
	if result != "" {
		t.Errorf("Expected empty string for nonexistent field, got '%s'", result)
	}
}

func Test_when_extracting_from_empty_content_should_return_empty_string(t *testing.T) {
	// Arrange
	content := ""
	field := "task_description"

	// Act
	result := extractJSONField(content, field)

	// Assert
	if result != "" {
		t.Errorf("Expected empty string for empty content, got '%s'", result)
	}
}

func Test_when_extracting_field_with_escaped_quotes_should_return_unescaped_value(t *testing.T) {
	// Arrange
	content := `{
  "task_description": "create a \"hello world\" script",
  "script": "echo \"hello\""
}`
	field := "task_description"
	expected := `create a "hello world" script`

	// Act
	result := extractJSONField(content, field)

	// Assert
	if result != expected {
		t.Errorf("Expected '%s', got '%s'", expected, result)
	}
}

func Test_when_extracting_field_with_malformed_json_should_return_empty_string(t *testing.T) {
	// Arrange
	content := `{
  "task_description": "incomplete json field`
	field := "task_description"

	// Act
	result := extractJSONField(content, field)

	// Assert
	if result != "" {
		t.Errorf("Expected empty string for malformed JSON, got '%s'", result)
	}
}
