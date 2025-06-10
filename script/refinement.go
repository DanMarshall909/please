package script

import (
	"fmt"
	"strings"

	"please/providers"
	"please/types"
)

// RefineScript refines an existing script based on user feedback using the configured provider
func RefineScript(
	originalResponse *types.ScriptResponse,
	refinementRequest string,
	config *types.Config,
) (*types.ScriptResponse, error) {
	// Create the appropriate provider
	var provider providers.Provider
	switch originalResponse.Provider {
	case "ollama":
		provider = providers.NewOllamaProvider(config)
	case "openai":
		provider = providers.NewOpenAIProvider(config)
	case "anthropic":
		provider = providers.NewAnthropicProvider(config)
	default:
		return nil, fmt.Errorf("unsupported provider: %s", originalResponse.Provider)
	}

	if !provider.IsConfigured(config) {
		return nil, fmt.Errorf("provider %s is not properly configured", originalResponse.Provider)
	}

	// Build refinement prompt
	prompt := BuildRefinementPrompt(
		originalResponse.Script,
		refinementRequest,
		originalResponse.ScriptType,
	)

	// Create request for refined script
	request := &types.ScriptRequest{
		TaskDescription: prompt,
		ScriptType:      originalResponse.ScriptType,
		Provider:        originalResponse.Provider,
		Model:           originalResponse.Model,
	}

	// Generate refined script
	response, err := provider.GenerateScript(request)
	if err != nil {
		return nil, err
	}

	// Update task description to include refinement context
	response.TaskDescription = originalResponse.TaskDescription + " [Refined: " + refinementRequest + "]"

	return response, nil
}

// BuildRefinementPrompt creates a prompt for script refinement
func BuildRefinementPrompt(originalScript, refinementRequest, scriptType string) string {
	scriptTypeCapitalized := strings.Title(scriptType)
	
	return "Please refine and improve the following " + scriptTypeCapitalized + " script based on this request: " + refinementRequest + "\n\n" +
		"Original Script:\n" + originalScript + "\n\n" +
		"Refinement Request: " + refinementRequest + "\n\n" +
		"Requirements:\n" +
		"- Keep the core functionality intact\n" +
		"- Apply the requested improvements/changes\n" +
		"- Maintain " + scriptTypeCapitalized + " best practices\n" +
		"- Include clear comments explaining changes\n" +
		"- Return ONLY the refined script, no explanations or markdown formatting\n\n" +
		"Refined " + scriptTypeCapitalized + " Script:"
}

// GetRefinementPromptSuggestions returns helpful refinement suggestions
func GetRefinementPromptSuggestions() []string {
	return []string{
		"Add error handling and validation",
		"Make it faster and more efficient", 
		"Add more detailed logging and output",
		"Make it safer and more secure",
		"Add command-line parameter support",
		"Improve readability with better comments",
		"Add progress indicators",
		"Make it more robust for edge cases",
		"Optimize for cross-platform compatibility",
		"Add confirmation prompts for dangerous operations",
	}
}
