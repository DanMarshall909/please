# Please - AI-Powered Script Generation

**Please** is an AI-powered command-line tool that generates scripts from natural language descriptions. Simply describe what you need in plain English, and Please creates executable scripts with proper validation and safety features.

## Natural Language Interface

Please accepts natural language commands without quotes:

```bash
# Build and install
dotnet build src/Presentation/Please.Console -c Release

# Use natural language directly
please find files older than 3 days
please create a backup script for my documents
please list all running services on this computer
please generate a script to clean temporary files
```

## Core Features

- **Natural Language Processing**: No quotes or special syntax required
- **Syntax Validation**: PowerShell AST parser with auto-fix capabilities
- **External Editor Support**: Edit scripts in VS Code, nano, vim, or any preferred editor
- **Security Validation**: 4-tier risk assessment with safety warnings
- **5 AI Providers**: OpenAI, Anthropic, Google Gemini, OpenRouter, Ollama
- **Professional UI**: Beautiful terminal interface with progress indicators

## Quick Start

### Installation
```bash
# Clone repository
git clone https://github.com/DanMarshall909/please.git
cd please

# Build release version
dotnet build src/Presentation/Please.Console -c Release

# Copy executable to PATH (optional)
cp src/Presentation/Please.Console/bin/Release/net8.0/win-x64/please.exe /usr/local/bin/
```

### Configuration
```bash
# Automated setup
./scripts/setup-environment.ps1 -Provider OpenAI -Permanent
./scripts/setup-environment.sh --provider anthropic --permanent

# Or set environment variables
export OPENAI_API_KEY="your-key-here"
export ANTHROPIC_API_KEY="your-key-here"
```

### Usage Examples
```bash
# File operations
please find duplicate files in downloads folder
please create a script to organize photos by date

# System administration  
please show system information and memory usage
please create a PowerShell script to monitor disk space
please list all installed programs

# Development tasks
please create a backup script for my project files
please generate a script to clean build artifacts
please create a PowerShell script to restart specific services
```

## Key Capabilities

### Syntax Validation and Auto-Fix
- Native PowerShell AST parser detects real syntax errors
- Automatically fixes common AI mistakes like non-existent cmdlets
- Re-validates scripts after editing or modification
- Provides line-specific error messages

### External Editor Integration
- Automatically detects available editors (VS Code, Notepad++, nano, vim)
- Supports `--wait` flag for proper editor integration
- Creates temporary files with correct extensions (.ps1, .sh, .py, .bat)
- Re-validates and applies fixes after editing

### Security Features
- 4-tier risk assessment (Low, Medium, High, Critical)
- Pattern-based security analysis for dangerous operations
- Non-interactive environment safety (CI/automation protection)
- Encrypted local storage for API keys (Windows DPAPI)

### Professional User Experience
- Beautiful terminal interface using Spectre.Console
- Progress indicators during AI generation
- Interactive menus with context-aware options
- Syntax highlighting and script preview
- Color-coded risk warnings and safety notes

## AI Provider Support

| Provider | Default Model | Local | API Key Required |
|----------|---------------|-------|------------------|
| OpenAI | gpt-4o-mini | No | Yes |
| Anthropic | claude-3-haiku-20240307 | No | Yes |
| Google Gemini | gemini-pro | No | Yes |
| OpenRouter | microsoft/wizardlm-2-8x22b | No | Yes |
| Ollama | llama3:latest | Yes | No |

## Architecture

Built using Clean Architecture principles with strict separation of concerns:

- **Domain Layer**: Zero dependencies, core business logic
- **Application Layer**: Use cases and business workflows
- **Infrastructure Layer**: AI providers, validation services, security
- **Presentation Layer**: Console UI with Spectre.Console

## Development

### Prerequisites
- .NET 8 SDK
- PowerShell (for syntax validation)
- VS Code or preferred editor

### Build and Test
```bash
# Build solution
dotnet build

# Run comprehensive tests
dotnet test

# Test specific features
dotnet test --filter="Auto_fix_corrects_nonexistent_cmdlets"
dotnet test --filter="External_editor_functionality"
```

### Code Quality
- Zero warnings policy (treats warnings as errors)
- 90%+ test coverage across all layers
- TDD approach with Red-Green-Refactor-Cover-Commit cycle
- Enterprise test naming conventions

## Configuration Options

### Environment Variables
- `PLEASE_EDITOR`: Preferred editor for script editing
- `EDITOR` / `VISUAL`: Standard Unix editor variables
- Provider-specific API keys (see documentation)

### Security Configuration
API keys are stored securely using:
1. Environment variables (highest priority)
2. Encrypted local storage (Windows DPAPI)
3. User Secrets (development)
4. Configuration files (non-sensitive data only)

## Legacy Go Implementation

A stable Go v5.0 implementation is available in `legacy-go/` for users preferring a lightweight alternative:

```bash
cd legacy-go
go build -o please.exe
./please.exe "create a script to list files"
```

## Contributing

Contributions welcome for:
- AI provider integrations
- Security improvements
- UI enhancements
- Documentation improvements
- Bug fixes and optimizations

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

*Built with .NET 8, Clean Architecture, and comprehensive testing for reliable AI-powered script generation.*