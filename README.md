# 🤖 OohLama - AI-Powered Cross-Platform Script Generator

**OohLama** is an intelligent command-line tool that automatically generates platform-specific scripts using multiple AI providers. Simply describe what you want to accomplish, and OohLama will create a ready-to-run script for your platform.

## ✨ Features

- **🌍 Cross-Platform**: Automatically generates PowerShell scripts on Windows, Bash scripts on Linux/macOS
- **🧠 Multiple AI Providers**: Support for Ollama, OpenAI, Anthropic, and custom providers
- **📋 Smart Model Selection**: Automatically chooses the best AI model for your task
- **🔄 Multiple Output Options**: Copy to clipboard, execute immediately, or save to file
- **🔍 Script Analysis**: Provides detailed explanations of what each script does
- **⚙️ Highly Configurable**: Supports provider preferences, API keys, and task-specific overrides
- **🛡️ Safety First**: Always shows the script before execution with clear warnings

## 🚀 Quick Start

### Option 1: Using Ollama (Local AI)

1. **Install and start Ollama**:
   ```bash
   # Install from https://ollama.ai/
   ollama serve
   ollama pull llama3.2  # or any preferred model
   ```

2. **Generate a script**:
   ```bash
   oohlama "list all files in the current directory"
   ```

### Option 2: Using OpenAI

1. **Set your API key**:
   ```bash
   # Windows
   set OPENAI_API_KEY=your_api_key_here
   set OOHLAMA_PROVIDER=openai
   
   # Linux/macOS
   export OPENAI_API_KEY=your_api_key_here
   export OOHLAMA_PROVIDER=openai
   ```

2. **Generate a script**:
   ```bash
   oohlama "create a backup script for important files"
   ```

### Option 3: Using Anthropic

1. **Set your API key**:
   ```bash
   # Windows
   set ANTHROPIC_API_KEY=your_api_key_here
   set OOHLAMA_PROVIDER=anthropic
   
   # Linux/macOS
   export ANTHROPIC_API_KEY=your_api_key_here
   export OOHLAMA_PROVIDER=anthropic
   ```

## 🖥️ Platform Support

### Windows
- **Script Type**: PowerShell (.ps1)
- **Execution**: Direct PowerShell execution
- **Clipboard**: Windows clip utility
- **Config Location**: `%APPDATA%\oohlama\config.json`

### Linux
- **Script Type**: Bash (.sh) 
- **Execution**: Bash shell execution
- **Clipboard**: xclip or xsel (auto-detected)
- **Config Location**: `~/.config/oohlama/config.json`

### macOS
- **Script Type**: Bash (.sh)
- **Execution**: Bash shell execution  
- **Clipboard**: pbcopy
- **Config Location**: `~/Library/Application Support/oohlama/config.json`

## 📖 Usage Examples

```bash
# File management (cross-platform)
oohlama "copy all .txt files to a backup folder"

# System information  
oohlama "show system memory usage"

# Network operations
oohlama "download a file from a URL and verify checksum"

# Process management
oohlama "find and kill processes using too much CPU"

# Development tasks
oohlama "create a git pre-commit hook script"
```

## ⚙️ Configuration

OohLama automatically creates a configuration file to store your preferences:

### Configuration File Structure

```json
{
  "provider": "ollama",
  "script_type": "auto",
  "ollama_url": "http://localhost:11434",
  "openai_api_key": "",
  "anthropic_api_key": "",
  "preferred_model": "",
  "model_overrides": {
    "coding": "deepseek-coder",
    "sysadmin": "llama3.1"
  },
  "custom_providers": {
    "my_provider": {
      "url": "https://api.example.com/v1/completions",
      "api_key": "your_key",
      "headers": {
        "Custom-Header": "value"
      },
      "model": "custom-model"
    }
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `OOHLAMA_PROVIDER` | AI provider to use | `ollama` |
| `OOHLAMA_SCRIPT_TYPE` | Force script type (`powershell`, `bash`, `auto`) | `auto` |
| `OLLAMA_URL` | Ollama server URL | `http://localhost:11434` |
| `OLLAMA_MODEL` | Force specific Ollama model | (auto-selected) |
| `OPENAI_API_KEY` | OpenAI API key | |
| `ANTHROPIC_API_KEY` | Anthropic API key | |

## 🤖 AI Provider Support

### Ollama (Local AI)
- **Models**: Any Ollama-compatible model
- **Best for**: Privacy, offline use, custom models
- **Setup**: Install Ollama and pull desired models
- **Cost**: Free (local compute)

### OpenAI
- **Models**: GPT-3.5-turbo, GPT-4, GPT-4-turbo
- **Best for**: High-quality output, latest capabilities
- **Setup**: Get API key from OpenAI platform
- **Cost**: Pay-per-use

### Anthropic Claude
- **Models**: Claude-3-haiku, Claude-3-sonnet, Claude-3-opus
- **Best for**: Balanced performance and safety
- **Setup**: Get API key from Anthropic console
- **Cost**: Pay-per-use

### Custom Providers
- **Setup**: Configure in config file or via API
- **Flexibility**: Support any OpenAI-compatible API
- **Examples**: Azure OpenAI, local LLM servers, other cloud providers

## 🔧 Advanced Features

### Smart Model Selection

OohLama intelligently selects models based on:

1. **Task Analysis**: Categorizes requests (coding, system admin, file management, etc.)
2. **Provider Capabilities**: Matches task requirements to model strengths
3. **Availability**: Checks what models are available
4. **User Preferences**: Respects configured overrides

### Script Type Detection

- **Automatic**: Detects platform and generates appropriate scripts
- **Override**: Force specific script types via config or environment
- **Cross-compilation**: Generate scripts for different platforms

### Enhanced User Interface

```
╔══════════════════════════════════════════════════════════════════════════════╗
║                           🤖 OohLama Script Generator                        ║
╚══════════════════════════════════════════════════════════════════════════════╝

📝 Task: create a simple script to show current time
🧠 Model: deepseek-coder:6.7b (ollama)  
🖥️  Platform: windows (powershell script)
📅 Generated: 2025-06-07 16:48:57

╔══════════════════════════════════════════════════════════════════════════════╗
║                              📋 Generated Script                             ║
╚══════════════════════════════════════════════════════════════════════════════╝

  1│ # Get current date and time
  2│ try {
  3│     $currentTime = Get-Date -Format "HH:mm:ss"
  4│     Write-Output $currentTime
  5│ } catch {
  6│     Write-Error "Failed to get the current time. Error: $_"
  7│ }
```

## 🛡️ Safety Features

- **Script Preview**: Always shows the generated script before execution
- **Platform Awareness**: Scripts use platform-appropriate commands and syntax
- **Detailed Analysis**: Explains what the script does and potential risks  
- **User Confirmation**: Requires explicit approval before running scripts
- **Error Handling**: Generated scripts include appropriate error handling
- **Secure Execution**: Scripts run in controlled environment

## 🔨 Building from Source

### Prerequisites
- Go 1.19 or later

### Build Commands

```bash
# Clone repository
git clone <repository-url>
cd oohlama

# Build for current platform
go build -o oohlama main.go

# Cross-compile for different platforms
# Windows
GOOS=windows GOARCH=amd64 go build -o oohlama.exe main.go

# Linux  
GOOS=linux GOARCH=amd64 go build -o oohlama main.go

# macOS
GOOS=darwin GOARCH=amd64 go build -o oohlama main.go
```

## 📋 Requirements

### Runtime Requirements
- **No dependencies** - single binary executable
- **AI Provider Access**: At least one of:
  - Ollama with installed models (local)
  - OpenAI API key (cloud)
  - Anthropic API key (cloud)
  - Custom provider access

### Platform-Specific
- **Windows**: PowerShell (usually pre-installed)
- **Linux**: Bash shell, optional xclip/xsel for clipboard
- **macOS**: Bash shell (built-in)

## 🎯 Use Cases

### System Administration
```bash
oohlama "create a script to monitor disk space and send alerts"
oohlama "automate log rotation for application logs"
oohlama "create a system health check script"
```

### Development
```bash
oohlama "create a git hook to run tests before commit"
oohlama "generate a script to build and deploy my application"
oohlama "create a development environment setup script"
```

### File Management
```bash
oohlama "organize photos by date taken"
oohlama "create incremental backup script"
oohlama "find and remove duplicate files"
```

### Automation
```bash
oohlama "schedule automated database backups"
oohlama "create a script to update all git repositories"
oohlama "automate certificate renewal process"
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests if applicable
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Development Setup

```bash
# Install development dependencies
go mod tidy

# Run tests
go test ./...

# Run with development flags
go run main.go "test task description"
```

## 📄 License

This project is open source. Feel free to use, modify, and distribute as needed.

## 🆘 Troubleshooting

### Common Issues

**"No models available in Ollama"**
- Ensure Ollama is running: `ollama serve`
- Install a model: `ollama pull llama3.2`

**"OpenAI API key not configured"**
- Set environment variable: `export OPENAI_API_KEY=your_key`
- Or configure in config file

**"Clipboard not supported"**
- Linux: Install `xclip` or `xsel`
- The script will still be displayed for manual copying

**"Permission denied executing script"**
- Linux/macOS: `chmod +x generated_script.sh`
- Windows: Run PowerShell as administrator if needed

### Getting Help

- Check the configuration file location for your platform
- Verify AI provider connectivity
- Review generated scripts before execution
- Use environment variables to override defaults

---

**⚠️ Important Security Notice**: Always review generated scripts before execution. While OohLama creates safe, well-structured scripts following best practices, you should understand what any script does before running it on your system. Different AI providers may generate different approaches to the same task.
