package types

import (
	"testing"
	"time"
)

func TestConfig(t *testing.T) {
	cfg := &Config{
		Provider:        "ollama",
		PreferredModel:  "llama3.2",
		ScriptType:      "powershell",
		ModelOverrides:  make(map[string]string),
		CustomProviders: make(map[string]ProviderConfig),
	}

	if cfg.Provider != "ollama" {
		t.Errorf("Expected provider 'ollama', got '%s'", cfg.Provider)
	}

	if cfg.PreferredModel != "llama3.2" {
		t.Errorf("Expected model 'llama3.2', got '%s'", cfg.PreferredModel)
	}

	if cfg.ModelOverrides == nil {
		t.Error("ModelOverrides should be initialized")
	}

	if cfg.CustomProviders == nil {
		t.Error("CustomProviders should be initialized")
	}
}

func TestProviderConfig(t *testing.T) {
	providerCfg := ProviderConfig{
		URL:     "http://localhost:11434",
		APIKey:  "test-key",
		Model:   "test-model",
		Headers: map[string]string{"Authorization": "Bearer test"},
	}

	if providerCfg.URL != "http://localhost:11434" {
		t.Errorf("Expected URL 'http://localhost:11434', got '%s'", providerCfg.URL)
	}

	if providerCfg.APIKey != "test-key" {
		t.Errorf("Expected API key 'test-key', got '%s'", providerCfg.APIKey)
	}

	if providerCfg.Headers["Authorization"] != "Bearer test" {
		t.Error("Headers should contain Authorization")
	}
}

func TestModelInfo(t *testing.T) {
	now := time.Now()
	modelInfo := ModelInfo{
		Name:       "llama3.2",
		ModifiedAt: now,
		Size:       1024,
		Digest:     "abc123",
	}

	if modelInfo.Name != "llama3.2" {
		t.Errorf("Expected name 'llama3.2', got '%s'", modelInfo.Name)
	}

	if modelInfo.Size != 1024 {
		t.Errorf("Expected size 1024, got %d", modelInfo.Size)
	}

	if modelInfo.ModifiedAt != now {
		t.Error("ModifiedAt should match the set time")
	}
}

func TestScriptRequest(t *testing.T) {
	request := ScriptRequest{
		TaskDescription: "Create a backup script",
		ScriptType:      "powershell",
		Provider:        "ollama",
		Model:           "llama3.2",
	}

	if request.TaskDescription != "Create a backup script" {
		t.Errorf("Expected task description 'Create a backup script', got '%s'", request.TaskDescription)
	}

	if request.ScriptType != "powershell" {
		t.Errorf("Expected script type 'powershell', got '%s'", request.ScriptType)
	}

	if request.Provider != "ollama" {
		t.Errorf("Expected provider 'ollama', got '%s'", request.Provider)
	}
}

func TestScriptResponse(t *testing.T) {
	response := ScriptResponse{
		Script:          "Get-Date",
		Model:           "llama3.2",
		Provider:        "ollama",
		TaskDescription: "Show current date",
		ScriptType:      "powershell",
	}

	if response.Script != "Get-Date" {
		t.Errorf("Expected script 'Get-Date', got '%s'", response.Script)
	}

	if response.TaskDescription != "Show current date" {
		t.Errorf("Expected task description 'Show current date', got '%s'", response.TaskDescription)
	}
}

func TestMessage(t *testing.T) {
	message := Message{
		Role:    "user",
		Content: "Hello, how are you?",
	}

	if message.Role != "user" {
		t.Errorf("Expected role 'user', got '%s'", message.Role)
	}

	if message.Content != "Hello, how are you?" {
		t.Errorf("Expected content 'Hello, how are you?', got '%s'", message.Content)
	}
}

func TestOpenAIRequest(t *testing.T) {
	request := OpenAIRequest{
		Model:       "gpt-4",
		Messages:    []Message{{Role: "user", Content: "test"}},
		Temperature: 0.7,
		MaxTokens:   1000,
	}

	if request.Model != "gpt-4" {
		t.Errorf("Expected model 'gpt-4', got '%s'", request.Model)
	}

	if len(request.Messages) != 1 {
		t.Errorf("Expected 1 message, got %d", len(request.Messages))
	}

	if request.Temperature != 0.7 {
		t.Errorf("Expected temperature 0.7, got %f", request.Temperature)
	}
}

func TestOllamaRequest(t *testing.T) {
	request := OllamaRequest{
		Model:   "llama3.2",
		Prompt:  "Hello world",
		Stream:  false,
		Options: make(map[string]interface{}),
	}

	if request.Model != "llama3.2" {
		t.Errorf("Expected model 'llama3.2', got '%s'", request.Model)
	}

	if request.Prompt != "Hello world" {
		t.Errorf("Expected prompt 'Hello world', got '%s'", request.Prompt)
	}

	if request.Stream != false {
		t.Error("Expected stream to be false")
	}
}
