package types

import "time"

// Config represents the application configuration
type Config struct {
	PreferredModel  string                    `json:"preferred_model"`
	ModelOverrides  map[string]string         `json:"model_overrides"`
	Provider        string                    `json:"provider"`    // "ollama", "openai", "anthropic", etc.
	ScriptType      string                    `json:"script_type"` // "auto", "powershell", "bash"
	OpenAIAPIKey    string                    `json:"openai_api_key"`
	AnthropicAPIKey string                    `json:"anthropic_api_key"`
	OllamaURL       string                    `json:"ollama_url"`
	CustomProviders map[string]ProviderConfig `json:"custom_providers"`
}

// ProviderConfig represents configuration for a custom AI provider
type ProviderConfig struct {
	URL     string            `json:"url"`
	APIKey  string            `json:"api_key"`
	Headers map[string]string `json:"headers"`
	Model   string            `json:"model"`
}

// ModelInfo represents information about an available model
type ModelInfo struct {
	Name       string    `json:"name"`
	ModifiedAt time.Time `json:"modified_at"`
	Size       int64     `json:"size"`
	Digest     string    `json:"digest"`
	Details    struct {
		Format string `json:"format"`
		Family string `json:"family"`
	} `json:"details"`
}

// ModelsResponse represents the response from listing models
type ModelsResponse struct {
	Models []ModelInfo `json:"models"`
}

// OllamaRequest represents a request to the Ollama API
type OllamaRequest struct {
	Model   string                 `json:"model"`
	Prompt  string                 `json:"prompt"`
	Stream  bool                   `json:"stream"`
	Options map[string]interface{} `json:"options"`
}

// OllamaResponse represents a response from the Ollama API
type OllamaResponse struct {
	Response string `json:"response"`
}

// Message represents a chat message for API requests
type Message struct {
	Role    string `json:"role"`
	Content string `json:"content"`
}

// OpenAIRequest represents a request to the OpenAI API
type OpenAIRequest struct {
	Model       string    `json:"model"`
	Messages    []Message `json:"messages"`
	Temperature float64   `json:"temperature"`
	MaxTokens   int       `json:"max_tokens"`
}

// OpenAIResponse represents a response from the OpenAI API
type OpenAIResponse struct {
	Choices []Choice `json:"choices"`
}

// Choice represents a choice in an OpenAI response
type Choice struct {
	Message Message `json:"message"`
}

// AnthropicRequest represents a request to the Anthropic API
type AnthropicRequest struct {
	Model       string    `json:"model"`
	MaxTokens   int       `json:"max_tokens"`
	Messages    []Message `json:"messages"`
	Temperature float64   `json:"temperature"`
}

// AnthropicResponse represents a response from the Anthropic API
type AnthropicResponse struct {
	Content []ContentBlock `json:"content"`
}

// ContentBlock represents a content block in an Anthropic response
type ContentBlock struct {
	Type string `json:"type"`
	Text string `json:"text"`
}

// ScriptRequest represents a request to generate a script
type ScriptRequest struct {
	TaskDescription string
	ScriptType      string
	Provider        string
	Model           string
}

// ScriptResponse represents the response from script generation
type ScriptResponse struct {
	Script          string
	Model           string
	Provider        string
	TaskDescription string
	ScriptType      string
}
