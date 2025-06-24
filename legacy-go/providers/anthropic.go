package providers

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"strings"
	"time"

	"please/types"
)

// AnthropicProvider implements the Provider interface for Anthropic Claude
type AnthropicProvider struct {
	config *types.Config
}

// NewAnthropicProvider creates a new Anthropic provider
func NewAnthropicProvider(config *types.Config) *AnthropicProvider {
	return &AnthropicProvider{config: config}
}

// Name returns the provider name
func (p *AnthropicProvider) Name() string {
	return "anthropic"
}

// IsConfigured checks if Anthropic is properly configured
func (p *AnthropicProvider) IsConfigured(config *types.Config) bool {
	return config.AnthropicAPIKey != ""
}

// GenerateScript generates a script using Anthropic's API
func (p *AnthropicProvider) GenerateScript(request *types.ScriptRequest) (*types.ScriptResponse, error) {
	if !p.IsConfigured(p.config) {
		return nil, fmt.Errorf("Anthropic API key not configured. Please set ANTHROPIC_API_KEY environment variable or use 'please set anthropic key'")
	}

	prompt := CreatePrompt(request.TaskDescription, request.ScriptType)
	
	// Determine the model to use
	model := request.Model
	if model == "" {
		model = p.getDefaultModel()
	}

	anthropicRequest := types.AnthropicRequest{
		Model:     model,
		MaxTokens: 2000,
		Messages: []types.Message{
			{
				Role:    "user",
				Content: prompt,
			},
		},
		Temperature: 0.3,
	}

	jsonData, err := json.Marshal(anthropicRequest)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal request: %v", err)
	}

	req, err := http.NewRequest("POST", "https://api.anthropic.com/v1/messages", bytes.NewBuffer(jsonData))
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %v", err)
	}

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("x-api-key", p.config.AnthropicAPIKey)
	req.Header.Set("anthropic-version", "2023-06-01")

	client := &http.Client{Timeout: 120 * time.Second}
	resp, err := client.Do(req)
	if err != nil {
		return nil, fmt.Errorf("failed to connect to Anthropic API: %v", err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %v", err)
	}

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("Anthropic API returned status %d: %s", resp.StatusCode, string(body))
	}

	var anthropicResp types.AnthropicResponse
	if err := json.Unmarshal(body, &anthropicResp); err != nil {
		return nil, fmt.Errorf("failed to parse response: %v", err)
	}

	if len(anthropicResp.Content) == 0 {
		return nil, fmt.Errorf("no content returned from Anthropic")
	}

	script := ""
	for _, content := range anthropicResp.Content {
		if content.Type == "text" {
			script += content.Text
		}
	}

	script = strings.TrimSpace(script)
	cleanedScript := cleanScript(script)

	return &types.ScriptResponse{
		Script:          cleanedScript,
		Model:           model,
		Provider:        request.Provider,
		TaskDescription: request.TaskDescription,
		ScriptType:      request.ScriptType,
	}, nil
}

// GetAvailableModels returns the list of available Anthropic models
func (p *AnthropicProvider) GetAvailableModels() []string {
	return []string{
		"claude-3-5-sonnet-20241022",
		"claude-3-5-haiku-20241022",
		"claude-3-opus-20240229",
		"claude-3-sonnet-20240229",
		"claude-3-haiku-20240307",
	}
}

// getDefaultModel returns the default model for Anthropic
func (p *AnthropicProvider) getDefaultModel() string {
	// Claude-3-5-haiku offers good performance at lower cost
	return "claude-3-5-haiku-20241022"
}
