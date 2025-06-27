# Contributing Guide

## Overview

Please welcomes contributions to improve AI-powered script generation, security validation, and user experience.

## Getting Started

### Prerequisites
- .NET 8 SDK
- Git
- PowerShell (for syntax validation)
- Preferred editor (VS Code, Visual Studio, Rider)

### Setup
```bash
# Fork and clone repository
git clone https://github.com/yourusername/please.git
cd please

# Build and test
dotnet build
dotnet test

# Verify natural language interface
dotnet build src/Presentation/Please.Console -c Release
./please create a test script
```

## Development Process

### Test-Driven Development
- **Write failing tests first** (Red phase)
- **Implement minimal code** to pass tests (Green phase)
- **Refactor and improve** code quality (Refactor phase)
- **Verify coverage** with `dotnet test --collect:"XPlat Code Coverage"`
- **Commit changes** with clear commit messages

### Code Quality Standards
- **Zero warnings**: Project treats warnings as errors
- **90%+ test coverage**: Comprehensive testing required
- **Enterprise test naming**: Plain English, behavior-focused
- **Clean Architecture**: Respect layer boundaries
- **Security focus**: Always consider security implications

## Architecture Guidelines

### Layer Responsibilities
- **Domain**: Pure business logic, zero dependencies
- **Application**: Use cases and workflows
- **Infrastructure**: External dependencies and implementations
- **Presentation**: User interface and interaction

### Design Patterns
- **Result Pattern**: Use `Result<T>` for error handling
- **Dependency Injection**: Register services in appropriate layers
- **Provider Factory**: For AI provider creation and selection
- **Validation Pipeline**: Syntax → Security → Auto-fix → Re-validation

## Areas for Contribution

### AI Provider Integration
- Add new AI service providers
- Improve existing provider implementations
- Enhance error handling and retry logic
- Implement streaming response support

### Security Enhancements
- Expand security pattern detection
- Improve risk assessment accuracy
- Add new script type support
- Enhance validation rules

### User Experience
- Improve console UI components
- Add new editor integrations
- Enhance progress indicators
- Implement additional output formats

### Performance Optimizations
- Reduce startup time
- Optimize memory usage
- Improve response caching
- Enhance Native AOT compatibility

## Contribution Workflow

### 1. Issue Creation
- Search existing issues to avoid duplicates
- Use issue templates for bug reports and feature requests
- Provide clear reproduction steps for bugs
- Include relevant system information

### 2. Development
```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make changes following TDD approach
# Write tests first, then implementation
# Ensure all tests pass

# Commit with clear messages
git commit -m "Add: feature description with clear intent"
```

### 3. Testing
```bash
# Run all tests
dotnet test

# Test specific features
dotnet test --filter="YourFeatureName"

# Verify coverage
dotnet test --collect:"XPlat Code Coverage"

# Manual testing with real scenarios
./please test your new feature functionality
```

### 4. Pull Request
- Create descriptive pull request title
- Include clear description of changes
- Reference related issues
- Ensure CI checks pass
- Address code review feedback

## Testing Guidelines

### Unit Tests
- Test individual components in isolation
- Use mocking for external dependencies
- Focus on behavior, not implementation details
- Follow Arrange-Act-Assert pattern

### Integration Tests
- Test complete workflows end-to-end
- Use real AI providers when configured
- Verify error handling and edge cases
- Test cross-platform compatibility

### Security Tests
- Validate security pattern detection
- Test auto-fix functionality
- Verify risk assessment accuracy
- Test malicious script prevention

## Code Review Process

### Reviewer Checklist
- [ ] Tests cover new functionality
- [ ] No build warnings or errors
- [ ] Documentation updated appropriately
- [ ] Security implications considered
- [ ] Performance impact assessed
- [ ] Cross-platform compatibility verified

### Feedback Guidelines
- Provide constructive, specific feedback
- Suggest improvements with examples
- Focus on code quality and maintainability
- Respect contributor efforts and time

## Documentation Standards

### Code Documentation
- Use XML comments for public APIs
- Include usage examples for complex methods
- Document security considerations
- Explain design decisions for non-obvious code

### User Documentation
- Update relevant markdown files
- Include practical examples
- Test documentation accuracy
- Consider different user skill levels

## Security Considerations

### Secure Development
- Validate all inputs rigorously
- Use secure coding practices
- Never commit secrets or API keys
- Follow principle of least privilege

### Security Testing
- Test for injection vulnerabilities
- Verify input sanitization
- Test error handling paths
- Validate security pattern detection

## Support and Questions

### Getting Help
- Check existing documentation first
- Search GitHub issues for similar questions
- Join discussions for design questions
- Contact maintainers for urgent security issues

### Community Guidelines
- Be respectful and inclusive
- Help other contributors
- Share knowledge and best practices
- Provide constructive feedback

Thank you for contributing to Please! Your efforts help make AI-powered script generation safer and more accessible.