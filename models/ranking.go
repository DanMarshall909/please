package models

import (
	"strings"
	"time"

	"oohlama/types"
)

// RankModels selects the best model based on task type and model capabilities
func RankModels(models []types.ModelInfo, taskDescription, taskType string) string {
	// Model preference ranking based on script generation capability
	modelPriority := map[string]int{
		"codegemma":      100, // Specialized for code generation
		"codellama":      95,  // Code-focused model
		"deepseek-coder": 90,  // Another code-focused model
		"llama3.1":       85,  // Latest general model with good coding
		"llama3.2":       80,  // Good general model
		"llama3":         75,  // Older but reliable
		"qwen2.5-coder":  85,  // Code-focused
		"phi3":           70,  // Smaller but capable
		"mistral":        65,  // Good general model
		"gemma2":         60,  // Decent general model
	}

	// Boost priority for code-related tasks
	if taskType == "coding" {
		for name := range modelPriority {
			if strings.Contains(strings.ToLower(name), "code") {
				modelPriority[name] += 20
			}
		}
	}

	bestModel := ""
	bestScore := -1

	for _, model := range models {
		score := 0
		modelName := strings.ToLower(model.Name)

		// Check for exact matches in our priority list
		for priorityModel, priorityScore := range modelPriority {
			if strings.Contains(modelName, priorityModel) {
				score = priorityScore
				break
			}
		}

		// If no specific match, give a base score
		if score == 0 {
			score = 50
		}

		// Prefer larger models (generally more capable)
		if model.Size > 7000000000 { // > 7GB
			score += 10
		} else if model.Size > 4000000000 { // > 4GB
			score += 5
		}

		// Prefer newer models (more recent modified date)
		if time.Since(model.ModifiedAt) < 30*24*time.Hour { // Less than 30 days old
			score += 5
		}

		if score > bestScore {
			bestScore = score
			bestModel = model.Name
		}
	}

	// Fallback if no model found
	if bestModel == "" && len(models) > 0 {
		bestModel = models[0].Name
	}

	return bestModel
}
