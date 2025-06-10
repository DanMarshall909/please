package script

import (
	"strings"
	"testing"

	"please/types"
)

// TestRefineScript_WithUserInput tests script refinement with user feedback
func TestRefineScript_WithUserInput(t *testing.T) {
	// Mock provider for testing
	mockProvider := &MockProvider{
		GenerateScriptFunc: func(req *types.ScriptRequest) (*types.ScriptResponse, error) {
			// Simulate refinement request
			if strings.Contains(req.TaskDescription, "make it faster") {
				return &types.ScriptResponse{
					Script:          "echo 'Fast version'",
					Provider:        req.Provider,
					Model:          req.Model,
					ScriptType:     req.ScriptType,
					TaskDescription: req.TaskDescription,
				}, nil
			}
			return &types.ScriptResponse{
				Script:          "echo 'Original version'",
				Provider:        req.Provider,
				Model:          req.Model,
				ScriptType:     req.ScriptType,
				TaskDescription: req.TaskDescription,
			}, nil
		},
	}

	originalResponse := &types.ScriptResponse{
		Script:          "echo 'Original script'",
		Provider:        "ollama",
		Model:          "llama3.2",
		ScriptType:     "bash",
		TaskDescription: "create echo script",
	}

	refinementRequest := "make it faster"
	config := &types.Config{Provider: "ollama", PreferredModel: "llama3.2"}

	refinedResponse, err := RefineScriptWithProvider(
		mockProvider,
		originalResponse,
		refinementRequest,
		config,
	)

	if err != nil {
		t.Errorf("RefineScript failed: %v", err)
	}

	if refinedResponse == nil {
		t.Fatal("RefineScript should return a response")
	}

	// Check that the script was refined
	if !strings.Contains(refinedResponse.Script, "Fast version") {
		t.Errorf("Expected refined script to contain 'Fast version', got: %s", refinedResponse.Script)
	}

	// Check that task description includes refinement context
	if !strings.Contains(refinedResponse.TaskDescription, "make it faster") {
		t.Error("Refined task description should include refinement request")
	}
}

// TestRefineScript_ProperPrompt tests that refinement creates appropriate prompts
func TestRefineScript_ProperPrompt(t *testing.T) {
	var receivedRequest *types.ScriptRequest

	mockProvider := &MockProvider{
		GenerateScriptFunc: func(req *types.ScriptRequest) (*types.ScriptResponse, error) {
			receivedRequest = req
			return &types.ScriptResponse{
				Script:          "echo 'Refined script'",
				Provider:        req.Provider,
				Model:          req.Model,
				ScriptType:     req.ScriptType,
				TaskDescription: req.TaskDescription,
			}, nil
		},
	}

	originalResponse := &types.ScriptResponse{
		Script:          "echo 'Original script'",
		Provider:        "ollama",
		Model:          "llama3.2",
		ScriptType:     "bash",
		TaskDescription: "create echo script",
	}

	refinementRequest := "add error handling"
	config := &types.Config{Provider: "ollama", PreferredModel: "llama3.2"}

	_, err := RefineScriptWithProvider(
		mockProvider,
		originalResponse,
		refinementRequest,
		config,
	)

	if err != nil {
		t.Errorf("RefineScript failed: %v", err)
	}

	if receivedRequest == nil {
		t.Fatal("Mock provider should have received a request")
	}

	// Check that the prompt includes original script and refinement request
	prompt := receivedRequest.TaskDescription
	if !strings.Contains(prompt, originalResponse.Script) {
		t.Error("Prompt should contain original script")
	}

	if !strings.Contains(prompt, refinementRequest) {
		t.Error("Prompt should contain refinement request")
	}

	if !strings.Contains(prompt, "refine") || !strings.Contains(prompt, "improve") {
		t.Error("Prompt should mention refinement/improvement")
	}
}

// TestGetRefinementPromptSuggestions tests that we provide helpful prompt suggestions
func TestGetRefinementPromptSuggestions(t *testing.T) {
	suggestions := GetRefinementPromptSuggestions()

	if len(suggestions) == 0 {
		t.Error("Should return refinement prompt suggestions")
	}

	// Check for common refinement types
	foundSafety := false
	foundPerformance := false
	foundError := false

	for _, suggestion := range suggestions {
		if strings.Contains(strings.ToLower(suggestion), "safe") || strings.Contains(strings.ToLower(suggestion), "secure") {
			foundSafety = true
		}
		if strings.Contains(strings.ToLower(suggestion), "fast") || strings.Contains(strings.ToLower(suggestion), "performance") {
			foundPerformance = true
		}
		if strings.Contains(strings.ToLower(suggestion), "error") || strings.Contains(strings.ToLower(suggestion), "handle") {
			foundError = true
		}
	}

	if !foundSafety {
		t.Error("Suggestions should include safety/security options")
	}
	if !foundPerformance {
		t.Error("Suggestions should include performance options")
	}
	if !foundError {
		t.Error("Suggestions should include error handling options")
	}
}

// TestBuildRefinementPrompt tests the prompt building for refinement
func TestBuildRefinementPrompt(t *testing.T) {
	originalScript := "echo 'Hello World'"
	refinementRequest := "add timestamp"
	scriptType := "bash"

	prompt := BuildRefinementPrompt(originalScript, refinementRequest, scriptType)

	// Check that prompt contains all necessary elements
	if !strings.Contains(prompt, originalScript) {
		t.Error("Prompt should contain original script")
	}

	if !strings.Contains(prompt, refinementRequest) {
		t.Error("Prompt should contain refinement request")
	}

	if !strings.Contains(prompt, "refine") || !strings.Contains(prompt, "improve") {
		t.Error("Prompt should mention refinement")
	}

	// Check script type specific instructions
	if scriptType == "bash" && !strings.Contains(prompt, "Bash") {
		t.Error("Bash refinement should mention Bash in prompt")
	}
}

// RefineScriptWithProvider is the function we need to implement for script refinement
func RefineScriptWithProvider(
	provider Provider,
	originalResponse *types.ScriptResponse,
	refinementRequest string,
	config *types.Config,
) (*types.ScriptResponse, error) {
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
