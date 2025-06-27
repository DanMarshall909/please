# Development Guide

## Current Status

All core features are complete and production-ready:
- AI provider integration (5 providers: OpenAI, Anthropic, Gemini, OpenRouter, Ollama)
- Syntax validation with PowerShell AST parser and auto-fix
- External editor support with smart detection
- Security validation with 4-tier risk assessment
- Professional console UI with progress indicators
- Natural language command processing
- Comprehensive testing with 90%+ coverage

## Development Workflow

### Prerequisites
- .NET 8 SDK
- PowerShell (for syntax validation features)
- Git
- VS Code, Visual Studio, or JetBrains Rider

### Build and Test
```bash
# Clone repository
git clone https://github.com/DanMarshall909/please.git
cd please

# Build solution
dotnet build

# Run all tests
dotnet test

# Build release version
dotnet build src/Presentation/Please.Console -c Release

# Test natural language interface
./please find files older than 3 days
```

### Code Quality Standards
- **Zero Warnings**: Project treats warnings as errors
- **TDD Approach**: Red-Green-Refactor-Cover-Commit cycle
- **Enterprise Test Naming**: Plain English, behavior-focused
- **Clean Architecture**: Strict dependency rules
- **Security Focus**: Input validation and risk assessment

## Testing Strategy

### Test Architecture
```
tests/
├── Domain.UnitTests/           # Pure domain logic, zero dependencies
├── Application.UnitTests/      # Business workflows with mocks
├── Infrastructure.UnitTests/   # Provider implementations and services
├── Presentation.UnitTests/    # Console UI and command processing
├── Application.IntegrationTests/ # End-to-end workflows
└── TestUtilities/             # Shared test infrastructure
```

### Testing Patterns
- **Arrange-Act-Assert**: Standard test structure
- **Builder Pattern**: Complex test data creation
- **Mocking**: NSubstitute for dependency isolation
- **Integration**: Real provider testing when configured
- **Behavior Focus**: Tests describe what the system should do

### Test Examples
```bash
# Test syntax validation
dotnet test --filter="Auto_fix_corrects_nonexistent_cmdlets"

# Test external editor functionality
dotnet test --filter="Edit_script_externally"

# Test security validation
dotnet test --filter="Script_with_corrupted_ai_tokens"

# Integration tests with real providers
dotnet test tests/Application.IntegrationTests/
```

## Architecture Decisions

### Clean Architecture Implementation
- **Domain**: Zero dependencies, pure business logic
- **Application**: Use cases, depends only on Domain
- **Infrastructure**: All external dependencies, implements Domain interfaces
- **Presentation**: Console UI, orchestrates through DI

### Key Design Patterns
- **Result Pattern**: Explicit error handling without exceptions
- **Provider Factory**: Automatic AI provider selection
- **Command Line Processing**: Natural language argument joining
- **Validation Pipeline**: Syntax → Security → Auto-fix → Re-validation

### Technology Choices
```csharp
// Core dependencies
Microsoft.Extensions.*      // Configuration, DI, Logging
Spectre.Console            // Professional terminal UI
Microsoft.PowerShell.SDK   // Native PowerShell parsing
System.Text.Json          // High-performance serialization

// Testing dependencies
NUnit                      // Test framework
Shouldly                   // Readable assertions
NSubstitute               // Mocking framework
```

### Excluded Dependencies
- **Entity Framework**: File-based persistence for simplicity
- **MediatR**: Direct services for Native AOT compatibility
- **AutoMapper**: Simple record mapping preferred
- **Heavy ORMs**: Performance and complexity considerations

## Development Process

### Feature Development
1. **Planning**: Document requirements and acceptance criteria
2. **TDD Cycle**: Write failing test, implement feature, refactor
3. **Testing**: Unit tests, integration tests, manual testing
4. **Documentation**: Update relevant documentation
5. **Review**: Code review and quality checks

### Git Workflow
- **v2**: Primary development branch
- **master**: Main branch for pull requests
- **Feature Branches**: For significant changes
- **Commit Messages**: Clear, descriptive commit messages

### Code Review Checklist
- [ ] Tests cover new functionality
- [ ] No warnings or errors
- [ ] Documentation updated
- [ ] Security considerations addressed
- [ ] Performance impact assessed

## Security Development

### Security-First Approach
- **Input Validation**: All user inputs and AI responses validated
- **Pattern Matching**: Regex-based security analysis
- **Auto-Fix Safety**: Only safe corrections applied automatically
- **Risk Assessment**: 4-tier classification with clear warnings

### Security Testing
```csharp
[Fact]
public void Script_with_dangerous_operations_has_high_risk()
{
    var script = "Remove-Item -Path 'C:\\temp' -Force -Recurse";
    var riskLevel = _validationService.AssessRiskLevel(script, ScriptType.PowerShell);
    riskLevel.ShouldBe(RiskLevel.High);
}
```

## Performance Considerations

### Native AOT Readiness
- **Direct Services**: No heavy abstraction layers
- **Minimal Dependencies**: Reduced startup time
- **Efficient JSON**: System.Text.Json for performance
- **Async Patterns**: Proper asynchronous programming

### Build Configuration
```xml
<PropertyGroup>
    <PublishAot>true</PublishAot>
    <PublishSingleFile>true</PublishSingleFile>
    <TrimMode>link</TrimMode>
    <AssemblyName>please</AssemblyName>
</PropertyGroup>
```

## Contributing Guidelines

### Getting Started
1. Fork the repository
2. Create a feature branch
3. Follow TDD approach
4. Ensure all tests pass
5. Update documentation
6. Submit pull request

### Coding Standards
- **C# Conventions**: Follow .NET coding standards
- **Async/Await**: Use async patterns for I/O operations
- **Error Handling**: Use Result pattern, avoid exceptions for control flow
- **Dependency Injection**: Register services in appropriate layers

### Pull Request Process
1. **Description**: Clear description of changes and motivation
2. **Tests**: Include comprehensive test coverage
3. **Documentation**: Update relevant documentation
4. **Review**: Address code review feedback
5. **Merge**: Squash commits for clean history

## Troubleshooting

### Common Development Issues
- **Build Errors**: Ensure .NET 8 SDK installed
- **Test Failures**: Check provider configuration and connectivity
- **PowerShell Issues**: Verify PowerShell installation for syntax validation
- **Editor Integration**: Check editor availability and PATH configuration

### Debug Configuration
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Debug Please Console",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/src/Presentation/Please.Console/bin/Debug/net8.0/please.dll",
            "args": ["create a script to list files"],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false
        }
    ]
}
```

## Future Development

### Planned Enhancements
- **Provider Caching**: Cache responses for repeated requests
- **Batch Processing**: Multiple script generation in single request
- **Streaming Responses**: Real-time streaming for large scripts
- **Policy Engine**: Organizational security policy enforcement

### Extension Points
- **AI Providers**: Interface for adding new providers
- **Validation Rules**: Configurable security patterns
- **Editor Support**: Pluggable editor detection
- **UI Themes**: Customizable console appearance

### Performance Optimizations
- **Startup Time**: Reduce cold start latency
- **Memory Usage**: Optimize memory allocation patterns
- **Network Efficiency**: Implement connection pooling and retry policies
- **Caching Strategy**: Intelligent response caching

This development guide ensures consistent, high-quality contributions while maintaining the security and performance standards of the Please project.