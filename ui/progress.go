package ui

import (
	"fmt"
	"strings"
	"sync"
	"time"

	"please/types"
)

// ProgressIndicator handles displaying progress messages with animated dots
type ProgressIndicator struct {
	message   string
	isRunning bool
	stopChan  chan bool
	mu        sync.Mutex
}

// NewProgressIndicator creates a new progress indicator with the given message
func NewProgressIndicator(message string) *ProgressIndicator {
	return &ProgressIndicator{
		message:   message,
		isRunning: false,
		stopChan:  make(chan bool),
	}
}

// Start begins displaying the progress indicator
func (p *ProgressIndicator) Start() {
	p.mu.Lock()
	defer p.mu.Unlock()
	
	if p.isRunning {
		return // Already running
	}
	
	p.isRunning = true
	
	go func() {
		dots := 0
		for {
			select {
			case <-p.stopChan:
				// Clear the line and return
				fmt.Print("\r" + strings.Repeat(" ", len(p.message)+10) + "\r")
				return
			default:
				// Display message with animated dots
				dotStr := strings.Repeat(".", dots%4)
				spaces := strings.Repeat(" ", 3-len(dotStr))
				fmt.Printf("\r%s%s%s%s%s", ColorCyan, p.message, dotStr, spaces, ColorReset)
				
				dots++
				time.Sleep(500 * time.Millisecond)
			}
		}
	}()
}

// Stop stops the progress indicator
func (p *ProgressIndicator) Stop() {
	p.mu.Lock()
	defer p.mu.Unlock()
	
	if !p.isRunning {
		return
	}
	
	p.isRunning = false
	p.stopChan <- true
	
	// Clear the line
	fmt.Print("\r" + strings.Repeat(" ", len(p.message)+10) + "\r")
}

// UpdateStatus updates the progress message
func (p *ProgressIndicator) UpdateStatus(message string) {
	p.mu.Lock()
	defer p.mu.Unlock()
	
	p.message = message
}

// GetProviderStatusMessage returns an appropriate status message for the provider
func GetProviderStatusMessage(provider string) string {
	switch strings.ToLower(provider) {
	case "ollama":
		return "🤖 Connecting to Ollama (this may take a moment to start up)..."
	case "openai":
		return "🤖 Connecting to OpenAI..."
	case "anthropic":
		return "🤖 Connecting to Anthropic..."
	default:
		return "🤖 Connecting to AI provider..."
	}
}

// GetScriptGenerationProgressMessages returns sequential progress messages for script generation
func GetScriptGenerationProgressMessages(config *types.Config) []string {
	provider := strings.ToLower(config.Provider)
	model := config.PreferredModel
	
	messages := []string{
		GetProviderStatusMessage(provider),
		fmt.Sprintf("🧠 Initializing %s model: %s", provider, model),
		"📝 Generating script based on your request",
		"🔧 Optimizing script for your platform",
		"✨ Finalizing script generation",
	}
	
	// Add provider-specific messages
	if provider == "ollama" {
		messages = append([]string{
			"🔍 Checking if Ollama is running locally",
		}, messages...)
		
		messages = append(messages, "📦 Loading model into memory (first time may be slower)")
	}
	
	return messages
}

// GetAutoFixProgressMessages returns progress messages for auto-fix operations
func GetAutoFixProgressMessages(originalScript, errorMessage, provider string) []string {
	messages := []string{
		"🔍 Analyzing script error",
		GetProviderStatusMessage(provider),
		"🧠 Understanding the issue",
		"🛠️ Generating corrected script",
		"✅ Applying fixes and optimizations",
	}
	
	// Add context-specific messages
	if strings.Contains(errorMessage, "syntax") {
		messages = append(messages, "📝 Fixing syntax errors")
	}
	
	if strings.Contains(errorMessage, "permission") || strings.Contains(errorMessage, "access") {
		messages = append(messages, "🔒 Addressing permission issues")
	}
	
	if len(originalScript) > 500 {
		messages = append(messages, "📄 Processing large script content")
	}
	
	return messages
}

// ShowProgressWithSteps displays a series of progress messages with delays
func ShowProgressWithSteps(messages []string) {
	for i, message := range messages {
		if i > 0 {
			time.Sleep(800 * time.Millisecond) // Brief pause between steps
		}
		
		progress := NewProgressIndicator(message)
		progress.Start()
		
		// Show each step for a moment
		time.Sleep(1200 * time.Millisecond)
		progress.Stop()
		
		// Show completion for this step
		fmt.Printf("%s✓ %s%s\n", ColorGreen, message, ColorReset)
	}
}

// ShowProviderProgress displays provider-specific progress indication
func ShowProviderProgress(provider, operation string) func() {
	message := fmt.Sprintf("🤖 %s using %s", operation, provider)
	
	// Add provider-specific context
	switch strings.ToLower(provider) {
	case "ollama":
		message += " (may take a moment if starting up)"
	case "openai":
		message += " (via OpenAI API)"
	case "anthropic":
		message += " (via Anthropic API)"
	}
	
	progress := NewProgressIndicator(message)
	progress.Start()
	
	// Return a function to stop the progress
	return func() {
		progress.Stop()
		fmt.Printf("%s✓ %s completed%s\n", ColorGreen, operation, ColorReset)
	}
}

// ShowSimpleProgress shows a simple progress indicator with message
func ShowSimpleProgress(message string) func() {
	progress := NewProgressIndicator(message)
	progress.Start()
	
	return func() {
		progress.Stop()
	}
}
