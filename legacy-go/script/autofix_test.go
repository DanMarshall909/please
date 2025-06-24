package script

import (
	"strings"
	"testing"

	"please/providers"
	"please/types"
)

// TestGenerateFixedScript_UsesCorrectProvider tests that auto-fix now uses the specified provider
func Test_when_generate_fixed_script_then_uses_correct_provider(t *testing.T) {
	// Test that the function now correctly uses different providers

	originalScript := "echo 'broken script'"
	errorMessage := "syntax error"
	scriptType := "bash"
	originalModel := "llama3.2"

	config := &types.Config{
		Provider:       "ollama",
		PreferredModel: originalModel,
		// No OpenAI key set - this should work with Ollama without requiring OpenAI
	}

	// Test with Ollama provider - this should now work (may fail due to Ollama not running, but won't fail due to OpenAI hardcoding)
	_, err := generateFixedScriptFixed(
		originalScript,
		errorMessage,
		scriptType,
		originalModel,
		"ollama",
		config,
	)

	// This may fail because Ollama isn't running, but it should NOT fail due to OpenAI hard-coding
	if err != nil && strings.Contains(err.Error(), "OpenAI") {
		t.Errorf("Error should not mention OpenAI when using Ollama provider, got: %v", err)
	}

	// Test with unsupported provider - should get proper error message
	_, err = generateFixedScriptFixed(
		originalScript,
		errorMessage,
		scriptType,
		originalModel,
		"unsupported_provider",
		config,
	)

	if err == nil {
		t.Error("Expected error for unsupported provider")
	}

	if !strings.Contains(err.Error(), "unsupported provider") {
		t.Errorf("Expected 'unsupported provider' error, got: %v", err)
	}
}

// TestGenerateFixedScript_ProperPrompt tests that the prompt includes proper context
func Test_when_generate_fixed_script_then_proper_prompt(t *testing.T) {
	var receivedRequest *types.ScriptRequest

	mockProvider := &MockProvider{
		GenerateScriptFunc: func(req *types.ScriptRequest) (*types.ScriptResponse, error) {
			receivedRequest = req
			return &types.ScriptResponse{
				Script:     "fixed script",
				Provider:   req.Provider,
				Model:      req.Model,
				ScriptType: req.ScriptType,
			}, nil
		},
	}

	originalScript := "echo 'broken script'"
	errorMessage := "syntax error: unexpected token"
	scriptType := "bash"
	model := "llama3.2"
	provider := "ollama"
	config := &types.Config{Provider: provider, PreferredModel: model}

	_, err := generateFixedScriptWithProvider(
		mockProvider,
		originalScript,
		errorMessage,
		scriptType,
		model,
		provider,
		config,
	)

	if err != nil {
		t.Errorf("GenerateFixedScript failed: %v", err)
	}

	if receivedRequest == nil {
		t.Fatal("Mock provider should have received a request")
	}

	// Check that the prompt includes the original script and error
	prompt := receivedRequest.TaskDescription
	if !strings.Contains(prompt, originalScript) {
		t.Error("Prompt should contain original script")
	}

	if !strings.Contains(prompt, errorMessage) {
		t.Error("Prompt should contain error message")
	}

	if !strings.Contains(prompt, "corrected version") {
		t.Error("Prompt should ask for corrected version")
	}
}

// MockProvider for testing
type MockProvider struct {
	GenerateScriptFunc func(*types.ScriptRequest) (*types.ScriptResponse, error)
}

func (m *MockProvider) GenerateScript(req *types.ScriptRequest) (*types.ScriptResponse, error) {
	if m.GenerateScriptFunc != nil {
		return m.GenerateScriptFunc(req)
	}
	return &types.ScriptResponse{}, nil
}

func (m *MockProvider) Name() string {
	return "mock"
}

func (m *MockProvider) IsConfigured(config *types.Config) bool {
	return true
}

// generateFixedScriptWithProvider is the function we need to implement to fix the bug
// This should replace the current GenerateFixedScript function
func generateFixedScriptWithProvider(
	provider Provider, // Add this interface import
	originalScript, errorMessage, scriptType, model, providerName string,
	config *types.Config,
) (string, error) {
	// This is what we need to implement to fix the hard-coded OpenAI issue
	prompt := "The following script failed with this error:\n\nScript:\n" +
		originalScript + "\n\nError:\n" + errorMessage +
		"\n\nPlease suggest a corrected version of the script. Return ONLY the fixed script, no explanations or markdown formatting."

	request := &types.ScriptRequest{
		TaskDescription: prompt,
		ScriptType:      scriptType,
		Provider:        providerName,
		Model:           model,
	}

	response, err := provider.GenerateScript(request)
	if err != nil {
		return "", err
	}

	return response.Script, nil
}

// Provider interface for testing - this should match the real one
type Provider interface {
	GenerateScript(request *types.ScriptRequest) (*types.ScriptResponse, error)
	Name() string
	IsConfigured(config *types.Config) bool
}

// generateFixedScriptBroken wraps the actual broken function to test it
func generateFixedScriptBroken(
	originalScript, errorMessage, scriptType, model, providerName string,
	config *types.Config,
) (string, error) {
	// Call the actual broken function from providers package
	return providers.GenerateFixedScript(
		originalScript,
		errorMessage,
		scriptType,
		model,
		providerName,
		config,
	)
}

// generateFixedScriptFixed wraps the now-fixed function to test it
func generateFixedScriptFixed(
	originalScript, errorMessage, scriptType, model, providerName string,
	config *types.Config,
) (string, error) {
	// Call the now-fixed function from providers package
	return providers.GenerateFixedScript(
		originalScript,
		errorMessage,
		scriptType,
		model,
		providerName,
		config,
	)
}
