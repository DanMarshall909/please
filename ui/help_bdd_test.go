package ui

import (
	"os"
	"strings"
	"testing"

	"please/localization"
)

func captureHelpOutput(fn func()) string {
	return captureStdoutForHelp(fn)
}

func TestWhenShowingHelpWithLocalization_ShouldUseLocalizedBannerTexts(t *testing.T) {
	dir := t.TempDir()
	langPath := dir + "/lang.json"
	os.WriteFile(langPath, []byte(`{"language":"x","theme":"default","messages":{"banner":{"title":"Hola","subtitle":"Ayuda"}}}`), 0644)
	mgr, err := localization.NewLocalizationManager(dir)
	if err != nil {
		t.Fatalf("failed to create manager: %v", err)
	}
	mgr.LoadLanguage("x", langPath)
	mgr.SetLanguage("x")
	SetLocalizationManagerForHelp(mgr)
	out := captureHelpOutput(ShowHelp)
	if !strings.Contains(out, "Hola") || !strings.Contains(out, "Ayuda") {
		t.Errorf("expected localized banner text: %s", out)
	}
}

func TestWhenShowHelpWithInjectedBanner_ShouldInvokeBanner(t *testing.T) {
	called := false
	out := captureHelpOutput(func() { showHelpWithBanner(func() { called = true }) })
	if !called {
		t.Error("expected injected banner to be called")
	}
	if out == "" {
		t.Error("expected some output from help")
	}
}
