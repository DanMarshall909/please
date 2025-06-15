package ui

import (
	"strings"
	"testing"

	"please/types"
)

func TestWhenCreatingProgressIndicator_ShouldInitializeFields(t *testing.T) {
	p := NewProgressIndicator("test")
	if p.message != "test" || p.isRunning {
		t.Errorf("progress indicator not initialized correctly: %+v", p)
	}
}

func TestWhenUpdatingProgressMessage_ShouldChangeInternalMessage(t *testing.T) {
	p := NewProgressIndicator("one")
	p.UpdateStatus("two")
	if p.message != "two" {
		t.Errorf("expected message 'two', got %s", p.message)
	}
}

func TestWhenStartingAndStoppingProgress_ShouldToggleRunningState(t *testing.T) {
	p := NewProgressIndicator("msg")
	p.Start()
	if !p.isRunning {
		t.Error("expected indicator running")
	}
	p.Stop()
	if p.isRunning {
		t.Error("expected indicator stopped")
	}
}
func TestWhenGettingScriptGenerationMessagesForOllama_ShouldIncludeStartupSteps(t *testing.T) {
	cfg := &types.Config{Provider: "ollama", PreferredModel: "test"}
	msgs := GetScriptGenerationProgressMessages(cfg)
	if len(msgs) < 7 {
		t.Errorf("expected extra startup messages for ollama, got %d", len(msgs))
	}
	if !strings.Contains(msgs[0], "Ollama") {
		t.Errorf("expected first message to mention Ollama startup")
	}
}
