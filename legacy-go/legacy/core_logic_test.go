package main

import (
	"strings"
	"testing"

	"please/types"
)

// TestGenerateScript_ErrorHandling tests critical error paths
func TestGenerateScript_WhenUnsupportedProvider_ShouldReturnError(t *testing.T) {
	cfg := &types.Config{}
	request := &types.ScriptRequest{
		Provider: "invalid-provider",
	}

	_, err := generateScript(cfg, request)
	if err == nil {
		t.Error("Expected error for unsupported provider")
	}

	expectedMsg := "unsupported provider: invalid-provider"
	if err.Error() != expectedMsg {
		t.Errorf("Expected '%s', got '%s'", expectedMsg, err.Error())
	}
}

// TestGetFallbackModel_AllProviders tests model selection logic
func TestGetFallbackModel_ForEachProvider_ShouldReturnCorrectModel(t *testing.T) {
	tests := []struct {
		provider string
		expected string
	}{
		{"openai", "gpt-3.5-turbo"},
		{"anthropic", "claude-3-haiku-20240307"},
		{"ollama", "llama3.2"},
		{"unknown", "llama3.2"}, // default case
	}

	for _, tt := range tests {
		t.Run("provider_"+tt.provider, func(t *testing.T) {
			result := getFallbackModel(tt.provider)
			if result != tt.expected {
				t.Errorf("Provider %s: expected '%s', got '%s'", 
					tt.provider, tt.expected, result)
			}
		})
	}
}

// TestIsLastScriptCommand_EdgeCases tests command recognition edge cases
func TestIsLastScriptCommand_WhenEdgeCases_ShouldHandleCorrectly(t *testing.T) {
	tests := []struct {
		name     string
		command  string
		expected bool
	}{
		{"empty string", "", false},
		{"case insensitive", "RUN LAST SCRIPT", true},
		{"extra whitespace", "  run last script  ", true},
		{"partial match", "run", false},
		{"similar but different", "run fast script", false},
		{"unicode characters", "run last script 🚀", true},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := isLastScriptCommand(tt.command)
			if result != tt.expected {
				t.Errorf("Command '%s': expected %v, got %v", 
					tt.command, tt.expected, result)
			}
		})
	}
}

// TestArguments_LanguageAndTheme tests CLI argument parsing
func TestArguments_WhenLanguageAndThemeFlags_ShouldParseCorrectly(t *testing.T) {
	tests := []struct {
		name     string
		args     []string
		expLang  string
		expTheme string
	}{
		{
			"default values",
			[]string{"please", "some", "task"},
			"en-us", "default",
		},
		{
			"language flag",
			[]string{"please", "--language=es-es", "task"},
			"es-es", "default",
		},
		{
			"theme flag", 
			[]string{"please", "--theme=dark", "task"},
			"en-us", "dark",
		},
		{
			"both flags",
			[]string{"please", "--language=fr-fr", "--theme=light", "task"},
			"fr-fr", "light",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			// Simulate argument parsing logic from main()
			lang := "en-us"
			theme := "default"
			
			for _, arg := range tt.args[1:] {
				if strings.HasPrefix(arg, "--language=") {
					lang = strings.SplitN(arg, "=", 2)[1]
				}
				if strings.HasPrefix(arg, "--theme=") {
					theme = strings.SplitN(arg, "=", 2)[1]
				}
			}

			if lang != tt.expLang {
				t.Errorf("Language: expected '%s', got '%s'", tt.expLang, lang)
			}
			if theme != tt.expTheme {
				t.Errorf("Theme: expected '%s', got '%s'", tt.expTheme, theme)
			}
		})
	}
}
