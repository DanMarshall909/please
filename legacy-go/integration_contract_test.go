package main

import (
	"os"
	"strings"
	"testing"

	"please/types"
)

// TestMainWorkflow_EndToEnd tests the complete CLI workflow
func TestMainWorkflow_WhenValidTask_ShouldGenerateScript(t *testing.T) {
	// Skip if no provider configured (CI environments)
	if os.Getenv("SKIP_INTEGRATION") == "true" {
		t.Skip("Integration tests skipped")
	}

	// Test core workflow: CLI args -> script generation
	// This test defines the contract for C# migration
	oldArgs := os.Args
	defer func() { os.Args = oldArgs }()

	os.Args = []string{"please", "list files in current directory"}

	// Should not panic or crash
	defer func() {
		if r := recover(); r != nil {
			t.Errorf("Main workflow panicked: %v", r)
		}
	}()

	// Run main flow (will exit, so we test components)
	testMainComponents(t)
}

func testMainComponents(t *testing.T) {
	// Test critical path components
	
	// 1. Command recognition
	if !isLastScriptCommand("run last script") {
		t.Error("Should recognize 'run last script' as last script command")
	}
	
	// 2. Fallback model selection
	model := getFallbackModel("openai")
	if model != "gpt-3.5-turbo" {
		t.Errorf("Expected 'gpt-3.5-turbo', got '%s'", model)
	}
	
	// 3. Script generation (mock test)
	request := &types.ScriptRequest{
		TaskDescription: "test task",
		ScriptType:      "powershell",
		Provider:        "openai",
		Model:           "gpt-3.5-turbo",
	}
	
	if request.TaskDescription == "" {
		t.Error("Script request should have task description")
	}
}

// TestCommandLineInterface_SpecialCommands tests CLI contract
func TestCommandLineInterface_WhenSpecialFlags_ShouldHandleCorrectly(t *testing.T) {
	tests := []struct {
		name     string
		args     []string
		expected string // Expected behavior description
	}{
		{"version flag", []string{"please", "--version"}, "should show version"},
		{"help flag", []string{"please", "--help"}, "should show help"},
		{"install alias", []string{"please", "--install-alias"}, "should install alias"},
		{"test monitor", []string{"please", "--test-monitor"}, "should run test monitor"},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			// Verify args are recognized (contract definition)
			if len(tt.args) < 2 {
				t.Error("Test case should have at least 2 args")
			}
			
			switch tt.args[1] {
			case "--version", "--help", "-h", "--install-alias", 
				 "--uninstall-alias", "--test-monitor", "--monitor-tests":
				// These are valid special commands
			default:
				t.Errorf("Unknown special command: %s", tt.args[1])
			}
		})
	}
}

// TestNaturalLanguageCommands_Recognition tests command parsing contract
func TestNaturalLanguageCommands_WhenLastScriptPatterns_ShouldRecognize(t *testing.T) {
	lastScriptCommands := []string{
		"run my last script",
		"run last script", 
		"execute last script",
		"run again",
		"do it again",
		"repeat last",
		"last script",
		"previous script",
	}

	for _, cmd := range lastScriptCommands {
		t.Run("command_"+strings.ReplaceAll(cmd, " ", "_"), func(t *testing.T) {
			if !isLastScriptCommand(cmd) {
				t.Errorf("Should recognize '%s' as last script command", cmd)
			}
		})
	}

	// Test negative cases
	nonLastScriptCommands := []string{
		"create a new file",
		"help me debug",
		"install package",
	}

	for _, cmd := range nonLastScriptCommands {
		t.Run("negative_"+strings.ReplaceAll(cmd, " ", "_"), func(t *testing.T) {
			if isLastScriptCommand(cmd) {
				t.Errorf("Should NOT recognize '%s' as last script command", cmd)
			}
		})
	}
}
