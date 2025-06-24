package ui

import (
	"os"
	"runtime"
	"strings"
	"testing"
)

// Test getConfigDir function
func Test_when_getting_config_dir_then_return_platform_specific_path(t *testing.T) {
	// Act
	configDir, err := getConfigDir()

	// Assert
	if err != nil {
		t.Errorf("Expected no error, got: %v", err)
	}

	if configDir == "" {
		t.Error("Expected non-empty config directory path")
	}

	// Verify platform-specific path format
	switch runtime.GOOS {
	case "windows":
		if !strings.Contains(configDir, "please") {
			t.Errorf("Expected Windows config dir to contain 'please', got: %s", configDir)
		}
	case "darwin":
		if !strings.Contains(configDir, "Library/Application Support") {
			t.Errorf("Expected macOS config dir to contain 'Library/Application Support', got: %s", configDir)
		}
	default: // Linux and others
		if !strings.Contains(configDir, ".config") {
			t.Errorf("Expected Linux config dir to contain '.config', got: %s", configDir)
		}
	}
}

func Test_when_getting_config_dir_then_create_directory_if_not_exists(t *testing.T) {
	// Act
	configDir, err := getConfigDir()

	// Assert
	if err != nil {
		t.Errorf("Expected no error, got: %v", err)
	}

	// Verify directory was created
	if _, statErr := os.Stat(configDir); os.IsNotExist(statErr) {
		t.Errorf("Expected config directory to be created at: %s", configDir)
	}
}
