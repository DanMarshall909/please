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

### Recently Added Features (Latest Session)

#### Enhanced Script Generation with Retry Logic (🔄)
- **Automatic Validation**: All generated scripts are validated using PowerShell AST parser
- **Intelligent Retry**: Up to 3 attempts to fix validation errors automatically
- **LLM Feedback Loop**: Sends syntax errors back to the AI for correction
- **User Progress Indicators**: Shows retry attempts and fixing progress
- **Graceful Fallback**: Saves final attempt even if not perfect
- **Cancellation Support**: Users can interrupt with Ctrl+C at any time
- **Service**: `EnhancedScriptService` in Application layer
- **Zero Auto-Execution**: Scripts never run without explicit user confirmation

#### Copy to Clipboard (📋)
- **Cross-Platform Support**: Windows (PowerShell), Linux (xclip/xsel/wl-copy), macOS (pbcopy)
- **Smart Detection**: Automatically detects available clipboard utilities
- **Error Handling**: Graceful fallback with helpful user messages
- **User Feedback**: Shows script statistics and provider information
- **Interface**: `IClipboardService` in Domain, `ClipboardService` in Infrastructure

#### Save to File (💾)
- **Intelligent Naming**: Auto-generates safe filenames from task descriptions
- **File Extensions**: Script-type aware (.ps1, .sh, .py, .bat, .txt)
- **Metadata Headers**: Rich comments with provider, timestamp, risk level, warnings
- **Conflict Resolution**: Automatic versioning for duplicate filenames
- **Cross-Platform Paths**: Uses appropriate default directories per OS
- **Interface**: `IFileService` in Domain, `FileService` in Infrastructure

#### Script History Browser (🕒)
- **CLI Command**: `please --history` / `please -r` / `please history`
- **Interactive Browser**: List last 20 scripts with task descriptions and timestamps
- **Time Display**: Human-readable format (just now, 5m ago, 2h ago, 3d ago)
- **Script Actions**: Execute, Edit & Execute, Copy to Clipboard, Save to File
- **History Management**: View all scripts, clear history with confirmation
- **Empty State**: Helpful guidance when no history exists
- **Backend**: Leverages existing `IScriptRepository` infrastructure

### Menu Integration
The interactive script menu now provides:
- 🚀 Execute script now
- ✏️ Edit in external editor
- 📋 **Copy to clipboard** ✅ (NEW)
- 💾 **Save to file** ✅ (NEW)
- ❌ Cancel

### Command Line Interface
```bash
# Core functionality
please create a backup script for my documents
please list running services

# New commands
please --history     # Browse script history
please -r           # Short form of --history
please history      # Alternative form

# All existing commands
please --install    # Install to system
please --status     # Show status
please --version    # Show version
please --help       # Show help
```

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
- **Enhanced Script Generation with Retry Logic** ✅ (NEW)
- Syntax validation with auto-fix
- External editor support
- Security validation and warnings
- Professional console UI
- Natural language processing
- **Copy to Clipboard** (cross-platform)
- **Save to File** (with metadata headers)
- **Script History Browser** (browse, re-run, manage past scripts)
- **Cancellation Support** (Ctrl+C handling) ✅ (NEW)
- Comprehensive testing with enterprise naming conventions

### Code Quality Standards
- **Zero warnings**: Treats warnings as errors
- **Comprehensive testing**: Unit, integration, and TDD coverage
- **Enterprise Test Naming**: Plain English, behavior-focused (follows https://enterprisecraftsmanship.com/posts/you-naming-tests-wrong/)
- **Test Coverage**: Covers validation, retry logic, cancellation, error handling
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
- **.NET 8**: Latest LTS with Native AOT support
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

### Latest Session Implementation Details

#### Service Registration Pattern
New services follow this pattern in `Infrastructure/DependencyInjection.cs`:
```csharp
services.AddSingleton<IClipboardService, ClipboardService>();
services.AddSingleton<IFileService, FileService>();
```

#### Test Integration Pattern
New services require test doubles in `TestUtilities/TestModule.cs`:
```csharp
services.AddSingleton<FakeClipboardService>();
services.AddSingleton<FakeFileService>();
services.AddSingleton<IClipboardService>(sp => sp.GetRequiredService<FakeClipboardService>());
services.AddSingleton<IFileService>(sp => sp.GetRequiredService<FakeFileService>());
```

#### TaskProcessor Constructor Updates
When adding new services, update TaskProcessor constructor:
```csharp
public TaskProcessor(IServiceProvider serviceProvider, ILogger<TaskProcessor> logger,
    CommandLineArguments arguments, IConsoleUIService consoleUI, 
    IClipboardService clipboardService, IFileService fileService)
```

And all test instantiations:
```csharp
new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService)
```

#### Command Line Arguments Pattern
Special commands follow this pattern in `CommandLineArguments.cs`:
```csharp
// Add property
public bool IsNewCommand { get; }

// Add parsing logic
IsNewCommand = firstArg == "--command" || firstArg == "-c" || firstArg == "command";

// Add to special command check
if (IsInstallCommand || IsStatusCommand || IsVersionCommand || IsHelpCommand || IsHistoryCommand || IsNewCommand)
```

#### Cross-Platform Considerations
- PowerShell scripts: Use `pwsh` on Linux/macOS, `powershell.exe` on Windows
- File paths: Use `Path.Combine()` and `Path.GetInvalidFileNameChars()`
- Clipboard: Platform-specific utilities (xclip/xsel/wl-copy on Linux, pbcopy on macOS)
- Default directories: Use `Environment.SpecialFolder` enums

#### Test Isolation
File-based tests require unique directories to prevent interference:
```csharp
private readonly string _testDirectory;

public TestClass()
{
    _testDirectory = Path.Combine(Path.GetTempPath(), "TestName", Guid.NewGuid().ToString());
    Directory.CreateDirectory(_testDirectory);
}

public void Dispose()
{
    if (Directory.Exists(_testDirectory))
        Directory.Delete(_testDirectory, true);
}
```

This architecture provides a robust, secure, and user-friendly platform for AI-powered script generation with comprehensive validation and safety features.