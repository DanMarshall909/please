package script

import (
	"strings"
	"testing"
	"time"
)

// TestParseTestFailures tests the test failure parsing functionality
func TestParseTestFailures(t *testing.T) {
	tm := &TestMonitor{}
	
	// Sample Go test output with failures
	testOutput := `=== RUN   TestExample
=== RUN   TestExample/should_pass
=== RUN   TestExample/should_fail
    example_test.go:15: Expected 5, got 3
    example_test.go:16: Values should be equal
--- FAIL: TestExample/should_fail (0.00s)
=== RUN   TestAnother
    another_test.go:25: Connection failed: timeout
--- FAIL: TestAnother (0.01s)
FAIL	example/package	0.012s`

	failures := tm.parseTestFailures(testOutput, "go test -v")
	
	if len(failures) != 2 {
		t.Errorf("Expected 2 failures, got %d", len(failures))
	}
	
	// Check first failure
	if failures[0].TestName != "TestExample/should_fail" {
		t.Errorf("Expected test name 'TestExample/should_fail', got '%s'", failures[0].TestName)
	}
	
	if failures[0].SourceFile != "example_test.go" {
		t.Errorf("Expected source file 'example_test.go', got '%s'", failures[0].SourceFile)
	}
	
	if failures[0].LineNumber != 15 {
		t.Errorf("Expected line number 15, got %d", failures[0].LineNumber)
	}
	
	if !strings.Contains(failures[0].ErrorMessage, "Expected 5, got 3") {
		t.Errorf("Expected error message to contain 'Expected 5, got 3', got '%s'", failures[0].ErrorMessage)
	}
	
	// Check second failure
	if failures[1].TestName != "TestAnother" {
		t.Errorf("Expected test name 'TestAnother', got '%s'", failures[1].TestName)
	}
	
	if failures[1].SourceFile != "another_test.go" {
		t.Errorf("Expected source file 'another_test.go', got '%s'", failures[1].SourceFile)
	}
	
	if failures[1].LineNumber != 25 {
		t.Errorf("Expected line number 25, got %d", failures[1].LineNumber)
	}
}

// TestExtractLineNumber tests line number extraction
func TestExtractLineNumber(t *testing.T) {
	tm := &TestMonitor{}
	
	tests := []struct {
		location string
		expected int
	}{
		{"test.go:15", 15},
		{"path/to/test.go:123", 123},
		{"test.go:0", 0},
		{"test.go:", 0},
		{"test.go", 0},
		{"", 0},
	}
	
	for _, test := range tests {
		result := tm.extractLineNumber(test.location)
		if result != test.expected {
			t.Errorf("For location '%s', expected %d, got %d", test.location, test.expected, result)
		}
	}
}

// TestExtractFilePath tests file path extraction
func TestExtractFilePath(t *testing.T) {
	tm := &TestMonitor{}
	
	tests := []struct {
		location string
		expected string
	}{
		{"test.go:15", "test.go"},
		{"path/to/test.go:123", "path/to/test.go"},
		{"test.go:", "test.go"},
		{"test.go", "test.go"},
		{"", ""},
	}
	
	for _, test := range tests {
		result := tm.extractFilePath(test.location)
		if result != test.expected {
			t.Errorf("For location '%s', expected '%s', got '%s'", test.location, test.expected, result)
		}
	}
}

// TestTruncateOutput tests output truncation
func TestTruncateOutput(t *testing.T) {
	tm := &TestMonitor{}
	
	// Test short text (no truncation)
	shortText := "This is a short text"
	result := tm.truncateOutput(shortText, 100)
	if result != shortText {
		t.Errorf("Short text should not be truncated")
	}
	
	// Test long text (should be truncated)
	longText := strings.Repeat("a", 200)
	result = tm.truncateOutput(longText, 50)
	if len(result) > 65 { // 50 + "... (truncated)" length
		t.Errorf("Long text should be truncated")
	}
	
	if !strings.Contains(result, "... (truncated)") {
		t.Errorf("Truncated text should contain truncation marker")
	}
}

// TestNewTestMonitor tests monitor creation
func TestNewTestMonitor(t *testing.T) {
	monitor := NewTestMonitor(nil, nil)
	
	if monitor == nil {
		t.Error("NewTestMonitor should not return nil")
	}
	
	if !monitor.VerboseMode {
		t.Error("VerboseMode should be true by default")
	}
	
	if !monitor.AutoAnalyze {
		t.Error("AutoAnalyze should be true by default")
	}
	
	if !monitor.SaveFailures {
		t.Error("SaveFailures should be true by default")
	}
}

// TestParseAIAnalysis tests AI response parsing
func TestParseAIAnalysis(t *testing.T) {
	tm := &TestMonitor{}
	
	// Test valid JSON response
	validJSON := `Some text before JSON {
		"summary": "Test summary",
		"root_cause": "Test root cause",
		"suggestions": ["suggestion1", "suggestion2"],
		"requires_manual": false,
		"failure_category": "logic_error"
	} some text after`
	
	analysis := tm.parseAIAnalysis(validJSON)
	
	if analysis.Summary != "Test summary" {
		t.Errorf("Expected summary 'Test summary', got '%s'", analysis.Summary)
	}
	
	if analysis.RootCause != "Test root cause" {
		t.Errorf("Expected root cause 'Test root cause', got '%s'", analysis.RootCause)
	}
	
	if len(analysis.Suggestions) != 2 {
		t.Errorf("Expected 2 suggestions, got %d", len(analysis.Suggestions))
	}
	
	if analysis.RequiresManual {
		t.Error("RequiresManual should be false")
	}
	
	if analysis.FailureCategory != "logic_error" {
		t.Errorf("Expected failure category 'logic_error', got '%s'", analysis.FailureCategory)
	}
	
	// Test invalid JSON response (should return fallback)
	invalidJSON := "This is not JSON at all"
	analysis = tm.parseAIAnalysis(invalidJSON)
	
	if analysis.Summary != "AI analysis parsing failed" {
		t.Errorf("Expected fallback summary for invalid JSON")
	}
	
	if !analysis.RequiresManual {
		t.Error("RequiresManual should be true for fallback analysis")
	}
}

// TestBuildAnalysisPrompt tests the AI prompt generation
func TestBuildAnalysisPrompt(t *testing.T) {
	tm := &TestMonitor{}
	
	failure := TestFailure{
		TestName:     "TestExample",
		PackageName:  "example",
		SourceFile:   "test.go",
		LineNumber:   15,
		ErrorMessage: "Expected 5, got 3",
		Timestamp:    time.Date(2023, 1, 1, 12, 0, 0, 0, time.UTC),
	}
	
	prompt := tm.buildAnalysisPrompt(failure, "test output", "source code")
	
	if !strings.Contains(prompt, "TestExample") {
		t.Error("Prompt should contain test name")
	}
	
	if !strings.Contains(prompt, "example") {
		t.Error("Prompt should contain package name")
	}
	
	if !strings.Contains(prompt, "test.go") {
		t.Error("Prompt should contain source file")
	}
	
	if !strings.Contains(prompt, "Expected 5, got 3") {
		t.Error("Prompt should contain error message")
	}
	
	if !strings.Contains(prompt, "JSON response") {
		t.Error("Prompt should request JSON response")
	}
	
	if !strings.Contains(prompt, "root_cause") {
		t.Error("Prompt should specify required JSON fields")
	}
}
