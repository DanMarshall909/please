package providers_test

import (
	"testing"
	"please/providers"
	"please/types"
)

// TestGenerateFixedScript_Ollama_NoOpenAIKey ensures that using Ollama as provider does not require an OpenAI key
func TestGenerateFixedScript_Ollama_NoOpenAIKey(t *testing.T) {
	config := &types.Config{
		Provider:      "ollama",
		OpenAIAPIKey:  "", // No OpenAI key
		OllamaURL:     "http://localhost:11434", // Default Ollama URL
	}

	script := "echo 'broken script'"
	errorMsg := "syntax error"
	scriptType := "bash"
	model := "llama3.2"
	provider := "ollama"

	// This should not error due to missing OpenAI key
	_, err := providers.GenerateFixedScript(script, errorMsg, scriptType, model, provider, config)
	if err != nil && (err.Error() == "OpenAI API key not configured. Please set OPENAI_API_KEY environment variable or use 'please set openai key'") {
		t.Fatalf("Ollama provider should not require OpenAI key, got error: %v", err)
	}
}
