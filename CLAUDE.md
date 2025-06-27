# CLAUDE.md

This file provides guidance to Claude Code when working with this repository.

## Project Overview

**Please** is an AI-powered script generation tool that transforms natural language descriptions into executable scripts. It features a dual implementation strategy providing both reliability and modern architecture.

### Current Status
- **C# v6.0** (Primary/Production): Complete implementation with all features
- **Go v5.0** (Stable/Legacy): Stable reference implementation in maintenance mode

### AI Provider Support
The project supports 5 AI providers:
- **OpenAI** (GPT-4, GPT-3.5) - API key required
- **Anthropic** (Claude 3) - API key required  
- **Google Gemini** - API key required
- **OpenRouter** (Multiple models) - API key required
- **Ollama** (Local models) - No API key needed, runs locally

## Natural Language Usage

The primary interface accepts natural language commands without quotes:

```bash
# Build and use the tool
dotnet build src/Presentation/Please.Console -c Release
./please find files older than 3 days
./please create a backup script for my documents
./please list all running services on this computer
```

Assembly name is configured as `please` for natural command-line usage.

## Build Commands

### C# Version (Primary)
```bash
# Build solution
dotnet build

# Build release
dotnet build src/Presentation/Please.Console -c Release

# Run with natural language
dotnet run --project src/Presentation/Please.Console -- create a PowerShell script to list files

# Output: src/Presentation/Please.Console/bin/Release/net8.0/win-x64/please.exe
```

### Go Version (Legacy)
```bash
cd legacy-go
go build -o please.exe
./please.exe "create a script to list files"
```

## Testing Strategy

### Comprehensive Test Coverage
```bash
# Run all tests
dotnet test

# Test syntax validation and auto-fix
dotnet test --filter="Auto_fix_corrects_nonexistent_cmdlets"

# Test external editor functionality  
dotnet test --filter="Edit_script_externally"

# Integration tests with real providers
dotnet test tests/Application.IntegrationTests/
```

### Test Architecture
- **Unit Tests**: 90%+ coverage across all layers
- **Integration Tests**: End-to-end workflows with real AI providers
- **TDD Approach**: Red-Green-Refactor-Cover-Commit cycle
- **Enterprise Test Naming**: Plain English, behavior-focused

## Architecture

### C# Clean Architecture (Jason Taylor Pattern)
```
src/
├── Domain/              # ZERO dependencies
│   ├── Entities/        # ScriptRequest, ScriptResponse
│   ├── Enums/          # ProviderType, RiskLevel, ScriptType
│   ├── Interfaces/     # UI and service contracts
│   └── Services/       # Domain service interfaces
├── Application/        # Business workflows (Direct Services)
│   └── Services/       # ScriptService, CommandProcessor
├── Infrastructure/     # ALL external dependencies
│   ├── Providers/      # AI provider implementations
│   ├── Services/       # Configuration, validation, generation
│   └── Security/       # Encryption, validation
└── Presentation/       # Console application
    └── Console/        # CLI with Spectre.Console UI
```

### Key Design Patterns
- **Result Pattern**: Explicit error handling instead of exceptions
- **Provider Factory**: Automatic AI provider selection and creation
- **Clean Architecture**: Strict dependency rules, zero domain dependencies
- **Native AOT Ready**: Direct services, no heavy abstractions

## Core Features

### Syntax Validation and Auto-Fix
- **PowerShell AST Parser**: Native syntax validation using Microsoft.PowerShell.SDK
- **Auto-Fix Engine**: Automatically corrects common AI syntax errors
- **Semantic Validation**: Detects non-existent cmdlets and parameter issues
- **Integration**: Full re-validation after editing or fixing

### External Editor Support
- **Smart Detection**: Automatically finds VS Code, Notepad++, nano, vim
- **Environment Variables**: PLEASE_EDITOR, EDITOR, VISUAL support
- **Wait Capability**: VS Code integration with --wait flag
- **Cross-Platform**: Windows, Linux, macOS support
- **Security**: Re-validation after editing

### Security Validation
- **4-Tier Risk Assessment**: Low, Medium, High, Critical
- **Pattern Matching**: Regex-based security analysis
- **Syntax Integration**: Security validation combined with syntax checking
- **Safety Features**: Non-interactive environment protection

### Professional UI
- **Spectre.Console**: Beautiful terminal rendering
- **Progress Indicators**: Spinners and multi-step progress
- **Risk Warnings**: Color-coded security indicators
- **Interactive Menus**: Context-aware options
- **Script Preview**: Syntax highlighting and metadata display

## Configuration

### Security-First Configuration
```bash
# Automated setup scripts
./scripts/setup-environment.ps1 -Provider OpenAI -Permanent
./scripts/setup-environment.sh --provider anthropic --permanent
```

### Priority Chain
1. Environment Variables (highest security)
2. Encrypted Local Storage (Windows DPAPI)
3. User Secrets (development)
4. appsettings.json (non-sensitive only)

### Provider Defaults
| Provider | Default Model | Local |
|----------|---------------|-------|
| OpenAI | gpt-4o-mini | No |
| Anthropic | claude-3-haiku-20240307 | No |
| Gemini | gemini-pro | No |
| OpenRouter | microsoft/wizardlm-2-8x22b | No |
| Ollama | llama3:latest | Yes |

## Development Workflow

### Current Implementation Status
All core features are complete and production-ready:
- AI provider integration (5 providers)
- Syntax validation with auto-fix
- External editor support
- Security validation and warnings
- Professional console UI
- Natural language processing
- Comprehensive testing

### Code Quality Standards
- **Zero warnings**: Treats warnings as errors
- **Comprehensive testing**: Unit, integration, and TDD coverage
- **Security focus**: Input validation, encrypted storage, risk assessment
- **Clean architecture**: Strict layering and dependency rules
- **Performance**: Native AOT compilation ready

### Git Workflow
- **v2**: Primary development branch
- **master**: Main branch for PRs  
- **TDD Cycle**: Red-Green-Refactor-Cover-Commit
- **Feature Branches**: For significant changes

## Technology Stack

### Core Dependencies
- **.NET 9**: Latest version with enhanced Native AOT support
- **Microsoft.Extensions.***: Configuration, DI, Logging
- **Spectre.Console**: Professional terminal UI
- **Microsoft.PowerShell.SDK**: Native PowerShell syntax validation
- **System.Text.Json**: High-performance JSON serialization

### Testing Dependencies
- **NUnit**: Test framework with comprehensive assertions
- **Shouldly**: Readable test assertions
- **NSubstitute**: Mocking framework for dependency isolation

### Excluded by Design
- Entity Framework (file-based persistence)
- MediatR (direct services for Native AOT)
- AutoMapper (simple record mapping)
- Heavy ORMs (performance and Native AOT considerations)

## Important Implementation Notes

### Command Line Processing
Natural language input is processed by joining all arguments:
```csharp
TaskDescription = string.Join(" ", RawArguments);
```

### Non-Interactive Safety
The system detects and safely handles non-interactive environments:
```csharp
if (Environment.GetEnvironmentVariable("CI") == "true" || 
    !Environment.UserInteractive)
{
    return options.Length - 1; // Default to cancel
}
```

### Syntax Validation Integration
PowerShell AST parsing provides real syntax validation:
```csharp
var ast = Parser.ParseInput(script, out tokens, out parseErrors);
```

### Auto-Fix Common Mistakes
Built-in corrections for frequent AI errors:
```csharp
"Get-ComputerName" → "$env:COMPUTERNAME"
"Get-SystemInfo" → "Get-ComputerInfo"
```

This architecture provides a robust, secure, and user-friendly platform for AI-powered script generation with comprehensive validation and safety features.