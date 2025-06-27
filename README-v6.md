# 🤖 Please v6 - AI-Powered Script Generator

[![Build & Release](https://github.com/DanMarshall909/please/actions/workflows/release-v2.yml/badge.svg)](https://github.com/DanMarshall909/please/actions/workflows/release-v2.yml)

**Please v6** is an AI-powered script generation tool that transforms natural language descriptions into executable scripts. Built with .NET 8 Native AOT, it's available as single-file executables for all platforms with automatic installation capabilities.

## ✨ Features

- 🎯 **Single-File Executables** - No dependencies, just download and run
- 🏠 **Auto-Installation** - Optionally install to system on first run  
- 📁 **Platform-Consistent Storage** - Data stored in appropriate OS locations
- 🤖 **5 AI Providers** - OpenAI, Anthropic, Gemini, OpenRouter, Ollama
- 🎨 **Syntax Highlighting** - Beautiful PowerShell script display with colors
- 🔍 **Script Validation** - Built-in PowerShell AST validation with auto-fix
- 🛡️ **Security Assessment** - 4-tier risk analysis with safety warnings
- ⚡ **Native AOT** - Fast startup, minimal memory footprint

## 🚀 Quick Start

### Option 1: Download Single-File Executable (Recommended)

**Windows (PowerShell):**
```powershell
# One-line installer
iwr -useb https://raw.githubusercontent.com/DanMarshall909/please/main/install.ps1 | iex

# Or manual download from releases
```

**Linux/macOS (Bash):**
```bash
# One-line installer  
curl -fsSL https://raw.githubusercontent.com/DanMarshall909/please/main/install.sh | bash

# Or manual download from releases
```

**Manual Download:**
Visit [Releases](https://github.com/DanMarshall909/please/releases) and download:
- `please-windows-x64.exe` - Windows 10/11 (Intel/AMD)
- `please-windows-arm64.exe` - Windows 11 (ARM64)
- `please-linux-x64` - Linux (Intel/AMD)
- `please-linux-arm64` - Linux (ARM64)
- `please-macos-x64` - macOS Intel
- `please-macos-arm64` - macOS Apple Silicon

### First Run Experience

When you run Please for the first time, it will:
1. 🎉 Welcome you with installation options
2. 📍 Offer to install to your system PATH
3. 🗂️ Set up platform-appropriate data directories
4. ⚡ Be ready to generate scripts immediately

```bash
# First run
./please get current time

# After installation (available anywhere)
please create backup script for my documents
please list running services
please find files older than 7 days
```

## 🛠️ Usage

### Commands
```bash
# Generate scripts with natural language
please get current time
please list all running services
please create backup script for my documents
please find files modified in the last week

# Utility commands
please --help      # Show help
please --version   # Show version info
please --status    # Show installation status
please --install   # Force installation to system
```

### Natural Language Examples
```bash
please "get system information"
please "backup my downloads folder"
please "find large files over 100MB"
please "restart the print spooler service"
please "check disk space on all drives"
please "list all installed programs"
```

## 🔧 Configuration

### AI Provider Setup

Please supports 5 AI providers. Configure at least one:

```bash
# Environment variables (recommended)
export OPENAI_API_KEY="sk-..."
export ANTHROPIC_API_KEY="sk-ant-..."
export GEMINI_API_KEY="..."
export OPENROUTER_API_KEY="sk-or-..."
# Ollama runs locally, no API key needed

# Using setup scripts
./scripts/setup-environment.ps1 -Provider OpenAI -Permanent
./scripts/setup-environment.sh --provider anthropic --permanent
```

### Provider Defaults
| Provider   | Default Model              | Local |
|------------|----------------------------|-------|
| OpenAI     | gpt-4o-mini               | No    |
| Anthropic  | claude-3-haiku-20240307   | No    |
| Gemini     | gemini-pro                | No    |
| OpenRouter | microsoft/wizardlm-2-8x22b| No    |
| Ollama     | llama3:latest             | Yes   |

### Data Storage Locations

Please stores data in platform-appropriate locations:

**Windows:**
- Config: `%APPDATA%\Please`
- Data: `%LOCALAPPDATA%\Please`
- Install: `%LOCALAPPDATA%\Programs\Please`

**Linux:**
- Config: `~/.config/please`
- Data: `~/.local/share/please`
- Install: `~/.local/bin`

**macOS:**
- Config: `~/Library/Application Support/Please`
- Data: `~/.local/share/please`
- Install: `~/.local/bin`

## 🏗️ Development

### Build from Source
```bash
# Clone and build
git clone https://github.com/DanMarshall909/please.git
cd please
dotnet build

# Run from source
dotnet run --project src/Presentation/Please.Console -- get current time
```

### Build Single-File Executable
```bash
# Build for your platform
dotnet publish src/Presentation/Please.Console -c Release -r win-x64 --self-contained

# Build for all platforms (requires GitHub Actions or cross-compilation setup)
dotnet publish src/Presentation/Please.Console -c Release -r linux-x64 --self-contained
dotnet publish src/Presentation/Please.Console -c Release -r osx-x64 --self-contained
dotnet publish src/Presentation/Please.Console -c Release -r osx-arm64 --self-contained
```

### Architecture

Please follows Clean Architecture principles:

```
src/
├── Domain/              # Core business logic (no dependencies)
├── Application/         # Use cases and workflows  
├── Infrastructure/      # External services (AI providers, file system)
└── Presentation/        # Console UI with Spectre.Console
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔒 Security

- **Risk Assessment**: 4-tier analysis (Low, Medium, High, Critical)
- **Script Validation**: PowerShell AST parsing catches syntax errors
- **Safe Defaults**: Non-interactive environments default to cancel
- **No Auto-Execution**: Always requires user confirmation
- **Input Sanitization**: All inputs validated before processing

## 📦 Releases

Releases are automatically built using GitHub Actions and include:
- Native AOT compilation for fast startup
- Single-file executables (no dependencies)
- Multi-platform support (Windows, Linux, macOS)
- Both x64 and ARM64 architectures
- Comprehensive testing before release

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/) and Native AOT
- UI powered by [Spectre.Console](https://spectreconsole.net/)
- PowerShell integration via [Microsoft.PowerShell.SDK](https://www.nuget.org/packages/Microsoft.PowerShell.SDK/)
- AI providers: OpenAI, Anthropic, Google, OpenRouter, Ollama

---

**Please v6** - Transform natural language into executable scripts with the power of AI! 🚀