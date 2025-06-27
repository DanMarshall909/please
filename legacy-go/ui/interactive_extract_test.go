package ui

import "testing"

func Test_when_extracting_valid_field_then_return_correct_value(t *testing.T) {
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

func Test_when_extracting_script_field_then_return_correct_value(t *testing.T) {
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

func Test_when_extracting_nonexistent_field_then_return_empty_string(t *testing.T) {
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

func Test_when_extracting_from_empty_content_then_return_empty_string(t *testing.T) {
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

func Test_when_extracting_field_with_escaped_quotes_then_return_unescaped_value(t *testing.T) {
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

func Test_when_extracting_field_with_malformed_json_then_return_empty_string(t *testing.T) {
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
