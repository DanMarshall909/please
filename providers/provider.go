package providers

import (
	"please/types"
)

// Provider defines the interface for AI providers
type Provider interface {
	// GenerateScript generates a script using the provider's AI service
	GenerateScript(request *types.ScriptRequest) (*types.ScriptResponse, error)

	// Name returns the name of the provider
	Name() string

	// IsConfigured returns true if the provider is properly configured
	IsConfigured(config *types.Config) bool
}

// CreatePrompt creates the appropriate prompt based on script type and task
func CreatePrompt(taskDescription, scriptType string) string {
	if scriptType == "bash" {
		return `You are a Bash scripting expert. Generate a complete, working Bash script to accomplish the following task:

` + taskDescription + `

Requirements:
- ***CRITICAL*** ANY POTENTIALLY DANGEROUS OR UNKNOWN COMMANDS SHOULD BE HIGHLIGHTED WITH COMMENTS EXPLAINING WHY THEY ARE DANGEROUS TO ENSURE THAT THEY ARE CHECKED BY THE USER BEFORE RUNNING
- Write clean, well-commented Bash code
- Include error handling where appropriate
- Use Bash best practices
- Include proper shebang (#!/bin/bash)
- Do NOT include markdown code blocks, backticks, or formatting
- Do NOT include explanations or descriptions
- Return ONLY the raw Bash script code
- The script should be ready to run as-is
- Start directly with the shebang and Bash commands

Bash Script:`
	} else {
		return `You are a PowerShell expert. Generate a complete, working PowerShell script to accomplish the following task:

` + taskDescription + `

Requirements:
- ***CRITICAL*** ANY POTENTIALLY DANGEROUS OR UNKNOWN COMMANDS SHOULD BE HIGHLIGHTED WITH COMMENTS EXPLAINING WHY THEY ARE DANGEROUS TO ENSURE THAT THEY ARE CHECKED BY THE USER BEFORE RUNNING
- Write clean, well-commented PowerShell code
- Include error handling where appropriate
- Use PowerShell best practices
- Do NOT include markdown code blocks, backticks, or formatting
- Do NOT include explanations or descriptions
- Return ONLY the raw PowerShell script code
- The script should be ready to run as-is
- Start directly with PowerShell commands, no preamble

PowerShell Script:`
	}
}

// GenerateFixedScript generates a fixed script using the provider's AI service, given the original script and error message
func GenerateFixedScript(originalScript, errorMessage, scriptType, model, provider string, config *types.Config) (string, error) {
	// Compose a prompt for the LLM to fix the script based on the error
	prompt := "The following script failed with this error:\n\nScript:\n" + originalScript + "\n\nError:\n" + errorMessage + "\n\nPlease suggest a corrected version of the script. Return ONLY the fixed script, no explanations or markdown formatting."

	request := &types.ScriptRequest{
		TaskDescription: prompt,
		ScriptType:      scriptType,
		Provider:        provider,
		Model:           model,
	}

	// Select provider (for now, only OpenAI is implemented here)
	openai := NewOpenAIProvider(config)
	resp, err := openai.GenerateScript(request)
	if err != nil {
		return "", err
	}
	return resp.Script, nil
}
