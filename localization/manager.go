package localization

import (
	"path/filepath"
	"strings"

	"please/types"
)

// LocalizationManager manages localization config and themes
type LocalizationManager struct {
	config   *types.LocalizationConfig
	fallback *types.LocalizationConfig
	files    map[string]string
	themes   map[string]types.Theme
	dir      string
}

// NewLocalizationManager creates a manager using provided fallback configuration
func NewLocalizationManager(configDir string) (*LocalizationManager, error) {
	mgr := &LocalizationManager{
		files:  make(map[string]string),
		themes: make(map[string]types.Theme),
		dir:    configDir,
	}
	mgr.config = &types.LocalizationConfig{Language: "en-us", Theme: "default"}
	if cfg, err := LoadFromFile(filepath.Join(configDir, "themes", "en-us.json")); err == nil {
		mgr.config.Messages = cfg.Messages
		mgr.config.Themes = cfg.Themes
		mgr.fallback = cfg
	} else {
		mgr.fallback = mgr.config
		mgr.config.Themes = types.Theme{Colors: map[string]string{"primary": "#00ff41"}}
	}
	return mgr, nil
}

// LoadLanguage registers a language file path
func (m *LocalizationManager) LoadLanguage(code, path string) {
	m.files[code] = path
}

// LoadTheme registers a theme by name
func (m *LocalizationManager) LoadTheme(name string, theme types.Theme) {
	m.themes[name] = theme
}

// SetLanguage loads and activates the given language code
func (m *LocalizationManager) SetLanguage(code string) {
	if path, ok := m.files[code]; ok {
		if cfg, err := LoadFromFile(path); err == nil {
			m.config.Language = code
			m.config.Messages = cfg.Messages
		}
	}
}

// SetTheme switches the active theme
func (m *LocalizationManager) SetTheme(name string) {
	if t, ok := m.themes[name]; ok {
		m.config.Theme = name
		m.config.Themes = t
	}
}

// GetMessage returns a localized message by dotted key
func (m *LocalizationManager) GetMessage(key string) string {
	parts := strings.Split(key, ".")
	if len(parts) != 2 {
		return ""
	}
	if val := getFromConfig(m.config, parts[0], parts[1]); val != "" {
		return val
	}
	if m.fallback != nil {
		return getFromConfig(m.fallback, parts[0], parts[1])
	}
	return ""
}

func getFromConfig(cfg *types.LocalizationConfig, cat, field string) string {
	if cfg == nil {
		return ""
	}
	switch cat {
	case "banner":
		switch field {
		case "title":
			return cfg.Messages.Banner.Title
		case "subtitle":
			return cfg.Messages.Banner.Subtitle
		}
	case "errors":
		if field == "provider_connection" {
			return cfg.Messages.Errors.ProviderConnection
		}
		if field == "invalid_input" {
			return cfg.Messages.Errors.InvalidInput
		}
	case "prompts":
		if field == "select_provider" {
			return cfg.Messages.Prompts.SelectProvider
		}
		if field == "enter_request" {
			return cfg.Messages.Prompts.EnterRequest
		}
	}
	return ""
}

// GetThemeColor retrieves a color value for the current theme
func (m *LocalizationManager) GetThemeColor(name string) string {
	if val, ok := m.config.Themes.Colors[name]; ok {
		return val
	}
	if m.fallback != nil {
		return m.fallback.Themes.Colors[name]
	}
	return ""
}
