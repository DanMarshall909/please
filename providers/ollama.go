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

// OllamaProvider implements the Provider interface for Ollama
type OllamaProvider struct {
	config *types.Config
}

// NewOllamaProvider creates a new Ollama provider
func NewOllamaProvider(config *types.Config) *OllamaProvider {
	return &OllamaProvider{config: config}
}

// Name returns the provider name
func (p *OllamaProvider) Name() string {
	return "ollama"
}

// IsConfigured checks if Ollama is properly configured
func (p *OllamaProvider) IsConfigured(config *types.Config) bool {
	// Ollama doesn't require API keys, just needs to be running
	// We could check if the server is reachable, but that's done during generation
	return true
}

// GenerateScript generates a script using Ollama
func (p *OllamaProvider) GenerateScript(request *types.ScriptRequest) (*types.ScriptResponse, error) {
	baseURL := p.config.OllamaURL
	if baseURL == "" {
		baseURL = "http://localhost:11434"
	}

	prompt := CreatePrompt(request.TaskDescription, request.ScriptType)

	ollamaRequest := types.OllamaRequest{
		Model:  request.Model,
		Prompt: prompt,
		Stream: false,
		Options: map[string]interface{}{
			"temperature": 0.3,
			"top_p":       0.9,
		},
	}

	jsonData, err := json.Marshal(ollamaRequest)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal request: %v", err)
	}

	client := &http.Client{Timeout: 120 * time.Second}
	resp, err := client.Post(baseURL+"/api/generate", "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		return nil, fmt.Errorf("failed to connect to Ollama at %s: %v\nMake sure Ollama is running", baseURL, err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("Ollama API returned status %d: %s", resp.StatusCode, string(body))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %v", err)
	}

	var ollamaResp types.OllamaResponse
	if err := json.Unmarshal(body, &ollamaResp); err != nil {
		return nil, fmt.Errorf("failed to parse response: %v", err)
	}

	script := strings.TrimSpace(ollamaResp.Response)
	cleanedScript := cleanScript(script)

	return &types.ScriptResponse{
		Script:          cleanedScript,
		Model:           request.Model,
		Provider:        request.Provider,
		TaskDescription: request.TaskDescription,
		ScriptType:      request.ScriptType,
	}, nil
}

// GetAvailableModels queries Ollama for all available models
func (p *OllamaProvider) GetAvailableModels() ([]types.ModelInfo, error) {
	baseURL := p.config.OllamaURL
	if baseURL == "" {
		baseURL = "http://localhost:11434"
	}

	client := &http.Client{Timeout: 10 * time.Second}

	resp, err := client.Get(baseURL + "/api/tags")
	if err != nil {
		return nil, fmt.Errorf("failed to connect to Ollama: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("Ollama API returned status %d: %s", resp.StatusCode, string(body))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %v", err)
	}

	var modelsResp types.ModelsResponse
	if err := json.Unmarshal(body, &modelsResp); err != nil {
		return nil, fmt.Errorf("failed to parse models response: %v", err)
	}

	return modelsResp.Models, nil
}

// cleanScript removes markdown formatting and other unwanted content from the generated script
func cleanScript(script string) string {
	lines := strings.Split(script, "\n")
	cleanedLines := []string{}

	for _, line := range lines {
		// Skip markdown code block markers
		if strings.HasPrefix(strings.TrimSpace(line), "```") {
			continue
		}

		// Skip empty explanatory lines that might have been added
		trimmed := strings.TrimSpace(line)
		if trimmed == "" ||
			strings.HasPrefix(trimmed, "Here's a PowerShell script") ||
			strings.HasPrefix(trimmed, "Here's a Bash script") ||
			strings.HasPrefix(trimmed, "This script will") ||
			strings.HasPrefix(trimmed, "The following script") {
			continue
		}

		cleanedLines = append(cleanedLines, line)
	}

	return strings.Join(cleanedLines, "\n")
}
