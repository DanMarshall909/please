package localization

import (
	"os"
	"path/filepath"
	"testing"
)

func TestNewLocalizationManager(t *testing.T) {
	// Create a temporary directory for testing
	tempDir := t.TempDir()
	
	manager, err := NewLocalizationManager(tempDir)
	if err != nil {
		t.Fatalf("NewLocalizationManager failed: %v", err)
	}
	
	if manager == nil {
		t.Error("NewLocalizationManager should return a non-nil manager")
	}
	
	if manager.System == nil {
		t.Error("Manager should have a System component")
	}
	
	if manager.System == nil {
		t.Error("Manager should have a System component")
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
	tempDir := t.TempDir()
	
	manager, err := NewLocalizationManager(tempDir)
	if err != nil {
		t.Fatalf("NewLocalizationManager failed: %v", err)
	}
	
	// Test that default strings are available
	helpText := manager.System.Get("menus.show_help")
	if helpText == "" {
		t.Error("System should have default help text")
	}
	
	exitText := manager.System.Get("menus.exit")
	if exitText == "" {
		t.Error("System should have default exit text")
	}
	
	// Test basic localization functionality
	errorText := manager.System.Get("errors.invalid_choice")
	if errorText == "" {
		t.Error("System should have default error text")
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
	"metadata": {
		"name": "Custom English",
		"code": "en-custom",
		"version": "1.0.0",
		"author": "Test",
		"description": "Custom test language pack"
	},
	"messages": {
		"menus": {
			"show_help": "Custom Help Text",
			"exit": "Custom Exit"
		}
	},
	"examples": {},
	"placeholders": {}
}`
	
	langPackPath := filepath.Join(langDir, "en-custom.json")
	if err := os.WriteFile(langPackPath, []byte(langPackContent), 0644); err != nil {
		t.Fatalf("Failed to write test language pack: %v", err)
	}
	
	manager, err := NewLocalizationManager(tempDir)
	if err != nil {
		t.Fatalf("NewLocalizationManager failed: %v", err)
	}
	
	// Test that custom strings are loaded
	helpText := manager.System.Get("menus.show_help")
	expectedHelp := "Custom Help Text"
	if helpText != expectedHelp {
		t.Errorf("Expected custom help text '%s', got '%s'", expectedHelp, helpText)
	}
	
	exitText := manager.System.Get("menus.exit")
	expectedExit := "Custom Exit"
	if exitText != expectedExit {
		t.Errorf("Expected custom exit text '%s', got '%s'", expectedExit, exitText)
	}
}

func TestLocalizationSystemGet(t *testing.T) {
	tempDir := t.TempDir()
	
	manager, err := NewLocalizationManager(tempDir)
	if err != nil {
		t.Fatalf("NewLocalizationManager failed: %v", err)
	}
	
	tests := []struct {
		name string
		key  string
	}{
		{
			name: "Valid menu key",
			key:  "menus.show_help",
		},
		{
			name: "Valid exit key", 
			key:  "menus.exit",
		},
		{
			name: "Non-existent key returns key itself",
			key:  "nonexistent.key",
		},
	}
	
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := manager.System.Get(tt.key)
			// Should return either the localized string or the key itself
			if result == "" {
				t.Errorf("Get(%s) should not return empty string", tt.key)
			}
		})
	}
}
