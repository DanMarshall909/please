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

// OpenAIProvider implements the Provider interface for OpenAI
type OpenAIProvider struct {
	config *types.Config
}

// NewOpenAIProvider creates a new OpenAI provider
func NewOpenAIProvider(config *types.Config) *OpenAIProvider {
	return &OpenAIProvider{config: config}
}

// Name returns the provider name
func (p *OpenAIProvider) Name() string {
	return "openai"
}

// IsConfigured checks if OpenAI is properly configured
func (p *OpenAIProvider) IsConfigured(config *types.Config) bool {
	return config.OpenAIAPIKey != ""
}

// GenerateScript generates a script using OpenAI's API
func (p *OpenAIProvider) GenerateScript(request *types.ScriptRequest) (*types.ScriptResponse, error) {
	if !p.IsConfigured(p.config) {
		return nil, fmt.Errorf("OpenAI API key not configured. Please set OPENAI_API_KEY environment variable or use 'please set openai key'")
	}

	prompt := CreatePrompt(request.TaskDescription, request.ScriptType)
	
	// Determine the model to use
	model := request.Model
	if model == "" {
		model = p.getDefaultModel()
	}

	openaiRequest := types.OpenAIRequest{
		Model: model,
		Messages: []types.Message{
			{
				Role:    "user",
				Content: prompt,
			},
		},
		Temperature: 0.3,
		MaxTokens:   2000,
	}

	jsonData, err := json.Marshal(openaiRequest)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal request: %v", err)
	}

	req, err := http.NewRequest("POST", "https://api.openai.com/v1/chat/completions", bytes.NewBuffer(jsonData))
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %v", err)
	}

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", "Bearer "+p.config.OpenAIAPIKey)

	client := &http.Client{Timeout: 120 * time.Second}
	resp, err := client.Do(req)
	if err != nil {
		return nil, fmt.Errorf("failed to connect to OpenAI API: %v", err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %v", err)
	}

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("OpenAI API returned status %d: %s", resp.StatusCode, string(body))
	}

	var openaiResp types.OpenAIResponse
	if err := json.Unmarshal(body, &openaiResp); err != nil {
		return nil, fmt.Errorf("failed to parse response: %v", err)
	}

	if len(openaiResp.Choices) == 0 {
		return nil, fmt.Errorf("no response choices returned from OpenAI")
	}

	script := strings.TrimSpace(openaiResp.Choices[0].Message.Content)
	cleanedScript := cleanScript(script)

	return &types.ScriptResponse{
		Script:          cleanedScript,
		Model:           model,
		Provider:        request.Provider,
		TaskDescription: request.TaskDescription,
		ScriptType:      request.ScriptType,
	}, nil
}

// GetAvailableModels returns the list of available OpenAI models
func (p *OpenAIProvider) GetAvailableModels() []string {
	return []string{
		"gpt-4",
		"gpt-4-turbo",
		"gpt-4-turbo-preview", 
		"gpt-3.5-turbo",
		"gpt-3.5-turbo-16k",
	}
}

// getDefaultModel returns the default model for OpenAI
func (p *OpenAIProvider) getDefaultModel() string {
	// GPT-3.5-turbo is more cost-effective for script generation
	return "gpt-3.5-turbo"
}
