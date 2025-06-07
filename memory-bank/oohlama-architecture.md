# OohLama - Technical Architecture & Implementation

## Current Architecture Overview

### Core Components
- **Multi-Provider AI Support**: Ollama, OpenAI, Anthropic, Custom providers
- **Cross-Platform Script Generation**: PowerShell (Windows), Bash (Linux/macOS)
- **Smart Model Selection**: Automatic best-model selection based on task analysis
- **Configuration Management**: JSON config files in platform-appropriate locations
- **Enhanced User Interface**: Colorful, interactive CLI with animations

## File Structure
```
oohlama/
├── main.go                           # Main application (1000+ lines)
├── oohlama.exe                       # Windows executable
├── oohlama                          # Linux executable
├── ol.bat                           # Windows alias wrapper
├── README.md                        # Comprehensive documentation
├── go.mod                           # Go module definition
├── .gitignore                       # Git ignore rules
└── memory-bank/
    ├── oohlama-v2-enhancements.md   # Recent enhancement documentation
    └── oohlama-architecture.md      # This file
```

## Key Data Structures

### Configuration System
```go
type Config struct {
    PreferredModel   string                    // User's preferred model
    ModelOverrides   map[string]string         // Task-specific model overrides
    Provider         string                    // AI provider selection
    ScriptType       string                    // Script type preference
    OpenAIAPIKey     string                    // OpenAI credentials
    AnthropicAPIKey  string                    // Anthropic credentials
    OllamaURL        string                    // Ollama server URL
    CustomProviders  map[string]ProviderConfig // Custom provider configs
}
```

### Provider Support
- **Ollama**: Local AI with model auto-detection and ranking
- **OpenAI**: GPT-3.5-turbo, GPT-4 series
- **Anthropic**: Claude-3 series (haiku, sonnet, opus)
- **Custom**: OpenAI-compatible API support

## Platform-Specific Features

### Configuration Paths
- **Windows**: `%APPDATA%\oohlama\config.json`
- **macOS**: `~/Library/Application Support/oohlama/config.json`
- **Linux**: `~/.config/oohlama/config.json`

### Alias Installation
- **Windows**: Creates `ol.bat` wrapper in executable directory
- **Unix**: Symlinks to `/usr/local/bin/ol` or `~/.local/bin/ol`
- **Fallback**: Shell script creation if symlinks fail

### Clipboard Support
- **Windows**: `clip` command
- **macOS**: `pbcopy` command
- **Linux**: `xclip` or `xsel` (auto-detected)

## Smart Model Selection Algorithm

### Task Categorization
1. **Coding**: script, function, code, program keywords
2. **System Admin**: system, server, service, process keywords
3. **File Management**: file, folder, directory, copy keywords
4. **Network**: web, http, url, download keywords
5. **General**: Default fallback category

### Model Ranking System
```go
modelPriority := map[string]int{
    "codegemma":      100, // Specialized for code generation
    "codellama":      95,  // Code-focused model
    "deepseek-coder": 90,  // Another code-focused model
    "llama3.1":       85,  // Latest general model with good coding
    // ... additional models ranked by capability
}
```

### Selection Factors
1. **Task Type Match**: Boost score for code-specialized models on coding tasks
2. **Model Size**: Larger models get preference (better capability)
3. **Recency**: Recently modified models get slight boost
4. **User Preferences**: Configured overrides take precedence

## Color System Implementation

### ANSI Color Codes
- Basic colors: Red, Green, Yellow, Blue, Purple, Cyan, White
- Formatting: Bold, Dim, Reset
- Background colors: Support for colored backgrounds
- Rainbow palette: 7-color gradient for animated effects

### Visual Elements
- **ASCII Art Banner**: Multi-line logo with rainbow animation
- **Progress Indicators**: Color-coded status messages
- **Interactive Menus**: Emoji + color combinations
- **Syntax Highlighting**: Line numbers and basic formatting

## Command Line Interface

### Main Commands
- `oohlama "task"` - Generate script for task
- `ol "task"` - Short alias (after installation)
- `--install-alias` - Set up short alias
- `--uninstall-alias` - Remove short alias
- `--help` - Show colorful help
- `--version` - Display version info

### Environment Variables
- `OOHLAMA_PROVIDER` - Override AI provider
- `OOHLAMA_SCRIPT_TYPE` - Force script type
- `OLLAMA_URL` - Ollama server URL
- `OLLAMA_MODEL` - Force specific model
- `OPENAI_API_KEY` - OpenAI authentication
- `ANTHROPIC_API_KEY` - Anthropic authentication

## Error Handling & Safety

### Script Generation Safety
1. **Preview Always**: Scripts shown before execution
2. **User Confirmation**: Explicit approval required
3. **Detailed Explanations**: Analysis of script operations
4. **Platform Warnings**: Platform-specific execution notes

### Error Recovery
- **Provider Fallbacks**: Automatic failover between providers
- **Model Fallbacks**: Default models when auto-selection fails
- **Configuration Recovery**: Default config creation on corruption
- **Graceful Degradation**: Feature availability based on system capabilities

## Future Enhancement Opportunities
1. **Syntax Highlighting**: Full syntax highlighting for generated scripts
2. **Template System**: Pre-built script templates
3. **History Management**: Track and reuse previous generations
4. **Plugin Architecture**: Extensible provider system
5. **Web Interface**: Optional web UI for remote usage

## Deployment Notes
- **Single Binary**: No external dependencies at runtime
- **Cross-Compilation**: Supports Windows, Linux, macOS builds
- **Minimal Footprint**: Efficient resource usage
- **Quick Setup**: One-command installation of alias system
