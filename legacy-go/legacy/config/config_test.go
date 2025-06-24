package config

import (
	"os"
	"strings"
	"testing"

	"please/types"
)

func TestCreateDefault(t *testing.T) {
	cfg := CreateDefault()

	if cfg == nil {
		t.Error("CreateDefault should return a non-nil config")
	}

	if cfg.Provider == "" {
		t.Error("Default config should have a provider set")
	}
}

func TestDetermineProvider(t *testing.T) {
	tests := []struct {
		name     string
		envVar   string
		envValue string
		want     string
	}{
		{
			name:     "PLEASE_PROVIDER set to ollama",
			envVar:   "PLEASE_PROVIDER",
			envValue: "ollama",
			want:     "ollama",
		},
		{
			name:     "PLEASE_PROVIDER set to openai",
			envVar:   "PLEASE_PROVIDER",
			envValue: "openai",
			want:     "openai",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			// Clean environment first
			os.Unsetenv("PLEASE_PROVIDER")

			// Set the test environment variable
			os.Setenv(tt.envVar, tt.envValue)
			defer os.Unsetenv(tt.envVar)

			cfg := CreateDefault()
			got := DetermineProvider(cfg)

			if got != tt.want {
				t.Errorf("DetermineProvider() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestDetermineScriptType(t *testing.T) {
	cfg := &types.Config{}

	scriptType := DetermineScriptType(cfg)

	// Should return a valid script type
	validTypes := []string{"powershell", "bash", "zsh", "sh"}
	found := false
	for _, validType := range validTypes {
		if scriptType == validType {
			found = true
			break
		}
	}

	if !found {
		t.Errorf("DetermineScriptType() returned invalid type: %s", scriptType)
	}
}

func TestLoadSave(t *testing.T) {
	// Test creating default config
	cfg := CreateDefault()
	cfg.Provider = "test-provider"

	// Save should not panic or error for basic functionality
	err := Save(cfg)
	if err != nil {
		t.Logf("Save returned error (expected in test environment): %v", err)
	}

	// Load should return some config even if file doesn't exist
	loadedCfg, err := Load()
	if err != nil {
		t.Logf("Load returned error (expected in test environment): %v", err)
	}

	if loadedCfg == nil {
		t.Error("Load should return a config even on error")
	}
}

func Test_when_environment_variables_set_then_override_config_values(t *testing.T) {
	// Test environment variable overrides
	testCases := []struct {
		name   string
		envVar string
		envVal string
		check  func(*types.Config) bool
	}{
		{
			name:   "OPENAI_API_KEY override",
			envVar: "OPENAI_API_KEY",
			envVal: "test-openai-key",
			check:  func(cfg *types.Config) bool { return cfg.OpenAIAPIKey == "test-openai-key" },
		},
		{
			name:   "ANTHROPIC_API_KEY override",
			envVar: "ANTHROPIC_API_KEY",
			envVal: "test-anthropic-key",
			check:  func(cfg *types.Config) bool { return cfg.AnthropicAPIKey == "test-anthropic-key" },
		},
		{
			name:   "OLLAMA_URL override",
			envVar: "OLLAMA_URL",
			envVal: "http://custom:8080",
			check:  func(cfg *types.Config) bool { return cfg.OllamaURL == "http://custom:8080" },
		},
		{
			name:   "PLEASE_SCRIPT_TYPE override",
			envVar: "PLEASE_SCRIPT_TYPE",
			envVal: "bash",
			check:  func(cfg *types.Config) bool { return cfg.ScriptType == "bash" },
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Set environment variable
			t.Setenv(tc.envVar, tc.envVal)

			// Create default config and apply environment overrides
			cfg := CreateDefault()
			overrideWithEnvironment(cfg)

			// Check if override was applied
			if !tc.check(cfg) {
				t.Errorf("Environment variable %s was not properly applied", tc.envVar)
			}
		})
	}
}

func Test_when_determining_provider_then_respect_fallback_hierarchy(t *testing.T) {
	tests := []struct {
		name             string
		envProvider      string
		configProvider   string
		expectedProvider string
	}{
		{
			name:             "Environment variable takes precedence",
			envProvider:      "openai",
			configProvider:   "anthropic",
			expectedProvider: "openai",
		},
		{
			name:             "Config provider when no environment",
			envProvider:      "",
			configProvider:   "anthropic",
			expectedProvider: "anthropic",
		},
		{
			name:             "Default to ollama when neither set",
			envProvider:      "",
			configProvider:   "",
			expectedProvider: "ollama",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			// Clean environment
			os.Unsetenv("PLEASE_PROVIDER")

			// Set environment if specified
			if tt.envProvider != "" {
				t.Setenv("PLEASE_PROVIDER", tt.envProvider)
			}

			// Create config with specified provider
			cfg := &types.Config{Provider: tt.configProvider}

			result := DetermineProvider(cfg)
			if result != tt.expectedProvider {
				t.Errorf("DetermineProvider() = %s, want %s", result, tt.expectedProvider)
			}
		})
	}
}

func Test_when_determining_script_type_then_handle_explicit_settings(t *testing.T) {
	tests := []struct {
		name           string
		configType     string
		expectedResult string
	}{
		{
			name:           "Explicit powershell setting",
			configType:     "powershell",
			expectedResult: "powershell",
		},
		{
			name:           "Explicit bash setting",
			configType:     "bash",
			expectedResult: "bash",
		},
		{
			name:           "Auto setting falls back to platform default",
			configType:     "auto",
			expectedResult: "", // Will be platform-specific
		},
		{
			name:           "Empty setting falls back to platform default",
			configType:     "",
			expectedResult: "", // Will be platform-specific
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			cfg := &types.Config{ScriptType: tt.configType}
			result := DetermineScriptType(cfg)

			if tt.expectedResult != "" {
				// Check explicit expected result
				if result != tt.expectedResult {
					t.Errorf("DetermineScriptType() = %s, want %s", result, tt.expectedResult)
				}
			} else {
				// Check that it returns a valid script type for platform defaults
				validTypes := []string{"powershell", "bash"}
				found := false
				for _, validType := range validTypes {
					if result == validType {
						found = true
						break
					}
				}
				if !found {
					t.Errorf("DetermineScriptType() = %s, should be one of %v", result, validTypes)
				}
			}
		})
	}
}

func Test_when_creating_default_config_then_initialize_all_required_fields(t *testing.T) {
	cfg := CreateDefault()

	tests := []struct {
		name  string
		check func() bool
		desc  string
	}{
		{
			name:  "Provider is set",
			check: func() bool { return cfg.Provider != "" },
			desc:  "Provider should not be empty",
		},
		{
			name:  "ScriptType is set",
			check: func() bool { return cfg.ScriptType != "" },
			desc:  "ScriptType should not be empty",
		},
		{
			name:  "OllamaURL is set",
			check: func() bool { return cfg.OllamaURL != "" },
			desc:  "OllamaURL should not be empty",
		},
		{
			name:  "ModelOverrides is initialized",
			check: func() bool { return cfg.ModelOverrides != nil },
			desc:  "ModelOverrides map should be initialized",
		},
		{
			name:  "CustomProviders is initialized",
			check: func() bool { return cfg.CustomProviders != nil },
			desc:  "CustomProviders map should be initialized",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if !tt.check() {
				t.Error(tt.desc)
			}
		})
	}
}

func Test_when_loading_config_then_handle_missing_file_gracefully(t *testing.T) {
	// This test verifies that Load() returns a default config when file doesn't exist
	// Note: This may actually try to read from real config locations, so we handle that

	cfg, err := Load()

	// Should always return a config, even if file doesn't exist
	if cfg == nil {
		t.Error("Load() should return a config even when file doesn't exist")
	}

	// Error is acceptable (file may not exist), but config should be valid
	if cfg != nil {
		if cfg.ModelOverrides == nil {
			t.Error("Loaded config should have initialized ModelOverrides")
		}
		if cfg.CustomProviders == nil {
			t.Error("Loaded config should have initialized CustomProviders")
		}
	}

	// Log error for debugging but don't fail test
	if err != nil {
		t.Logf("Load returned error (acceptable): %v", err)
	}
}

func Test_when_config_has_platform_specific_behavior_then_work_cross_platform(t *testing.T) {
	// Test that getConfigPath works on current platform
	configPath, err := getConfigPath()

	if err != nil {
		t.Errorf("getConfigPath() should not error on supported platforms: %v", err)
	}

	if configPath == "" {
		t.Error("getConfigPath() should return non-empty path")
	}

	// Path should end with config.json
	if !strings.HasSuffix(configPath, "config.json") {
		t.Errorf("Config path should end with 'config.json', got: %s", configPath)
	}
}
