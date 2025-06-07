# OohLama - Ollama PowerShell Script Generator

A command-line utility that uses Ollama to generate PowerShell scripts for any task you describe.

## Prerequisites

- [Ollama](https://ollama.ai/) must be installed and running locally
- A language model installed in Ollama (e.g., `llama3.2`)

## Usage

```bash
# Basic usage
oohlama.exe "list all files in the current directory"

# More complex tasks
oohlama.exe "create a backup of all .txt files to a backup folder"
oohlama.exe "find all large files over 100MB and show their sizes"
oohlama.exe "check disk space on all drives and alert if any are over 80% full"
```

### Interactive Interface (New!)

OohLama now provides a safe, interactive experience:

🤖 **Beautiful Display**: Scripts are shown with syntax highlighting and line numbers  
💡 **Smart Explanation**: Automatic analysis of what the script does  
⚠️ **Safety Warnings**: Clear warnings before execution  
🎯 **Multiple Options**: Choose how to handle the generated script

**Available Actions:**
- 📋 **Copy to Clipboard**: Copy script for pasting into PowerShell
- ▶️ **Execute Now**: Run the script immediately with confirmation
- 💾 **Save to File**: Save as .ps1 file with header comments
- 📄 **Display Only**: Show raw script content
- ❌ **Cancel**: Exit without action

## Configuration

### Automatic Model Selection (New!)

OohLama now automatically detects and selects the most appropriate model for your task:

- **Intelligent Detection**: Automatically queries Ollama for available models
- **Task-Aware Selection**: Chooses models based on task type (coding, system admin, file management, etc.)
- **Smart Ranking**: Prioritizes code-focused models for script generation tasks
- **Persistent Preferences**: Stores configuration in `%APPDATA%\oohlama\config.json`

### Manual Configuration

You can still override the automatic selection using environment variables:

- `OLLAMA_URL`: Ollama server URL (default: `http://localhost:11434`)
- `OLLAMA_MODEL`: Force a specific model (overrides automatic selection)

Example:
```bash
set OLLAMA_MODEL=codellama
set OLLAMA_URL=http://localhost:11434
oohlama.exe "create a function to parse CSV files"
```

### Model Selection Logic

The system automatically ranks models by:
1. **Code Specialization**: Prefers models like `deepseek-coder`, `codellama`, `codegemma`
2. **Task Type**: Boosts code-focused models for script generation tasks
3. **Model Size**: Larger models generally perform better
4. **Recency**: Newer models are preferred

Supported model families: CodeGemma, CodeLlama, DeepSeek-Coder, Llama 3.x, Qwen2.5-Coder, Phi3, Mistral, Gemma2

## Output

The utility outputs a complete PowerShell script that you can:

1. Copy and paste into PowerShell
2. Save to a `.ps1` file and execute
3. Pipe directly to PowerShell: `oohlama.exe "task description" | powershell -`

## Examples

### Generate a file listing script:
```bash
oohlama.exe "list all files in the current directory with their sizes"
```

### Generate a system monitoring script:
```bash
oohlama.exe "check CPU and memory usage and display in a formatted table"
```

### Generate a file management script:
```bash
oohlama.exe "organize files by extension into separate folders"
```

## Building from Source

This project is written in Go. To build:

1. Install [Go](https://golang.org/dl/)
2. Clone this repository
3. Run: `go build -o oohlama.exe main.go`

## Learning Exercise Notes

This project demonstrates several Go concepts:
- HTTP client usage for REST API calls
- JSON marshaling and unmarshaling
- Command-line argument parsing
- Error handling and timeouts
- Environment variable access
- Cross-compilation to create standalone executables

The utility makes HTTP requests to the Ollama API to generate PowerShell scripts based on natural language descriptions.
