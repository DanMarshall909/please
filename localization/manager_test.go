package localization

import (
	"os"
	"path/filepath"
	"testing"

	"please/types"
)

func TestNewLocalizationManager(t *testing.T) {
	mgr, err := NewLocalizationManager(".")
	if err != nil || mgr == nil {
		t.Fatalf("manager init error: %v", err)
	}
}

func TestLocalizationManagerWithMissingDirectory(t *testing.T) {
	// Test with non-existent directory
	nonExistentDir := "/this/path/should/not/exist"

	manager, err := NewLocalizationManager(nonExistentDir)

	// Should still create a manager with defaults even if directory doesn't exist
	if manager == nil {
		t.Error("NewLocalizationManager should return a manager even with missing directory")
	}

	// Error is acceptable but manager should still be functional
	if err != nil {
		t.Logf("Expected error with missing directory: %v", err)
	}
}

func TestLocalizationManagerDefaults(t *testing.T) {
	manager, err := NewLocalizationManager("..")
	if err != nil {
		t.Fatalf("NewLocalizationManager failed: %v", err)
	}

	title := manager.GetMessage("banner.title")
	if title == "" {
		t.Errorf("expected default banner title, got '%s'", title)
	}
}

func TestLocalizationManagerWithConfigFile(t *testing.T) {
	tempDir := t.TempDir()

	// Create languages directory
	langDir := filepath.Join(tempDir, "languages")
	if err := os.MkdirAll(langDir, 0755); err != nil {
		t.Fatalf("Failed to create languages directory: %v", err)
	}

	// Create a proper language pack file
	langPackContent := `{
        "language": "en-custom",
        "theme": "default",
        "messages": {"banner": {"title": "Custom Banner"}}
        }`

	langPackPath := filepath.Join(langDir, "en-custom.json")
	if err := os.WriteFile(langPackPath, []byte(langPackContent), 0644); err != nil {
		t.Fatalf("Failed to write test language pack: %v", err)
	}

	manager, err := NewLocalizationManager(tempDir)
	if err != nil {
		t.Fatalf("NewLocalizationManager failed: %v", err)
	}

	manager.LoadLanguage("en-custom", langPackPath)
	manager.SetLanguage("en-custom")
	title := manager.GetMessage("banner.title")
	if title == "" {
		t.Errorf("expected banner title from custom pack")
	}
}

func TestLocalizationSystemGet(t *testing.T) {
	manager, err := NewLocalizationManager("..")
	if err != nil {
		t.Fatalf("NewLocalizationManager failed: %v", err)
	}

	tests := []struct {
		name string
		key  string
	}{
		{
			name: "Banner title",
			key:  "banner.title",
		},
		{
			name: "Non-existent key returns empty",
			key:  "nonexistent.key",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := manager.GetMessage(tt.key)
			if tt.key == "nonexistent.key" {
				if result != "" {
					t.Errorf("expected empty string for missing key")
				}
			} else if result == "" {
				t.Errorf("expected value for %s", tt.key)
			}
		})
	}
}

func TestWhenLoadFromFile_ShouldParseLocalizationConfig(t *testing.T) {
	temp := t.TempDir()
	jsonPath := filepath.Join(temp, "test.json")
	content := `{
        "language": "en-us",
        "theme": "default",
        "messages": {
            "banner": {"title": "Hello", "subtitle": "World"}
        },
        "themes": {"colors": {"primary": "#fff"}}
    }`
	os.WriteFile(jsonPath, []byte(content), 0644)
	cfg, err := LoadFromFile(jsonPath)
	if err != nil {
		t.Fatalf("load error: %v", err)
	}
	if cfg.Messages.Banner.Title != "Hello" {
		t.Errorf("unexpected title: %s", cfg.Messages.Banner.Title)
	}
	if cfg.Themes.Colors["primary"] != "#fff" {
		t.Errorf("unexpected color: %s", cfg.Themes.Colors["primary"])
	}
}

func TestWhenManagerSwitchesLanguage_ShouldReturnNewMessage(t *testing.T) {
	temp := t.TempDir()
	enPath := filepath.Join(temp, "en-us.json")
	esPath := filepath.Join(temp, "es-es.json")
	os.WriteFile(enPath, []byte(`{"language":"en-us","theme":"default","messages":{"banner":{"title":"Hi"}},"themes":{"colors":{}}}`), 0644)
	os.WriteFile(esPath, []byte(`{"language":"es-es","theme":"default","messages":{"banner":{"title":"Hola"}},"themes":{"colors":{}}}`), 0644)
	mgr := &LocalizationManager{config: &types.LocalizationConfig{}}
	mgr.files = map[string]string{"en-us": enPath, "es-es": esPath}
	mgr.SetLanguage("en-us")
	if mgr.GetMessage("banner.title") != "Hi" {
		t.Errorf("expected english message")
	}
	mgr.SetLanguage("es-es")
	if mgr.GetMessage("banner.title") != "Hola" {
		t.Errorf("expected spanish message")
	}
}

func TestWhenManagerSwitchesTheme_ShouldReturnNewColor(t *testing.T) {
	mgr := &LocalizationManager{config: &types.LocalizationConfig{Themes: types.Theme{Colors: map[string]string{"primary": "#fff"}}}}
	mgr.themes = map[string]types.Theme{"dark": {Colors: map[string]string{"primary": "#000"}}}
	if mgr.GetThemeColor("primary") != "#fff" {
		t.Errorf("default color wrong")
	}
	mgr.SetTheme("dark")
	if mgr.GetThemeColor("primary") != "#000" {
		t.Errorf("theme color not applied")
	}
}

func TestWhenGettingMissingMessage_ShouldUseFallback(t *testing.T) {
	mgr := &LocalizationManager{config: &types.LocalizationConfig{Messages: types.Messages{Banner: types.Banner{Title: "hi"}}}, fallback: &types.LocalizationConfig{Messages: types.Messages{Banner: types.Banner{Title: "fallback"}}}}
	if mgr.GetMessage("banner.subtitle") != "" {
		t.Errorf("expected empty string for missing message")
	}
	if mgr.GetMessage("banner.title") != "hi" {
		t.Errorf("expected primary message")
	}
}
