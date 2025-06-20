package ui

import (
	"os"
	"strings"
	"testing"
	"time"

	"please/types"
)

// TestProgressIndicator_ShowsStatusMessages tests that progress indicators display status
func Test_when_progress_indicator_then_shows_status_messages(t *testing.T) {
	// Create a progress indicator
	progress := NewProgressIndicator("Testing operation")

	if progress == nil {
		t.Fatal("NewProgressIndicator should not return nil")
	}

	// Test that it has the correct initial message
	if progress.message != "Testing operation" {
		t.Errorf("Expected message 'Testing operation', got '%s'", progress.message)
	}

	// Test that it's not started initially
	if progress.isRunning {
		t.Error("Progress indicator should not be running initially")
	}
}

// TestProgressIndicator_UpdateStatus tests status updates
func Test_when_progress_indicator_then_update_status(t *testing.T) {
	progress := NewProgressIndicator("Initial message")

	// Update the status
	progress.UpdateStatus("Updated message")

	if progress.message != "Updated message" {
		t.Errorf("Expected updated message 'Updated message', got '%s'", progress.message)
	}
}

// TestProgressIndicator_StartStop tests start and stop functionality
func Test_when_progress_indicator_then_start_stop(t *testing.T) {
	progress := NewProgressIndicator("Test message")

	// Start the progress indicator
	progress.Start()

	if !progress.isRunning {
		t.Error("Progress indicator should be running after Start()")
	}

	// Give it a moment to display
	time.Sleep(100 * time.Millisecond)

	// Stop the progress indicator
	progress.Stop()

	if progress.isRunning {
		t.Error("Progress indicator should not be running after Stop()")
	}
}

// TestProviderStatusMessages tests provider-specific status messages
func TestProviderStatusMessages(t *testing.T) {
	tests := []struct {
		provider string
		expected string
	}{
		{"ollama", "🤖 Connecting to Ollama (this may take a moment to start up)..."},
		{"openai", "🤖 Connecting to OpenAI..."},
		{"anthropic", "🤖 Connecting to Anthropic..."},
		{"unknown", "🤖 Connecting to AI provider..."},
	}

	for _, test := range tests {
		message := GetProviderStatusMessage(test.provider)
		if !strings.Contains(message, test.expected) {
			t.Errorf("For provider '%s', expected message containing '%s', got '%s'",
				test.provider, test.expected, message)
		}
	}
}

// TestScriptGenerationProgress tests script generation progress messages
func TestScriptGenerationProgress(t *testing.T) {
	config := &types.Config{
		Provider:       "ollama",
		PreferredModel: "deepseek-coder:6.7b",
	}

	// Test that we get appropriate progress messages for script generation
	messages := GetScriptGenerationProgressMessages(config)

	if len(messages) == 0 {
		t.Error("Should return progress messages for script generation")
	}

	// Check that it includes provider-specific messaging
	foundProviderMessage := false
	for _, msg := range messages {
		if strings.Contains(msg, "ollama") || strings.Contains(msg, "Ollama") {
			foundProviderMessage = true
			break
		}
	}

	if !foundProviderMessage {
		t.Error("Progress messages should include provider-specific information")
	}
}

// TestAutoFixProgress tests auto-fix operation progress messages
func TestAutoFixProgress(t *testing.T) {
	originalScript := "echo 'broken script'"
	errorMessage := "syntax error"
	provider := "ollama"

	messages := GetAutoFixProgressMessages(originalScript, errorMessage, provider)

	if len(messages) == 0 {
		t.Error("Should return progress messages for auto-fix")
	}

	// Check that messages include relevant context
	foundScriptInfo := false
	foundErrorInfo := false

	for _, msg := range messages {
		if strings.Contains(msg, "fix") || strings.Contains(msg, "repair") {
			foundScriptInfo = true
		}
		if strings.Contains(msg, "error") || strings.Contains(msg, "issue") {
			foundErrorInfo = true
		}
	}

	if !foundScriptInfo {
		t.Error("Progress messages should mention fixing/repairing")
	}

	if !foundErrorInfo {
		t.Error("Progress messages should mention error/issue context")
	}
}

func Test_when_show_simple_progress_then_stop_cleanly(t *testing.T) {
	os.Setenv("PROGRESS_TEST_MODE", "1")
	stop := ShowSimpleProgress("doing work")
	time.Sleep(100 * time.Millisecond)
	stop()
}

func Test_when_show_provider_progress_then_stop_cleanly(t *testing.T) {
	os.Setenv("PROGRESS_TEST_MODE", "1")
	stop := ShowProviderProgress("openai", "testing")
	time.Sleep(100 * time.Millisecond)
	stop()
}

func Test_when_show_progress_with_steps_then_run_steps(t *testing.T) {
	os.Setenv("PROGRESS_TEST_MODE", "1")
	start := time.Now()
	ShowProgressWithSteps([]string{"one"})
	if time.Since(start) > 3*time.Second {
		t.Errorf("progress took too long")
	}
}

func Test_when_auto_fix_error_contains_permission_then_add_permission_message(t *testing.T) {
	messages := GetAutoFixProgressMessages(strings.Repeat("a", 100), "permission denied", "openai")
	found := false
	for _, m := range messages {
		if strings.Contains(m, "permission") {
			found = true
			break
		}
	}
	if !found {
		t.Error("expected permission message when error contains permission")
	}
}

func Test_when_auto_fix_script_very_large_then_add_large_script_message(t *testing.T) {
	script := strings.Repeat("x", 600)
	messages := GetAutoFixProgressMessages(script, "other", "anthropic")
	last := messages[len(messages)-1]
	if !strings.Contains(last, "large script") {
		t.Errorf("expected large script message, got %s", last)
	}
}
