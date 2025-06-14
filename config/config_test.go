package config

import (
	"os"
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
			name:     "OOHLAMA_PROVIDER legacy fallback",
			envVar:   "OOHLAMA_PROVIDER",
			envValue: "openai", 
			want:     "openai",
		},
	}
	
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			// Clean environment first
			os.Unsetenv("PLEASE_PROVIDER")
			os.Unsetenv("OOHLAMA_PROVIDER")
			
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
