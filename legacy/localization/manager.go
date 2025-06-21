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
	} else if cfg, err := LoadFromFile(filepath.Join(configDir, "..", "themes", "en-us.json")); err == nil {
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
		switch field {
		case "provider_connection":
			return cfg.Messages.Errors.ProviderConnection
		case "invalid_input":
			return cfg.Messages.Errors.InvalidInput
		case "invalid_choice":
			return cfg.Messages.Errors.InvalidInput // fallback
		}
	case "prompts":
		switch field {
		case "select_provider":
			return cfg.Messages.Prompts.SelectProvider
		case "enter_request":
			return cfg.Messages.Prompts.EnterRequest
		}
	case "installation":
		switch field {
		case "success":
			return cfg.Messages.Installation.Success
		case "try_it":
			return cfg.Messages.Installation.TryIt
		case "magic":
			return cfg.Messages.Installation.Magic
		}
	case "footer":
		switch field {
		case "tips":
			return cfg.Messages.Footer.Tips
		case "happy":
			return cfg.Messages.Footer.Happy
		}
	case "script_display":
		switch field {
		case "task_label":
			return cfg.Messages.ScriptDisplay.TaskLabel
		case "model_label":
			return cfg.Messages.ScriptDisplay.ModelLabel
		case "platform_label":
			return cfg.Messages.ScriptDisplay.PlatformLabel
		case "script_header":
			return cfg.Messages.ScriptDisplay.ScriptHeader
		case "success_message":
			return cfg.Messages.ScriptDisplay.SuccessMessage
		}
	case "menu":
		switch field {
		case "generate_script":
			return cfg.Messages.Menu.GenerateScript
		case "run_last":
			return cfg.Messages.Menu.RunLast
		case "help":
			return cfg.Messages.Menu.Help
		case "exit":
			return cfg.Messages.Menu.Exit
		case "main_prompt":
			return cfg.Messages.Menu.MainPrompt
		case "show_help":
			return cfg.Messages.Menu.ShowHelp
		case "load_last":
			return cfg.Messages.Menu.LoadLast
		case "browse_history":
			return cfg.Messages.Menu.BrowseHistory
		case "show_config":
			return cfg.Messages.Menu.ShowConfig
		}
	case "menus":
		switch field {
		case "show_help":
			return cfg.Messages.Menus.ShowHelp
		case "generate_script":
			return cfg.Messages.Menus.GenerateScript
		case "load_last":
			return cfg.Messages.Menus.LoadLast
		case "browse_history":
			return cfg.Messages.Menus.BrowseHistory
		case "show_config":
			return cfg.Messages.Menus.ShowConfig
		case "exit":
			return cfg.Messages.Menus.Exit
		case "main_prompt":
			return cfg.Messages.Menus.MainPrompt
		}
	case "success":
		switch field {
		case "exit":
			return cfg.Messages.Success.Exit
		case "exit_quick":
			return cfg.Messages.Success.ExitQuick
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
