package ui

import (
	"os"
	"strings"
	"testing"
	"time"

	"please/types"
)

func TestWhenGettingProviderStatusMessageUnknown_ShouldUseDefaultMessage(t *testing.T) {
	SetLocalizationManager(nil)
	msg := GetProviderStatusMessage("unknown")
	if !strings.Contains(msg, "AI provider") {
		t.Errorf("expected default provider message, got %s", msg)
	}
}

func TestWhenShowProviderProgress_ShouldReturnStopFunction(t *testing.T) {
	os.Setenv("PROGRESS_TEST_MODE", "1")
	stop := ShowProviderProgress("openai", "testing")
	time.Sleep(10 * time.Millisecond)
	if stop == nil {
		t.Fatal("expected stop func")
	}
	stop()
}

func TestWhenShowProgressWithSteps_ShouldIterateAllMessages(t *testing.T) {
	os.Setenv("PROGRESS_TEST_MODE", "1")
	start := time.Now()
	ShowProgressWithSteps([]string{"one", "two"})
	if time.Since(start) > 5*time.Second {
		t.Errorf("progress took too long")
	}
}

func TestWhenGettingScriptGenerationProgressMessages_ShouldIncludeProviderInit(t *testing.T) {
	cfg := &types.Config{Provider: "openai", PreferredModel: "gpt"}
	msgs := GetScriptGenerationProgressMessages(cfg)
	found := false
	for _, m := range msgs {
		if strings.Contains(m, "openai") {
			found = true
			break
		}
	}
	if !found {
		t.Errorf("expected provider name in messages")
	}
}
