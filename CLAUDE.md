# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Please** is an AI-powered script generation tool that transforms natural language descriptions into executable scripts. It features a dual implementation strategy providing both reliability and modern architecture.

### Implementations
- **C# v6.0** (Primary/Production): Clean Architecture in `src/` - Modern, feature-rich implementation
- **Go v5.0** (Stable/Legacy): Monolithic implementation in `legacy-go/` - Stable fallback

### AI Provider Support
The project supports 5 AI providers:
- **OpenAI** (GPT-4, GPT-3.5) - API key required
- **Anthropic** (Claude 3) - API key required  
- **Google Gemini** - API key required
- **OpenRouter** (Multiple models) - API key required
- **Ollama** (Local models) - No API key needed, runs locally

## Build Commands

### C# Version (Primary) - Production Ready ✅
```bash
# Build the solution
dotnet build

# Build specific console app
dotnet build src/Presentation/Please.Console

# Run the console application
dotnet run --project src/Presentation/Please.Console

# Run with arguments
dotnet run --project src/Presentation/Please.Console -- "create a PowerShell script to list files"

# Build for release
dotnet build -c Release

# Output location: src/Presentation/Please.Console/bin/Debug/net8.0/Please.Console.exe
```

### Go Version (Legacy) - Stable Reference ✅
```bash
cd legacy-go

# Build for current platform
go build -o please.exe

# Cross-platform build script
./build.sh

# Simple build
go build

# Run with arguments
./please.exe "create a script to list files"
```

## Testing Commands

### C# Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Domain.UnitTests/Please.Domain.UnitTests
dotnet test tests/Application.UnitTests/Please.Application.UnitTests
dotnet test tests/Infrastructure.UnitTests/Please.Infrastructure.UnitTests
dotnet test tests/Presentation.UnitTests/Please.Presentation.UnitTests
dotnet test tests/Application.IntegrationTests/Please.Application.IntegrationTests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Go Tests
```bash
cd legacy-go

# Run all tests
go test ./...

# Run specific package tests
go test ./config
go test ./providers
```

## Architecture

### C# Clean Architecture (src/) - Jason Taylor's Clean Architecture Pattern
```
src/
├── Domain/              # ZERO dependencies - Core business logic
│   ├── Entities/        # ScriptRequest, ScriptResponse, CommandContext
│   ├── Enums/          # ProviderType, RiskLevel, ScriptType
│   ├── Interfaces/     # Repository and service contracts (ISecureConfigurationService, ISecureInputService)
│   ├── ValueObjects/   # Result<T>, SecureString, Error types
│   └── Exceptions/     # Domain exceptions
├── Application/        # Use cases and business workflows (Direct Services - NO MediatR for Native AOT)
│   ├── Services/       # ScriptService, CommandProcessor, TaskProcessor
│   └── Common/         # DTOs, interfaces, models
├── Infrastructure/     # ALL external dependencies
│   ├── Providers/      # AI provider implementations (OpenAI, Anthropic, Gemini, OpenRouter, Ollama)
│   ├── Services/       # SecureConfigurationService, ScriptGenerator, WindowsEncryptedStorage
│   ├── Security/       # ApiKeyValidator, SecureStringExtensions
│   └── Repositories/   # File-based persistence (NO Entity Framework)
└── Presentation/       # Console application with professional UI
    └── Console/        # CLI interface with Spectre.Console UI, dependency injection
```

### Go Monolithic Architecture (legacy-go/) - Stable Reference Implementation
```
legacy-go/
├── main.go             # Entry point
├── config/             # Configuration management
├── localization/       # Internationalization
├── models/             # Selection logic
├── providers/          # AI provider implementations (OpenAI, Anthropic, Ollama)
├── script/             # Script operations and validation
├── types/              # Type definitions
├── ui/                 # User interface components
└── themes/             # Localization themes
```

## Key Design Patterns

### Result Pattern (C#)
The codebase uses explicit Result<T> types instead of exceptions for predictable error handling:
```csharp
public Result<ScriptResponse> GenerateScript(ScriptRequest request)
{
    if (request.IsInvalid)
        return Result<ScriptResponse>.Failure("Invalid request");
    
    return Result<ScriptResponse>.Success(response);
}
```

### Provider Factory Pattern
AI providers are created through a factory with automatic selection:
```csharp
var provider = providerFactory.CreateProvider(ProviderType.OpenAI);
```

### Secure Configuration System ✅ COMPLETED
Enhanced API key security with encrypted local storage:
- **Priority Chain**: Environment Variables → Encrypted Storage → User Secrets → Configuration
- **Windows DPAPI**: User-scoped encryption for stored API keys
- **Backward Compatible**: Existing configuration methods still work
- **Interactive Prompts**: Optional guided setup

### Professional Console UI ✅ COMPLETED
Implemented with Spectre.Console:
- **IConsoleUIService Interface**: Clean abstraction for console operations
- **Beautiful Script Display**: Panels with syntax highlighting and borders
- **Progress Indicators**: Professional spinners with status messages
- **Risk Warning System**: Color-coded safety indicators
- **Interactive Menus**: Framework ready for single-key navigation

### Clean Architecture Dependencies (Strict Jason Taylor Pattern)
- **Domain** → ZERO dependencies (not even Microsoft.Extensions.*)
- **Application** → Depends only on Domain (Direct Services, NO MediatR for Native AOT)
- **Infrastructure** → Implements Domain interfaces, contains ALL external dependencies
- **Presentation** → Orchestrates through dependency injection, UI-specific concerns only

## Configuration

### Quick Setup (Recommended) 🚀
Use automated configuration scripts for fastest setup:

#### Windows (PowerShell)
```powershell
# Interactive setup with intelligent defaults
.\scripts\setup-environment.ps1

# Direct provider setup with permanent storage
.\scripts\setup-environment.ps1 -Provider OpenAI -Permanent
```

#### Linux/macOS/WSL (Bash)  
```bash
# Interactive setup
./scripts/setup-environment.sh

# Direct provider setup with permanent storage
./scripts/setup-environment.sh --provider openai --permanent
```

### Configuration Priority Chain
1. **Environment Variables** (highest priority) 🔐 SECURE
2. **Encrypted Local Storage** (Windows DPAPI) 🔐 SECURE  
3. **User Secrets** (development) 🔐 SECURE
4. **appsettings.json** (non-sensitive data only)
5. **Default Values** (lowest priority)

### Supported Providers & Default Models
| Provider | Key Format | Default Model | Local |
|----------|------------|---------------|-------|
| **OpenAI** | `sk-...` | `gpt-4o-mini` | ❌ |
| **Anthropic** | `sk-ant-...` | `claude-3-haiku-20240307` | ❌ |
| **Google Gemini** | `[39 chars]` | `gemini-pro` | ❌ |
| **OpenRouter** | `sk-or-...` | `microsoft/wizardlm-2-8x22b` | ❌ |
| **Ollama** | N/A | `llama2` | ✅ |

## Development Workflow

### Current Status ✅ PRODUCTION READY
- **C# v6.0**: Complete implementation with all 5 AI providers, professional UI, secure configuration
- **Go v5.0**: Stable reference implementation, maintenance mode

### Working with C# Version (Primary Focus)
1. **Focus development** in `src/` directory
2. **Follow Clean Architecture** principles (Jason Taylor pattern)
3. **Use Result<T> pattern** for error handling instead of exceptions
4. **Add comprehensive unit tests** for each layer
5. **Run build and test commands** before committing:
   ```bash
   dotnet build
   dotnet test
   ```

### Working with Go Version (Maintenance Only)
1. **Work in `legacy-go/` directory** - tagged as `v5.0-stable`
2. **Maintain backward compatibility** - no breaking changes
3. **Focus on bug fixes** and minor enhancements only
4. **Run tests before committing**:
   ```bash
   cd legacy-go
   go test ./...
   ```

### Git Branch Strategy
- **v2** (current): Primary development branch
- **master**: Main branch for PRs
- **release/please-v5-stable**: Go v5.0 stable releases
- **legacy/archive**: Previous experimental work

## Important Files & Directories

### Solution Structure
- **Please.sln** - Main C# solution file
- **Directory.Build.props** - Treats warnings as errors across all projects
- **src/** - C# Clean Architecture implementation
- **legacy-go/** - Go v5.0 stable implementation with go.mod
- **tests/** - Comprehensive test suite for C# version
- **scripts/** - Automated environment setup scripts (PowerShell & Bash)
- **memory-bank/** - Project documentation, architecture decisions, and planning

### Key Documentation Files
- **README.md** - Main project overview and quick start
- **ARCHITECTURE.md** - Technical design and dual implementation strategy
- **CONFIGURATION.md** - Detailed provider setup and security guide
- **DEVELOPMENT.md** - Development workflow and contribution guide
- **GETTING-STARTED.md** - Step-by-step setup instructions
- **scripts/README.md** - Setup script documentation
- **legacy-go/README.md** - Go implementation reference

## Security Implementation ✅ COMPLETED

### Secure Configuration Features
- **Encrypted Local Storage**: Windows DPAPI for API key encryption
- **Priority Chain**: Environment Variables → Encrypted Storage → User Secrets → Configuration
- **Memory Security**: SecureString usage with automatic memory clearing
- **API Key Validation**: Built-in validation for all provider types
- **Interactive Setup**: Secure prompts that hide sensitive input

### Security Best Practices
- **Never commit secrets** to the repository
- **Use ISecureConfigurationService** for all sensitive data access
- **Automatic memory clearing** after sensitive operations
- **User-scoped encryption** (other users cannot decrypt keys)
- **Script safety validation** before execution

## Testing Strategy

### C# Testing Framework ✅ COMPREHENSIVE
- **Unit Tests**: Each layer has 90%+ test coverage
- **Integration Tests**: End-to-end provider testing with real APIs
- **Test Utilities**: Shared builders, fakes, and utilities
- **Framework**: NUnit with Shouldly assertions and NSubstitute mocking

### Test Architecture
```
tests/
├── Domain.UnitTests/           # Pure domain logic tests
├── Application.UnitTests/      # Service layer tests with mocks
├── Infrastructure.UnitTests/   # Provider and service implementations
├── Presentation.UnitTests/    # Console UI and command tests
├── Application.IntegrationTests/ # End-to-end workflow tests
└── TestUtilities/             # Shared test infrastructure
```

### Testing Patterns
- **Arrange-Act-Assert** pattern throughout
- **Builder pattern** for complex test data creation
- **Fake implementations** for dependencies
- **Integration tests** with real AI providers (when configured)
- **TDD approach** for new features

## Usage Examples

### Basic Script Generation
```bash
# PowerShell scripts
.\Please.Console.exe "create a script to clean temporary files"

# Batch files  
.\Please.Console.exe "write a batch file to backup my documents"

# Python scripts
.\Please.Console.exe "generate a Python script to process CSV files"
```

### System Administration
```bash
# File management
.\Please.Console.exe "create a PowerShell script to organize files by date"

# System monitoring
.\Please.Console.exe "write a script to check disk usage and send alerts"

# Network utilities
.\Please.Console.exe "generate a script to test network connectivity"
```

### Development Tasks
```bash
# Build automation
.\Please.Console.exe "create a script to build and deploy my application"

# Testing utilities
.\Please.Console.exe "write a script to run tests and generate reports"

# Environment setup
.\Please.Console.exe "generate a script to install development dependencies"
```

## Technology Stack

### C# Version Dependencies
- **.NET 8** (Latest LTS)
- **Microsoft.Extensions.*** (Configuration, DI, Logging)
- **Spectre.Console** (Professional terminal UI)
- **System.Text.Json** (JSON serialization)
- **HttpClient** with **Polly** (HTTP resilience)

### Testing Dependencies
- **NUnit** (Test framework)
- **Shouldly** (Readable assertions)
- **NSubstitute** (Mocking framework)
- **Microsoft.Extensions.Testing** (DI testing utilities)

### Excluded Dependencies (By Design)
- ❌ **Entity Framework** (File-based persistence only)
- ❌ **MediatR** (Direct Services for Native AOT compatibility)
- ❌ **AutoMapper** (Simple record mapping)
- ❌ **Heavy ORMs** (Plain file I/O preferred)

## Current Implementation Status

### ✅ Phase 1: Foundation (COMPLETE)
- Clean Architecture solution structure
- Domain entities, enums, and interfaces
- Zero-dependency domain layer

### ✅ Phase 2: Core Services (COMPLETE)
- Application layer with direct services
- Result<T> pattern implementation
- Service method implementations

### ✅ Phase 3: Infrastructure (COMPLETE)
- All 5 AI provider implementations
- Secure configuration system with encryption
- File-based repositories
- HTTP client configurations

### ✅ Phase 4: Professional UI (COMPLETE)
- Spectre.Console integration
- Professional console interface
- Progress indicators and status display
- Risk warning system

### ✅ Phase 5: Testing & Validation (COMPLETE)
- Comprehensive test suite
- Integration tests with real providers
- 90%+ test coverage
- Performance validation

## Memory Bank Documentation

The `memory-bank/` directory contains comprehensive project documentation:

### Architecture & Planning
- **Architecture decisions** and comparisons
- **Clean Architecture migration** strategy
- **Multi-agent workflow** strategies

### Current Tasks
- **Console UI design** specifications
- **Secure configuration** implementation details
- **Feature gap analysis** between Go and C# versions
- **Infrastructure layer** completion status

### Future Enhancements
- **Security enhancements** roadmap
- **CLI rules** from secure configuration
- **UI walking skeleton** implementation plans

### Reference Materials
- **AI provider implementation** details
- **Codex execution strategy** documentation
- **Testing strategy** for C# migration

## Troubleshooting Common Issues

### "API key not configured"
**Solution**: Run setup script or set environment variable:
```bash
.\scripts\setup-environment.ps1 -Provider OpenAI
```

### "Could not find Please.Console.exe"
**Solution**: Build the project first:
```bash
dotnet build src/Presentation/Please.Console
```

### Environment variables not persisting
**Solution**: Use permanent storage flag:
```bash
.\scripts\setup-environment.ps1 -Provider OpenAI -Permanent
```

### Go version issues
**Solution**: Work in legacy-go directory:
```bash
cd legacy-go
go build -o please.exe
```