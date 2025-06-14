package localization

import (
	"os"
	"path/filepath"
	"testing"

	"please/types"
)

func TestWhenLoadingLanguageFile_ShouldReturnMessages(t *testing.T) {
	dir := t.TempDir()
	file := filepath.Join(dir, "en.json")
	os.WriteFile(file, []byte(`{"language":"en","theme":"default","messages":{"banner":{"title":"Hi"}}}`), 0644)
	mgr, err := NewLocalizationManager(dir)
	if err != nil {
		t.Fatalf("init: %v", err)
	}
	mgr.LoadLanguage("en", file)
	mgr.SetLanguage("en")
	if msg := mgr.GetMessage("banner.title"); msg != "Hi" {
		t.Errorf("expected Hi, got %s", msg)
	}
}

func TestWhenLanguageMissing_ShouldFallbackToDefaults(t *testing.T) {
	mgr, err := NewLocalizationManager("..")
	if err != nil {
		t.Fatalf("init: %v", err)
	}
	if mgr.GetMessage("banner.title") == "" {
		t.Errorf("expected default title")
	}
}

func TestWhenSwitchingTheme_ShouldReturnUpdatedColor(t *testing.T) {
	mgr := &LocalizationManager{config: &types.LocalizationConfig{Themes: types.Theme{Colors: map[string]string{"primary": "#fff"}}}, themes: make(map[string]types.Theme)}
	mgr.LoadTheme("dark", types.Theme{Colors: map[string]string{"primary": "#000"}})
	if mgr.GetThemeColor("primary") != "#fff" {
		t.Errorf("expected default color")
	}
	mgr.SetTheme("dark")
	if mgr.GetThemeColor("primary") != "#000" {
		t.Errorf("expected dark color")
	}
}
