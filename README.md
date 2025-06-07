# 🤖 Please - Your Overly Helpful Digital Assistant

**Please** is a politely silly AI-powered command-line tool that generates platform-specific scripts using natural language. Simply tell Please what you want to accomplish in plain English, and it will create a ready-to-run script for your platform with an overly helpful attitude!

## ✨ Current Status (v4.0+)

### 🎪 Complete Rebrand: From OohLama to Please ✅
- **🤖 New Personality**: Politely silly and overly helpful digital assistant
- **🗣️ Natural Language Interface**: No quotes needed! Just talk naturally
- **⚡ New Shortcuts**: `pls` and `please` commands (plus legacy `ol` support)
- **🎨 Fresh Look**: New ASCII art, colors, and personality throughout
- **📦 Environment Variables**: Updated from `OOHLAMA_*` to `PLEASE_*` (legacy still supported)

### 🗣️ Natural Language Interface ✅
```bash
# Natural language - just talk normally!
pls list all files older than 10 years
pls backup my documents folder  
pls find processes using too much memory

# Legacy quoted syntax still works
ol "create a backup script"
please "show system information"
```

## ✨ Core Features

### 🤖 Intelligent Script Generation ✅
- **🌍 Cross-Platform**: Automatically generates PowerShell scripts on Windows, Bash scripts on Linux/macOS
- **🧠 AI Provider Support**: Currently supports Ollama (OpenAI and Anthropic planned)
- **📋 Smart Model Selection**: Automatically chooses the best AI model for your task
- **⚙️ Configurable**: Supports provider preferences and task-specific settings

### 🎯 Interactive Experience ✅
- **🎯 Interactive Menu System**: Choose from multiple actions after script generation
- **📋 Copy to Clipboard**: Cross-platform clipboard integration (Windows clip, macOS pbcopy, Linux xclip/xsel)
- **▶️ Safe Execution**: Execute scripts with safety validation and risk-based warnings
- **💾 Smart File Saving**: Intelligent filename suggestions based on task description
- **📖 Detailed Analysis**: Script explanations with line counts and usage tips
- **🔄 Load Last Script**: Reload the previously generated script
- **🎮 Single-Key Navigation**: Quick menu navigation without pressing Enter

### 🛡️ Advanced Safety Features ✅
- **⚠️ Enhanced Safety Warnings**: Comprehensive warning system with severity levels:
  - ⛔ **Critical**: `rm -rf /`, `format c:`, filesystem destruction
  - 🔴 **High Risk**: `shutdown`, `chmod 777`, privilege escalation  
  - 🟡 **Medium Risk**: `rm -rf`, service management, recursive deletion
  - 🟢 **Info**: Missing error handling, incomplete scripts
- **✏️ Script Review**: Always shows scripts with line numbers before execution
- **🛡️ Advanced Validation**: Dangerous command detection with detailed explanations
- **🔒 Risk-Based Execution**: Different confirmation levels based on script risk:
  - 🟢 **Green (Safe)**: Execute immediately with brief message
  - 🟡 **Yellow (Caution)**: Single 'y' confirmation required
  - 🔴 **Red (High Risk)**: Must type 'EXECUTE' to proceed
- **🚑 Auto-Fix**: Attempts automatic script repair when execution fails

## 🚀 Quick Start

### Option 1: Using Ollama (Local AI - Recommended) ✅

1. **Install and start Ollama**:
   ```bash
   # Install from https://ollama.ai/
   ollama serve
   ollama pull llama3.2  # or any preferred model
   ```

2. **Generate a script naturally**:
   ```bash
   pls list all files in the current directory
   pls create a backup script for my documents
   ```

### Option 2: Using OpenAI ✅

1. **Set your API key**:
   ```bash
   # Windows
   set OPENAI_API_KEY=your_api_key_here
   set PLEASE_PROVIDER=openai
   
   # Linux/macOS
   export OPENAI_API_KEY=your_api_key_here
   export PLEASE_PROVIDER=openai
   ```

2. **Generate a script**:
   ```bash
   pls create a backup script for important files
   ```

### Option 3: Using Anthropic ✅

1. **Set your API key**:
   ```bash
   # Windows
   set ANTHROPIC_API_KEY=your_api_key_here
   set PLEASE_PROVIDER=anthropic
   
   # Linux/macOS
   export ANTHROPIC_API_KEY=your_api_key_here
   export PLEASE_PROVIDER=anthropic
   ```

2. **Generate a script**:
   ```bash
   pls create a backup script for important files
   ```

## 🖥️ Platform Support ✅

### Windows
- **Script Type**: PowerShell (.ps1)
- **Execution**: Direct PowerShell execution with `-ExecutionPolicy Bypass`
- **Clipboard**: Windows clip utility
- **Config Location**: `%APPDATA%\please\config.json`

### Linux
- **Script Type**: Bash (.sh) 
- **Execution**: Bash shell execution
- **Clipboard**: xclip or xsel (auto-detected)
- **Config Location**: `~/.config/please/config.json`

### macOS
- **Script Type**: Bash (.sh)
- **Execution**: Bash shell execution  
- **Clipboard**: pbcopy
- **Config Location**: `~/Library/Application Support/please/config.json`

## 📖 Natural Language Usage Examples

```bash
# File management (cross-platform)
pls copy all .txt files to a backup folder
pls find large files taking up space
pls organize my photos by date

# System information  
pls show system memory usage
pls check what processes are running
pls display disk space information

# Network operations
pls download a file from a URL and verify checksum
pls test if a website is reachable
pls show my network configuration

# Process management
pls find and kill processes using too much CPU
pls restart a specific service
pls monitor system performance

# Development tasks
pls create a git pre-commit hook script
pls set up a development environment
pls build and deploy my application
```

## ⚙️ Configuration ✅

Please automatically creates a configuration file to store your preferences:

### Configuration File Structure

```json
{
  "preferred_model": "",
  "model_overrides": {
    "coding": "deepseek-coder",
    "sysadmin": "llama3.1"
  },
  "provider": "ollama",
  "script_type": "auto",
  "openai_api_key": "",
  "anthropic_api_key": "",
  "ollama_url": "http://localhost:11434",
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
| `PLEASE_PROVIDER` | AI provider to use | `ollama` |
| `PLEASE_SCRIPT_TYPE` | Force script type (`powershell`, `bash`, `auto`) | `auto` |
| `OLLAMA_URL` | Ollama server URL | `http://localhost:11434` |
| `OLLAMA_MODEL` | Force specific Ollama model | (auto-selected) |
| `OPENAI_API_KEY` | OpenAI API key | |
| `ANTHROPIC_API_KEY` | Anthropic API key | |

### Legacy Environment Variables (Still Supported)
For backward compatibility, the old `OOHLAMA_*` environment variables still work but will show deprecation warnings.

## 🤖 AI Provider Support

### Ollama (Local AI) ✅ Fully Implemented
- **Models**: Any Ollama-compatible model (codegemma, codellama, llama3.1, deepseek-coder, etc.)
- **Best for**: Privacy, offline use, custom models
- **Setup**: Install Ollama and pull desired models
- **Cost**: Free (local compute)
- **Features**: Smart model selection, automatic ranking, task categorization

### OpenAI ✅ Fully Implemented
- **Models**: GPT-3.5-turbo, GPT-4, GPT-4-turbo, GPT-4-turbo-preview
- **Best for**: High-quality output, latest capabilities
- **Setup**: Get API key from OpenAI platform
- **Cost**: Pay-per-use
- **Features**: Automatic model selection, task-specific optimization

### Anthropic Claude ✅ Fully Implemented
- **Models**: Claude-3-5-sonnet, Claude-3-5-haiku, Claude-3-opus, Claude-3-sonnet, Claude-3-haiku
- **Best for**: Balanced performance and safety
- **Setup**: Get API key from Anthropic console
- **Cost**: Pay-per-use
- **Features**: Advanced reasoning, safety-focused responses

### Custom Providers 🚧 Planned
- **Setup**: Configure in config file
- **Flexibility**: Support any OpenAI-compatible API
- **Examples**: Azure OpenAI, local LLM servers, other cloud providers
- **Status**: Configuration structure exists, implementation pending

## 🔧 Advanced Features

### 🧠 Smart Model Selection ✅

Please intelligently selects models based on:

1. **Task Analysis**: Categorizes requests (coding, system admin, file management, etc.)
2. **Model Ranking**: Uses sophisticated ranking algorithm considering:
   - Model specialization (code-focused models get priority for coding tasks)
   - Model size (larger models preferred)
   - Recency (recently modified models get slight boost)
3. **Availability**: Checks what models are available via Ollama API
4. **User Preferences**: Respects configured overrides and preferences

### 🌍 Platform Detection ✅

- **Automatic**: Detects platform and generates appropriate scripts
- **Override**: Force specific script types via config or environment
- **Smart Defaults**: Windows→PowerShell, Linux/macOS→Bash

### 🎨 Enhanced User Interface ✅

```
╔══════════════════════════════════════════════════════════════════════════════╗
║                           🤖 Please Script Generator                         ║
╚══════════════════════════════════════════════════════════════════════════════╝

📝 Task: create a simple script to show current time
🧠 Model: deepseek-coder:6.7b (ollama)  
🖥️  Platform: powershell script

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

✅ Script generated successfully!

🎯 What would you like to do with this script?

  1. 📋 Copy to clipboard
  2. ▶️  Execute script now
  3. 💾 Save to file
  4. ✏️  Edit script
  5. 📖 Show detailed explanation
  6. 🔄 Load last script
  7. 🚪 Exit

Press 1-7: _
```

### 🎯 Interactive Menu System ✅

After generating a script, Please presents an interactive menu with these options:

- **📋 Copy to clipboard**: Cross-platform clipboard integration (Windows clip, macOS pbcopy, Linux xclip/xsel)
- **▶️ Execute script now**: Safe execution with validation warnings and risk-based confirmation
- **💾 Save to file**: Intelligent filename suggestions based on task description
- **✏️ Edit script**: Full editing capabilities with external and inline editing options
- **📖 Show detailed explanation**: Comprehensive analysis including:
  - Task analysis and AI model used
  - Script statistics (lines, comments, commands)
  - Platform-specific usage tips
  - Safety recommendations
- **🔄 Load last script**: Reload the previously generated script from local storage
- **🚪 Exit**: Clean program termination with a polite goodbye

The menu system supports single-key input and multiple actions on the same script.

### 📚 History System ✅ Partial Implementation

- **Last Script**: Automatically saves and can reload the last generated script
- **JSON Storage**: Simple JSON-based storage in config directory
- **🚧 Full History Browser**: Planned for future release

## 🛡️ Safety Features ✅

- **📖 Script Preview**: Always shows the generated script with line numbers before execution
- **🌍 Platform Awareness**: Scripts use platform-appropriate commands and syntax
- **🔍 Detailed Analysis**: Explains what the script does and potential risks  
- **✋ Risk-Based Confirmation**: Different confirmation levels based on detected dangers
- **🛡️ Error Handling**: Generated scripts include appropriate error handling
- **🔒 Secure Execution**: Scripts run via temporary files with proper cleanup
- **⚠️ Dangerous Command Detection**: Advanced pattern matching with context awareness
- **🚑 Auto-Fix**: Attempts automatic script repair when execution fails

## 🎯 Command Examples

### 📁 File Management
```bash
pls organize photos by date taken
pls create incremental backup script  
pls find and remove duplicate files
pls compress old log files
```

### 💻 System Administration  
```bash
pls monitor disk space and send alerts
pls automate log rotation for application logs
pls create a system health check script
pls restart services if they stop responding
```

### 🔧 Development
```bash
pls create a git hook to run tests before commit
pls generate a script to build and deploy my application
pls create a development environment setup script
pls automate database migrations
```

### 🌐 Network & Automation
```bash
pls schedule automated database backups
pls create a script to update all git repositories  
pls automate certificate renewal process
pls monitor website uptime
```

## 🚀 Installation & Shortcuts ✅

### Install Shortcuts
```bash
# Install both 'pls' and legacy 'ol' shortcuts  
please --install-alias

# Remove shortcuts
please --uninstall-alias
```

After installation, you can use:
- `pls` - The new primary command
- `please` - Full command name
- `ol` - Legacy shortcut (still supported)

### Interactive Main Menu
Run `please` without arguments to access the interactive main menu:
- Generate new scripts interactively
- Browse history (🚧 planned)
- Show configuration
- Access help system

## 🔨 Building from Source

### Prerequisites
- Go 1.24.4 or later

### Build Commands

```bash
# Clone repository
git clone <repository-url>
cd please

# Build for current platform
go build -o please main.go

# Cross-compile for different platforms
# Windows
GOOS=windows GOARCH=amd64 go build -o please.exe main.go

# Linux  
GOOS=linux GOARCH=amd64 go build -o please main.go

# macOS
GOOS=darwin GOARCH=amd64 go build -o please main.go
```

## 📋 Requirements

### Runtime Requirements
- **No dependencies** - single binary executable
- **AI Provider Access**: Currently requires Ollama with installed models
  - Download from: https://ollama.ai/
  - Install models: `ollama pull llama3.2` or `ollama pull deepseek-coder`

### Platform-Specific
- **Windows**: PowerShell (usually pre-installed)
- **Linux**: Bash shell, optional xclip/xsel for clipboard
- **macOS**: Bash shell (built-in)

## 🏗️ Architecture

### Modular Design ✅
```
please/
├── main.go              # Clean entry point with natural language processing
├── config/             # Configuration management  
├── providers/          # AI provider implementations
│   ├── provider.go     # Interface definition
│   └── ollama.go       # ✅ Ollama implementation
├── models/             # Smart model selection and ranking
├── ui/                 # Interactive menus and display
├── types/              # Shared data structures
└── script/             # Script operations, validation, and execution
```

### Provider Interface
```go
type Provider interface {
    GenerateScript(request *ScriptRequest) (*ScriptResponse, error)
    Name() string
    IsConfigured(config *Config) bool
}
```

## 🚧 Planned Features (Future Releases)

### v5.0 Planned Features
- **🤔 Interactive Clarification System**: Handle ambiguous requests intelligently
- **🌍 Internationalization**: Multiple languages and tone customization
- **🌐 Browser Viewing**: View scripts with syntax highlighting in browser
- **📝 Advanced Script Editing**: Full editing capabilities with validation
- **📚 Complete History System**: Browse, search, and manage script history

### Provider Implementations
- **🔌 OpenAI Provider**: Complete GPT integration
- **🔌 Anthropic Provider**: Complete Claude integration
- **🔌 Custom Providers**: OpenAI-compatible API support

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
go run main.go show me system information
```

## 📄 License

This project is open source. Feel free to use, modify, and distribute as needed.

## 🆘 Troubleshooting

### Common Issues

**"No models available in Ollama"**
- Ensure Ollama is running: `ollama serve`
- Install a model: `ollama pull llama3.2`

**"Provider ollama is not properly configured"**
- Check Ollama is running on `http://localhost:11434`
- Set `OLLAMA_URL` environment variable if using different URL

**"Clipboard not supported"**
- Linux: Install `xclip` or `xsel`: `sudo apt install xclip`
- The script will still be displayed for manual copying

**"Permission denied executing script"**
- Linux/macOS: Scripts are automatically made executable
- Windows: Run PowerShell as administrator if needed

### Getting Help

- Check the configuration file location for your platform
- Verify Ollama connectivity: `ollama list` 
- Review generated scripts before execution
- Use environment variables to override defaults
- Check `please --help` for usage information

### Migration from OohLama

If you're upgrading from OohLama, Please will:
- Automatically migrate your existing configuration
- Continue to support `OOHLAMA_*` environment variables (with deprecation warnings)
- Maintain backward compatibility with the `ol` command

---

**⚠️ Important Security Notice**: Always review generated scripts before execution. While Please creates safe, well-structured scripts following best practices, you should understand what any script does before running it on your system. Please is politely insistent about this for your safety! 🛡️

**✨ Have a wonderful day, and happy scripting! 🎉**
