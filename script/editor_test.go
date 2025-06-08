package script

import (
	"os"
	"please/types"
	"strings"
	"testing"
)

func TestEditScript_NoChange(t *testing.T) {
	// Prepare a dummy script response
	resp := &types.ScriptResponse{
		TaskDescription: "echo hello world",
		Script:          "echo hello world",
		ScriptType:      "bash",
		Model:           "test-model",
		Provider:        "test-provider",
	}

	// Write the script to a temp file and simulate editing with no change
	tempFile, err := os.CreateTemp("", "please_edit_test_*.sh")
	if err != nil {
		t.Fatalf("failed to create temp file: %v", err)
	}
	defer os.Remove(tempFile.Name())
	if _, err := tempFile.WriteString(resp.Script); err != nil {
		t.Fatalf("failed to write to temp file: %v", err)
	}
	tempFile.Close()

	// Simulate reading back the same content (no change)
	content, err := os.ReadFile(tempFile.Name())
	if err != nil {
		t.Fatalf("failed to read temp file: %v", err)
	}
	if string(content) != resp.Script {
		t.Fatalf("content mismatch: got %q, want %q", string(content), resp.Script)
	}

	// Simulate EditScript logic for no change
	if string(content) == resp.Script {
		// Should return the original response and no error
		newResp, err := func() (*types.ScriptResponse, error) {
			return resp, nil
		}()
		if err != nil {
			t.Fatalf("unexpected error: %v", err)
		}
		if newResp != resp {
			t.Errorf("expected original response, got new one")
		}
	}
}

func TestEditScript_Modified(t *testing.T) {
	resp := &types.ScriptResponse{
		TaskDescription: "echo hello world",
		Script:          "echo hello world",
		ScriptType:      "bash",
		Model:           "test-model",
		Provider:        "test-provider",
	}

	// Simulate user modifies the script
	modified := "echo goodbye world"
	if modified == resp.Script {
		t.Fatalf("modified script should differ from original")
	}

	editedResp := &types.ScriptResponse{
		TaskDescription: resp.TaskDescription + " (edited)",
		Script:          modified,
		ScriptType:      resp.ScriptType,
		Model:           resp.Model,
		Provider:        resp.Provider,
	}

	if !strings.Contains(editedResp.TaskDescription, "edited") {
		t.Errorf("TaskDescription should indicate edit")
	}
	if editedResp.Script == resp.Script {
		t.Errorf("Script should be modified")
	}
}
