package script

import (
	"bytes"
	"encoding/json"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"time"

	"please/providers"
	"please/types"
)

// TestFailure represents a captured test failure with context
type TestFailure struct {
	TestName     string    `json:"test_name"`
	PackageName  string    `json:"package_name"`
	FailureType  string    `json:"failure_type"`
	ErrorMessage string    `json:"error_message"`
	StackTrace   string    `json:"stack_trace"`
	SourceFile   string    `json:"source_file"`
	LineNumber   int       `json:"line_number"`
	Timestamp    time.Time `json:"timestamp"`
	Command      string    `json:"command"`
	FullOutput   string    `json:"full_output"`
}

// TestAnalysis represents AI analysis of test failures
type TestAnalysis struct {
	Summary          string              `json:"summary"`
	RootCause        string              `json:"root_cause"`
	Suggestions      []string            `json:"suggestions"`
	CodeFix          string              `json:"code_fix,omitempty"`
	RequiresManual   bool                `json:"requires_manual"`
	RelatedFiles     []string            `json:"related_files"`
	TestStrategy     string              `json:"test_strategy"`
	FailureCategory  string              `json:"failure_category"`
	RecommendedSteps []RecommendedAction `json:"recommended_steps"`
}

// RecommendedAction represents a specific action to fix the test
type RecommendedAction struct {
	Action      string `json:"action"`
	Description string `json:"description"`
	Command     string `json:"command,omitempty"`
	FilePath    string `json:"file_path,omitempty"`
	CodeChange  string `json:"code_change,omitempty"`
	Priority    string `json:"priority"` // high, medium, low
}

// TestMonitor handles automatic test execution monitoring and AI analysis
type TestMonitor struct {
	Provider     providers.Provider
	Config       *types.Config
	WorkingDir   string
	VerboseMode  bool
	AutoAnalyze  bool
	SaveFailures bool
}

// NewTestMonitor creates a new test monitoring instance
func NewTestMonitor(provider providers.Provider, config *types.Config) *TestMonitor {
	workingDir, _ := os.Getwd()
	return &TestMonitor{
		Provider:     provider,
		Config:       config,
		WorkingDir:   workingDir,
		VerboseMode:  true,
		AutoAnalyze:  true,
		SaveFailures: true,
	}
}

// RunTestsWithMonitoring executes tests and automatically analyzes failures
func (tm *TestMonitor) RunTestsWithMonitoring(testPattern string) error {
	fmt.Println("🧪 Starting test execution with AI monitoring...")
	
	// Execute tests and capture output
	testOutput, failures, err := tm.executeTests(testPattern)
	
	if err != nil {
		fmt.Printf("❌ Test execution failed: %v\n", err)
	}
	
	if len(failures) == 0 {
		fmt.Println("✅ All tests passed! No failures to analyze.")
		return nil
	}
	
	fmt.Printf("🔍 Detected %d test failure(s). Starting AI analysis...\n", len(failures))
	
	// Analyze each failure with AI
	for i, failure := range failures {
		fmt.Printf("\n🤖 Analyzing failure %d/%d: %s\n", i+1, len(failures), failure.TestName)
		
		analysis, err := tm.analyzeFailureWithAI(failure, testOutput)
		if err != nil {
			fmt.Printf("⚠️  AI analysis failed: %v\n", err)
			continue
		}
		
		tm.displayAnalysis(failure, analysis)
		
		if tm.SaveFailures {
			tm.saveFailureReport(failure, analysis)
		}
	}
	
	return err
}

// executeTests runs Go tests and captures failures
func (tm *TestMonitor) executeTests(pattern string) (string, []TestFailure, error) {
	var cmd *exec.Cmd
	if pattern == "" {
		cmd = exec.Command("go", "test", "-v", "./...")
	} else {
		cmd = exec.Command("go", "test", "-v", "-run", pattern, "./...")
	}
	
	var stdout, stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr
	cmd.Dir = tm.WorkingDir
	
	// Execute command
	err := cmd.Run()
	
	fullOutput := stdout.String() + stderr.String()
	
	// Parse failures from output
	failures := tm.parseTestFailures(fullOutput, fmt.Sprintf("go test -v %s", pattern))
	
	return fullOutput, failures, err
}

// parseTestFailures extracts test failures from Go test output
func (tm *TestMonitor) parseTestFailures(output, command string) []TestFailure {
	var failures []TestFailure
	
	// Regex patterns to match Go test failure output
	failPattern := regexp.MustCompile(`--- FAIL: (\S+) \([\d.]+s\)`)
	errorPattern := regexp.MustCompile(`^\s+(.+\.go:\d+):\s+(.+)$`)
	
	lines := strings.Split(output, "\n")
	var currentTestName string
	var errorLines []string
	var currentSourceFile string
	var currentLineNumber int
	
	for i, line := range lines {
		// Collect error lines between test start and test fail
		if matches := errorPattern.FindStringSubmatch(line); len(matches) > 2 {
			// This is an error line - collect it and set file/line info
			// Use first occurrence for source file and line number
			if currentSourceFile == "" {
				currentSourceFile = tm.extractFilePath(matches[1])
				currentLineNumber = tm.extractLineNumber(matches[1])
			}
			errorLines = append(errorLines, matches[2])
		} else if strings.HasPrefix(line, "=== RUN   ") {
			// New test starting - reset error collection
			currentTestName = strings.TrimSpace(strings.TrimPrefix(line, "=== RUN   "))
			errorLines = []string{}
			currentSourceFile = ""
			currentLineNumber = 0
		} else if matches := failPattern.FindStringSubmatch(line); len(matches) > 1 {
			// Test failed - create failure record
			testName := matches[1]
			
			failure := TestFailure{
				TestName:     testName,
				PackageName:  tm.extractPackageName(output, i),
				Timestamp:    time.Now(),
				Command:      command,
				FullOutput:   output,
				SourceFile:   currentSourceFile,
				LineNumber:   currentLineNumber,
				ErrorMessage: strings.Join(errorLines, "\n"),
			}
			
			failures = append(failures, failure)
			
			// Reset for next test
			errorLines = []string{}
			currentSourceFile = ""
			currentLineNumber = 0
		} else if strings.TrimSpace(line) != "" && 
				  !strings.HasPrefix(line, "=== ") && 
				  !strings.HasPrefix(line, "--- ") &&
				  !strings.HasPrefix(line, "FAIL") &&
				  !strings.HasPrefix(line, "ok") &&
				  !strings.HasPrefix(line, "?") &&
				  currentTestName != "" {
			// Additional error line for current test
			errorLines = append(errorLines, strings.TrimSpace(line))
		}
	}
	
	return failures
}

// analyzeFailureWithAI sends test failure to AI for analysis
func (tm *TestMonitor) analyzeFailureWithAI(failure TestFailure, fullOutput string) (*TestAnalysis, error) {
	// Read source file for context if available
	sourceContext := ""
	if failure.SourceFile != "" {
		if content, err := os.ReadFile(failure.SourceFile); err == nil {
			sourceContext = string(content)
		}
	}
	
	// Build AI prompt for test failure analysis
	prompt := tm.buildAnalysisPrompt(failure, fullOutput, sourceContext)
	
	// Create AI request
	request := &types.ScriptRequest{
		TaskDescription: prompt,
		ScriptType:      "analysis",
		Provider:        tm.Config.Provider,
		Model:           tm.Config.PreferredModel,
	}
	
	// Get AI response
	response, err := tm.Provider.GenerateScript(request)
	if err != nil {
		return nil, fmt.Errorf("AI analysis request failed: %v", err)
	}
	
	// Parse structured analysis from AI response
	analysis := tm.parseAIAnalysis(response.Script)
	
	return analysis, nil
}

// buildAnalysisPrompt creates a detailed prompt for AI analysis
func (tm *TestMonitor) buildAnalysisPrompt(failure TestFailure, fullOutput, sourceContext string) string {
	return fmt.Sprintf(`🧪 TEST FAILURE ANALYSIS REQUEST

You are an expert Go developer and testing specialist. Please analyze this test failure and provide structured recommendations.

## Test Failure Details
- Test Name: %s
- Package: %s
- Source File: %s (line %d)
- Failure Time: %s

## Error Message
%s

## Full Test Output
%s

## Source Code Context
%s

## Analysis Request
Please provide a JSON response with the following structure:
{
  "summary": "Brief description of what went wrong",
  "root_cause": "Detailed explanation of the underlying cause",
  "suggestions": ["List of specific suggestions to fix the issue"],
  "code_fix": "Suggested code changes (if applicable)",
  "requires_manual": false,
  "related_files": ["List of files that might need changes"],
  "test_strategy": "Testing approach recommendations",
  "failure_category": "Category: logic_error|assertion_failure|setup_issue|dependency_issue|race_condition|environment_issue",
  "recommended_steps": [
    {
      "action": "Action type",
      "description": "What to do",
      "command": "Command to run (if applicable)",
      "file_path": "File to modify (if applicable)",
      "code_change": "Specific code change (if applicable)",
      "priority": "high|medium|low"
    }
  ]
}

Focus on:
1. Understanding why the test failed
2. Identifying the root cause
3. Providing actionable steps to fix it
4. Suggesting improvements to prevent similar failures
5. Following Go testing best practices and TDD principles

Please provide only the JSON response.`, 
		failure.TestName, 
		failure.PackageName, 
		failure.SourceFile, 
		failure.LineNumber,
		failure.Timestamp.Format("2006-01-02 15:04:05"),
		failure.ErrorMessage,
		tm.truncateOutput(fullOutput, 2000),
		tm.truncateOutput(sourceContext, 1000))
}

// parseAIAnalysis extracts structured analysis from AI response
func (tm *TestMonitor) parseAIAnalysis(response string) *TestAnalysis {
	// Try to extract JSON from response
	jsonStart := strings.Index(response, "{")
	jsonEnd := strings.LastIndex(response, "}")
	
	if jsonStart == -1 || jsonEnd == -1 {
		// Fallback to basic analysis if JSON parsing fails
		return &TestAnalysis{
			Summary:         "AI analysis parsing failed",
			RootCause:       "Could not parse structured response",
			Suggestions:     []string{"Review the test manually", "Check test logic and assertions"},
			RequiresManual:  true,
			FailureCategory: "analysis_error",
		}
	}
	
	jsonStr := response[jsonStart : jsonEnd+1]
	
	var analysis TestAnalysis
	if err := json.Unmarshal([]byte(jsonStr), &analysis); err != nil {
		// Fallback analysis
		return &TestAnalysis{
			Summary:         "JSON parsing failed",
			RootCause:       fmt.Sprintf("Could not parse AI response: %v", err),
			Suggestions:     []string{"Review the raw AI response", "Check test manually"},
			RequiresManual:  true,
			FailureCategory: "analysis_error",
		}
	}
	
	return &analysis
}

// displayAnalysis shows the AI analysis in a formatted way
func (tm *TestMonitor) displayAnalysis(failure TestFailure, analysis *TestAnalysis) {
	fmt.Printf("\n╔══════════════════════════════════════════════════════════════════════════════╗\n")
	fmt.Printf("║                        🤖 AI TEST FAILURE ANALYSIS                          ║\n")
	fmt.Printf("╚══════════════════════════════════════════════════════════════════════════════╝\n")
	
	fmt.Printf("\n🔍 Test: %s\n", failure.TestName)
	fmt.Printf("📁 Package: %s\n", failure.PackageName)
	fmt.Printf("📄 File: %s:%d\n", failure.SourceFile, failure.LineNumber)
	fmt.Printf("🏷️  Category: %s\n", analysis.FailureCategory)
	
	fmt.Printf("\n📝 Summary:\n%s\n", analysis.Summary)
	
	fmt.Printf("\n🎯 Root Cause:\n%s\n", analysis.RootCause)
	
	if len(analysis.Suggestions) > 0 {
		fmt.Printf("\n💡 Suggestions:\n")
		for i, suggestion := range analysis.Suggestions {
			fmt.Printf("  %d. %s\n", i+1, suggestion)
		}
	}
	
	if analysis.CodeFix != "" {
		fmt.Printf("\n🔧 Suggested Code Fix:\n")
		fmt.Printf("```go\n%s\n```\n", analysis.CodeFix)
	}
	
	if len(analysis.RecommendedSteps) > 0 {
		fmt.Printf("\n📋 Recommended Actions:\n")
		for i, step := range analysis.RecommendedSteps {
			fmt.Printf("  %d. [%s] %s: %s\n", i+1, strings.ToUpper(step.Priority), step.Action, step.Description)
			if step.Command != "" {
				fmt.Printf("     Command: %s\n", step.Command)
			}
			if step.FilePath != "" {
				fmt.Printf("     File: %s\n", step.FilePath)
			}
		}
	}
	
	if analysis.TestStrategy != "" {
		fmt.Printf("\n🧪 Testing Strategy:\n%s\n", analysis.TestStrategy)
	}
	
	if analysis.RequiresManual {
		fmt.Printf("\n⚠️  Manual Review Required\n")
	}
	
	fmt.Printf("\n%s\n", strings.Repeat("═", 80))
}

// saveFailureReport saves the failure and analysis to a file
func (tm *TestMonitor) saveFailureReport(failure TestFailure, analysis *TestAnalysis) {
	reportDir := filepath.Join(tm.WorkingDir, ".please", "test-reports")
	if err := os.MkdirAll(reportDir, 0755); err != nil {
		fmt.Printf("⚠️  Could not create reports directory: %v\n", err)
		return
	}
	
	timestamp := failure.Timestamp.Format("20060102-150405")
	filename := fmt.Sprintf("test-failure-%s-%s.json", timestamp, failure.TestName)
	filepath := filepath.Join(reportDir, filename)
	
	report := map[string]interface{}{
		"failure":  failure,
		"analysis": analysis,
		"metadata": map[string]interface{}{
			"generated_at": time.Now(),
			"version":      "1.0",
			"working_dir":  tm.WorkingDir,
		},
	}
	
	data, err := json.MarshalIndent(report, "", "  ")
	if err != nil {
		fmt.Printf("⚠️  Could not marshal report: %v\n", err)
		return
	}
	
	if err := os.WriteFile(filepath, data, 0644); err != nil {
		fmt.Printf("⚠️  Could not save report: %v\n", err)
		return
	}
	
	fmt.Printf("💾 Failure report saved: %s\n", filepath)
}

// Helper functions

func (tm *TestMonitor) extractPackageName(output string, lineIndex int) string {
	lines := strings.Split(output, "\n")
	for i := lineIndex; i >= 0; i-- {
		if strings.Contains(lines[i], "=== RUN") {
			continue
		}
		if strings.HasPrefix(lines[i], "?") || strings.HasPrefix(lines[i], "ok") || strings.HasPrefix(lines[i], "FAIL") {
			parts := strings.Fields(lines[i])
			if len(parts) > 1 {
				return parts[1]
			}
		}
	}
	return "unknown"
}

func (tm *TestMonitor) extractFilePath(location string) string {
	parts := strings.Split(location, ":")
	if len(parts) > 0 {
		return parts[0]
	}
	return ""
}

func (tm *TestMonitor) extractLineNumber(location string) int {
	parts := strings.Split(location, ":")
	if len(parts) > 1 {
		if num := strings.TrimSpace(parts[1]); num != "" {
			if lineNumber, err := strconv.Atoi(num); err == nil {
				return lineNumber
			}
		}
	}
	return 0
}

func (tm *TestMonitor) truncateOutput(text string, maxLength int) string {
	if len(text) <= maxLength {
		return text
	}
	return text[:maxLength] + "... (truncated)"
}

// RunMonitoredTests is the main entry point for AI-monitored testing
func RunMonitoredTests(provider providers.Provider, config *types.Config, pattern string) error {
	monitor := NewTestMonitor(provider, config)
	return monitor.RunTestsWithMonitoring(pattern)
}
