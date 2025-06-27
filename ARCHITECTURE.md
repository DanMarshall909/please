# Architecture Documentation

## Overview

Please implements Clean Architecture principles with strict separation of concerns, designed for maintainability, testability, and performance.

## Clean Architecture Implementation

### Layer Structure
```
src/
├── Domain/              # ZERO dependencies
│   ├── Entities/        # ScriptRequest, ScriptResponse, CommandContext
│   ├── Enums/          # ProviderType, RiskLevel, ScriptType
│   ├── Interfaces/     # Service and repository contracts
│   └── Services/       # Domain service interfaces
├── Application/        # Business workflows
│   └── Services/       # ScriptService, CommandProcessor
├── Infrastructure/     # ALL external dependencies
│   ├── Providers/      # AI provider implementations
│   ├── Services/       # Configuration, validation, generation
│   └── Security/       # Encryption, API key management
└── Presentation/       # Console application
    └── Console/        # CLI with Spectre.Console UI
```

### Dependency Rules
- **Domain**: Zero dependencies, pure business logic
- **Application**: Depends only on Domain, contains use cases
- **Infrastructure**: Implements Domain interfaces, contains all external dependencies
- **Presentation**: Orchestrates through dependency injection

## Core Design Patterns

### Result Pattern
Explicit error handling without exceptions:
```csharp
public Result<ScriptResponse> GenerateScript(ScriptRequest request)
{
    if (request.IsInvalid)
        return Result<ScriptResponse>.Failure("Invalid request");
    
    return Result<ScriptResponse>.Success(response);
}
```

### Provider Factory Pattern
Automatic AI provider selection and creation:
```csharp
var provider = providerFactory.CreateProvider(ProviderType.OpenAI);
var result = await provider.GenerateScriptAsync(request);
```

### Command Line Processing
Natural language input processing:
```csharp
// CommandLineArguments.cs
TaskDescription = string.Join(" ", RawArguments);
```

## Key Components

### Syntax Validation System
- **PowerShell AST Parser**: Native syntax validation using Microsoft.PowerShell.SDK
- **Auto-Fix Engine**: Corrects common AI syntax errors automatically
- **Semantic Validation**: Detects non-existent cmdlets and parameter issues
- **Integration**: Re-validation after editing or fixing

### External Editor Integration
- **Smart Detection**: Automatically finds available editors
- **Environment Variables**: PLEASE_EDITOR, EDITOR, VISUAL support
- **Wait Capability**: VS Code integration with --wait flag
- **Cross-Platform**: Windows, Linux, macOS support

### Security Validation
- **4-Tier Risk Assessment**: Low, Medium, High, Critical
- **Pattern Matching**: Regex-based security analysis
- **Syntax Integration**: Combined syntax and security validation
- **Safety Features**: Non-interactive environment protection

### Configuration Management
Security-first configuration with multiple sources:
1. Environment Variables (highest priority)
2. Encrypted Local Storage (Windows DPAPI)
3. User Secrets (development)
4. Configuration files (non-sensitive only)

### Professional UI
- **Spectre.Console**: Beautiful terminal rendering
- **Progress Indicators**: Spinners and multi-step progress
- **Interactive Menus**: Context-aware options
- **Risk Warnings**: Color-coded security indicators

## AI Provider Architecture

### Provider Interface
```csharp
public interface IAiProvider
{
    Task<Result<string>> GenerateScriptAsync(string prompt, CancellationToken cancellationToken = default);
    bool IsAvailable();
    string Name { get; }
}
```

### Supported Providers
- **OpenAI**: GPT-4, GPT-3.5 with retry policies
- **Anthropic**: Claude 3 with streaming support
- **Google Gemini**: Latest Gemini models
- **OpenRouter**: Multiple model access
- **Ollama**: Local model execution

### Provider Factory
Automatic selection based on availability and configuration:
```csharp
public IAiProvider CreateProvider(ProviderType? requestedType = null)
{
    // Try requested provider first
    // Fall back to available providers
    // Prioritize local providers (Ollama) when available
}
```

## Security Architecture

### API Key Management
- **Encrypted Storage**: Windows DPAPI for local storage
- **Memory Security**: SecureString usage with automatic clearing
- **Validation**: Built-in API key format validation
- **Priority Chain**: Environment → Encrypted → User Secrets → Config

### Script Validation
- **Syntax Parsing**: Real PowerShell AST validation
- **Security Analysis**: Pattern-based dangerous operation detection
- **Risk Assessment**: 4-tier classification with specific warnings
- **Auto-Fix**: Common mistake correction with re-validation

### Safety Features
- **Non-Interactive Detection**: CI/automation environment safety
- **Execution Confirmation**: Final review before script execution
- **Risk Warnings**: Clear security indicators and recommendations

## Testing Architecture

### Test Structure
```
tests/
├── Domain.UnitTests/           # Pure domain logic tests
├── Application.UnitTests/      # Service layer with mocks
├── Infrastructure.UnitTests/   # Provider implementations
├── Presentation.UnitTests/    # Console UI and commands
├── Application.IntegrationTests/ # End-to-end workflows
└── TestUtilities/             # Shared test infrastructure
```

### Testing Strategy
- **TDD Approach**: Red-Green-Refactor-Cover-Commit cycle
- **90%+ Coverage**: Comprehensive test coverage across layers
- **Enterprise Naming**: Plain English, behavior-focused test names
- **Mocking**: NSubstitute for dependency isolation
- **Integration**: Real provider testing when configured

## Technology Choices

### Core Dependencies
- **.NET 8**: Latest LTS with Native AOT support
- **Microsoft.Extensions.***: Configuration, DI, Logging
- **Spectre.Console**: Professional terminal UI
- **Microsoft.PowerShell.SDK**: Native PowerShell parsing
- **System.Text.Json**: High-performance serialization

### Excluded Dependencies
- **Entity Framework**: File-based persistence for simplicity
- **MediatR**: Direct services for Native AOT compatibility
- **AutoMapper**: Simple record mapping preferred
- **Heavy ORMs**: Performance and complexity considerations

### Performance Considerations
- **Native AOT Ready**: Designed for ahead-of-time compilation
- **Minimal Dependencies**: Reduced startup time and memory usage
- **Async/Await**: Proper asynchronous patterns throughout
- **Efficient JSON**: System.Text.Json for better performance

## Build and Deployment

### Build Configuration
- **AssemblyName**: `please` for natural command-line usage
- **PublishAot**: Native AOT compilation support
- **PublishSingleFile**: Self-contained executable
- **TrimMode**: Link-level trimming for smaller binaries

### Cross-Platform Support
- **Windows**: Full feature set with DPAPI encryption
- **Linux/macOS**: Core functionality with environment variable configuration
- **Editors**: Platform-specific editor detection and integration

## Future Architecture Considerations

### Extensibility Points
- **Provider Interface**: Easy addition of new AI providers
- **Validation Rules**: Configurable security pattern matching
- **Editor Support**: Pluggable editor detection and integration
- **UI Themes**: Customizable console appearance

### Performance Optimizations
- **Caching**: Provider response caching for repeated requests
- **Streaming**: Streaming responses for large script generation
- **Batch Processing**: Multiple script generation in single request

### Security Enhancements
- **Sandboxing**: Script execution in isolated environments
- **Audit Logging**: Security event tracking and reporting
- **Policy Engine**: Organizational security policy enforcement

This architecture provides a robust foundation for AI-powered script generation with comprehensive validation, security, and user experience features.