package providers

import (
	"oohlama/types"
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
