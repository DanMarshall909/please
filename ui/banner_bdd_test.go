package ui

import (
	"os"
	"strings"
	"testing"
	"time"

	"please/localization"
)

// helper to capture stdout
func captureBannerOutput(fn func()) string {
	return captureStdout(fn)
}

func TestWhenPrintingRainbowBannerWithDelay_ShouldRespectProvidedDelay(t *testing.T) {
	start := time.Now()
	PrintRainbowBannerWithDelay(2 * time.Millisecond)
	if time.Since(start) < 10*time.Millisecond { // 6 lines * 2ms plus overhead
		t.Errorf("expected banner to take at least 10ms")
	}
}

func TestWhenPrintingInstallationSuccess_ShouldOutputInstallationCompleteMessage(t *testing.T) {
	out := captureBannerOutput(PrintInstallationSuccess)
	if !strings.Contains(out, "Installation complete") {
		t.Errorf("expected installation message in output")
	}
}

func TestWhenPrintingFooter_ShouldOutputHappyScripting(t *testing.T) {
	out := captureBannerOutput(PrintFooter)
	if !strings.Contains(out, "Happy scripting") {
		t.Errorf("expected footer to mention happy scripting")
	}
}

func TestWhenPrintingBannerWithLocalization_ShouldUseProvidedMessages(t *testing.T) {
	dir := t.TempDir()
	langPath := dir + "/test.json"
	os.WriteFile(langPath, []byte(`{"language":"test","theme":"default","messages":{"banner":{"title":"Hola","subtitle":"Mundo"}}}`), 0644)
	mgr, err := localization.NewLocalizationManager(dir)
	if err != nil {
		t.Fatalf("failed to create localization manager: %v", err)
	}
	mgr.LoadLanguage("test", langPath)
	mgr.SetLanguage("test")
	SetGlobalLocalizationManager(mgr)
	defer SetGlobalLocalizationManager(nil)
	out := captureBannerOutput(func() { PrintRainbowBannerWithDelay(0) })
	if !strings.Contains(out, "Hola") || !strings.Contains(out, "Mundo") {
		t.Errorf("expected localized messages in banner output: %s", out)
	}
}
