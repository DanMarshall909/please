# Testing Guide

## Overview

Please implements comprehensive testing with 90%+ coverage across all layers using Test-Driven Development (TDD) principles.

## Testing Architecture

### Test Structure
```
tests/
├── Domain.UnitTests/           # Pure domain logic, zero dependencies
├── Application.UnitTests/      # Business workflows with mocks
├── Infrastructure.UnitTests/   # Provider implementations and services
├── Presentation.UnitTests/    # Console UI and command processing
├── Application.IntegrationTests/ # End-to-end workflows
└── TestUtilities/             # Shared test infrastructure
```

### Test Categories
- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test complete workflows with real dependencies
- **Security Tests**: Validate security features and risk assessment
- **Syntax Tests**: Verify PowerShell AST parsing and auto-fix

## Test-Driven Development Process

### TDD Cycle
1. **Red**: Write failing test that describes desired behavior
2. **Green**: Write minimal code to make test pass
3. **Refactor**: Improve code quality while keeping tests green
4. **Cover**: Verify test coverage meets standards
5. **Commit**: Commit changes with clear message

### Enterprise Test Naming
Tests use plain English describing behavior:
```csharp
[Fact]
public void Safe_powershell_script_has_low_risk_level()
{
    // Arrange: A safe PowerShell command
    string safeScript = "Get-Date | Format-Table";

    // Act: Assess the risk level
    var riskLevel = _validationService.AssessRiskLevel(safeScript, ScriptType.PowerShell);

    // Assert: Should be classified as low risk
    riskLevel.ShouldBe(RiskLevel.Low);
}
```

## Running Tests

### All Tests
```bash
# Run complete test suite
dotnet test

# Run with coverage collection
dotnet test --collect:"XPlat Code Coverage"

# Run with detailed output
dotnet test --verbosity normal
```

### Specific Test Categories
```bash
# Unit tests only
dotnet test tests/Domain.UnitTests/
dotnet test tests/Application.UnitTests/
dotnet test tests/Infrastructure.UnitTests/
dotnet test tests/Presentation.UnitTests/

# Integration tests
dotnet test tests/Application.IntegrationTests/

# Specific feature tests
dotnet test --filter="Auto_fix_corrects_nonexistent_cmdlets"
dotnet test --filter="External_editor_functionality"
dotnet test --filter="Security_validation"
```

### Test Configuration
Tests automatically handle environment setup:
- Mock services for unit tests
- Real providers for integration tests (when configured)
- Secure test data management
- Cross-platform compatibility

## Unit Testing Patterns

### Arrange-Act-Assert Structure
```csharp
[Fact]
public void Generated_script_with_syntax_errors_is_detected()
{
    // Arrange: Create script with known syntax errors
    string invalidScript = "Get-Date -Format \"unclosed quote";
    
    // Act: Validate syntax using native parser
    var syntaxErrors = _validationService.ValidateSyntax(invalidScript, ScriptType.PowerShell);
    
    // Assert: Should detect syntax errors
    syntaxErrors.ShouldNotBeEmpty();
}
```

### Builder Pattern for Test Data
```csharp
var response = ScriptResponseBuilder.Create()
    .WithScript("Get-Process")
    .WithProvider(ProviderType.OpenAi)
    .WithRiskLevel(RiskLevel.Low)
    .Build();
```

### Mocking with NSubstitute
```csharp
var mockProvider = Substitute.For<IAiProvider>();
mockProvider.GenerateScriptAsync(Arg.Any<string>())
    .Returns(Result<string>.Success("Get-Date"));
```

## Integration Testing

### Real Provider Testing
Integration tests use real AI providers when configured:
```csharp
[Fact]
public async Task Can_generate_script_with_real_openai_provider()
{
    // Requires OPENAI_API_KEY environment variable
    var request = ScriptRequest.Create("list running processes");
    var result = await _scriptService.GenerateScriptAsync(request);
    
    result.IsSuccess.ShouldBeTrue();
    result.Value.Script.ShouldNotBeNullOrEmpty();
}
```

### End-to-End Workflows
```csharp
[Fact]
public async Task Complete_script_generation_workflow_succeeds()
{
    // Test: Natural language → AI generation → Validation → Response
    var taskDescription = "create a script to list files";
    var request = ScriptRequest.Create(taskDescription);
    
    var result = await _scriptService.GenerateScriptAsync(request);
    var validatedResponse = _validationService.EnhanceWithValidation(result.Value);
    
    validatedResponse.Script.ShouldNotBeNullOrEmpty();
    validatedResponse.RiskLevel.ShouldBeDefined();
}
```

## Security Testing

### Risk Assessment Validation
```csharp
[Fact]
public void Script_with_file_deletion_has_high_risk()
{
    string dangerousScript = "Remove-Item -Path 'C:\\temp\\*' -Recurse -Force";
    var riskLevel = _validationService.AssessRiskLevel(dangerousScript, ScriptType.PowerShell);
    riskLevel.ShouldBe(RiskLevel.High);
}
```

### Auto-Fix Testing
```csharp
[Fact]
public void Auto_fix_corrects_nonexistent_cmdlets()
{
    string scriptWithBadCmdlet = "$computerName = Get-ComputerName";
    var syntaxErrors = new List<string> { "Cmdlet 'Get-ComputerName' does not exist" };
    
    var fixedScript = _validationService.AutoFixSyntaxErrors(
        scriptWithBadCmdlet, ScriptType.PowerShell, syntaxErrors);
    
    fixedScript.ShouldBe("$computerName = $env:COMPUTERNAME");
}
```

### Corrupted AI Response Detection
```csharp
[Fact]
public void Script_with_corrupted_ai_tokens_returns_critical_warning()
{
    string corruptedScript = "if ($LastExitCode -ne <|begin▁of▁sentence|>) { Write-Host 'Error' }";
    var warnings = _validationService.ValidateScript(corruptedScript, ScriptType.PowerShell);
    
    warnings.ShouldContain(warning => 
        warning.Contains("CRITICAL") && warning.Contains("Corrupted"));
}
```

## Syntax Validation Testing

### PowerShell AST Parser Testing
```csharp
[Fact]
public void PowerShell_native_parser_detects_actual_syntax_errors()
{
    string syntaxErrorScript = @"
        Write-Host ""Hello world
        if ($true -and ($false
        {
            Write-Host 'Test'
        }";
    
    var syntaxErrors = _validationService.ValidateSyntax(syntaxErrorScript, ScriptType.PowerShell);
    
    syntaxErrors.Count.ShouldBeGreaterThan(0);
    syntaxErrors.ShouldContain(error => error.Contains("quote") || error.Contains("string"));
}
```

## Test Utilities

### Shared Builders
- `ScriptRequestBuilder`: Create test script requests
- `ScriptResponseBuilder`: Create test script responses
- `TestModule`: Dependency injection for tests
- `ShouldlyExtensions`: Custom assertions

### Fake Implementations
- `FakeScriptGenerator`: Controllable script generation
- `FakeScriptRepository`: In-memory storage
- `TestScriptValidationService`: Simplified validation

## Coverage Requirements

### Minimum Coverage Standards
- **Domain Layer**: 95%+ (pure business logic)
- **Application Layer**: 90%+ (use cases and workflows)
- **Infrastructure Layer**: 85%+ (external integrations)
- **Presentation Layer**: 80%+ (UI and interaction)

### Coverage Analysis
```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# View detailed coverage (with reportgenerator tool)
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

## Continuous Integration

### Automated Testing
- All tests run on every push
- Coverage reports generated automatically
- Security tests validate risk assessment
- Cross-platform testing on Windows, Linux, macOS

### Quality Gates
- All tests must pass
- Coverage thresholds must be met
- No build warnings allowed
- Security tests must validate correctly

## Best Practices

### Test Organization
- Group related tests in same test class
- Use descriptive test class names
- Order tests logically within classes
- Keep test methods focused and atomic

### Test Data Management
- Use builders for complex test objects
- Avoid hardcoded strings where possible
- Use constants for shared test values
- Clean up test data appropriately

### Performance Testing
- Keep unit tests fast (< 100ms each)
- Use async/await properly in async tests
- Mock expensive operations
- Measure and optimize slow tests

### Error Testing
- Test both success and failure paths
- Verify error messages are helpful
- Test edge cases and boundary conditions
- Validate exception handling

This testing approach ensures high-quality, reliable code with comprehensive validation and security features.