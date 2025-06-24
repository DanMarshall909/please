package ui

import (
	"os"
	"strings"
	"testing"

	"please/localization"
)

func TestWhenPrintingInstallationSuccessWithLocalization_ShouldUseLocalizedMessages(t *testing.T) {
	dir := t.TempDir()
	langPath := dir + "/lang.json"
	os.WriteFile(langPath, []byte(`{"language":"x","theme":"default","messages":{"installation":{"success":"OK","try_it":"TRY","magic":"MAGIC"}}}`), 0644)
	mgr, err := localization.NewLocalizationManager(dir)
	if err != nil {
		t.Fatalf("manager error: %v", err)
	}
	mgr.LoadLanguage("x", langPath)
	mgr.SetLanguage("x")
	SetGlobalLocalizationManager(mgr)
	defer SetGlobalLocalizationManager(nil)

	output := captureStdout(func() { PrintInstallationSuccess() })
	if !strings.Contains(output, "OK") || !strings.Contains(output, "TRY") || !strings.Contains(output, "MAGIC") {
		t.Errorf("expected localized messages, got: %s", output)
	}
}

func TestWhenPrintingFooterWithLocalization_ShouldUseLocalizedMessages(t *testing.T) {
	dir := t.TempDir()
	langPath := dir + "/lang.json"
	os.WriteFile(langPath, []byte(`{"language":"x","theme":"default","messages":{"footer":{"tips":"TIPS","happy":"HAPPY"}}}`), 0644)
	mgr, err := localization.NewLocalizationManager(dir)
	if err != nil {
		t.Fatalf("manager error: %v", err)
	}
	mgr.LoadLanguage("x", langPath)
	mgr.SetLanguage("x")
	SetGlobalLocalizationManager(mgr)

	defer SetGlobalLocalizationManager(nil)
	output := captureStdout(func() { PrintFooter() })
	if !strings.Contains(output, "TIPS") || !strings.Contains(output, "HAPPY") {
		t.Errorf("expected localized footer messages, got: %s", output)
	}
}

func TestWhenPrintingRainbowBanner_ShouldIncludeAsciiArtLines(t *testing.T) {
	output := captureStdout(func() { PrintRainbowBannerWithDelay(0) })
	if strings.Count(output, "\n") < 6 {
		t.Errorf("expected at least 6 lines in banner, got %d", strings.Count(output, "\n"))
	}
}
