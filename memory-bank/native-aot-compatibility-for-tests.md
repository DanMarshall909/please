# Native AOT Compatibility for Tests

## Overview

This document outlines the strategies and patterns implemented to ensure test compatibility with Native AOT (Ahead-of-Time) compilation in the Please application. Native AOT compilation requires all types to be known at compile time, which affects how dependency injection and test doubles are configured.

## Key Principles

1. **Explicit Registration**: All dependencies must be explicitly registered with the DI container
2. **Concrete Types First**: Register concrete types before interfaces
3. **Direct Interface Mapping**: Use direct mapping from interfaces to concrete instances
4. **Avoid Factory Methods**: Minimize use of factory methods that rely on runtime reflection
5. **Complete Dependency Chain**: Ensure all dependencies in the chain are registered

## Implementation Details

### TestSystem.cs Changes

The `TestSystem.cs` file was updated to use direct registration of interfaces instead of factory methods:

```csharp
public static class TestSystem
{
    public static IServiceProvider Create(Action<IServiceCollection>? configure = null)
    {
        return PleaseHost.CreateServiceProvider(services =>
        {
            // Register test doubles with explicit interface implementations for AOT compatibility
            services.AddSingleton<FakeScriptGenerator>();
            services.AddSingleton<FakeScriptRepository>();
            services.AddSingleton<FakeContextService>();

            // Use direct registration instead of factory methods for AOT compatibility
            services.AddSingleton<IScriptGenerator, FakeScriptGenerator>();
            services.AddSingleton<IScriptRepository, FakeScriptRepository>();
            services.AddSingleton<IContextService, FakeContextService>();

            services.AddLogging(builder => builder.AddDebug());
            configure?.Invoke(services);
        });
    }
}
```

### Test Class Updates

All test classes were updated to follow these patterns:

1. **PleaseHostTests.cs**: Properly registers all required dependencies
2. **ScriptServiceTests.cs**: Explicitly configures test doubles
3. **CommandProcessorTests.cs**: Explicitly configures test doubles
4. **ScriptGenerationIntegrationTests.cs**: Includes required FakeContextService
5. **CommandProcessorIntegrationTests.cs**: Registers FakeScriptRepository and sets proper context

Example from ScriptServiceTests.cs:

```csharp
public ScriptServiceTests()
{
    // Create a test service provider with explicit configuration for AOT compatibility
    var provider = TestSystem.Create(services =>
    {
        // Ensure the test doubles are properly configured
        services.AddSingleton<FakeScriptGenerator>();
        services.AddSingleton<FakeScriptRepository>();
        services.AddSingleton<FakeContextService>();

        // Register interfaces with their implementations
        services.AddSingleton<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
        services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
        services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
    });

    _generator = provider.GetRequiredService<FakeScriptGenerator>();
    _repository = provider.GetRequiredService<FakeScriptRepository>();
    _service = provider.GetRequiredService<IScriptService>();
}
```

### Test Doubles Configuration

Test doubles were updated to match expected error messages and behavior:

1. **FakeScriptGenerator.cs**: Default error message set to "nope"
2. **FakeContextService.cs**: Default error message set to "no context"
3. **Integration Tests**: Context explicitly set to Success when needed

## Common Issues and Solutions

### 1. Missing Dependencies

**Problem**: Native AOT compilation fails when dependencies are missing in the DI container.

**Solution**: Ensure all dependencies are explicitly registered, even if they're not directly used in the test.

### 2. Factory Method Limitations

**Problem**: Factory methods that use runtime reflection may not work with Native AOT.

**Solution**: Use direct type registration or lambda-based factory methods that don't rely on reflection.

### 3. Interface Registration Order

**Problem**: Registering interfaces before concrete types can cause issues.

**Solution**: Always register concrete types first, then map interfaces to those instances.

### 4. Test Double Configuration

**Problem**: Default configurations may not match test expectations.

**Solution**: Explicitly configure test doubles with expected values and behaviors.

## Best Practices for Future Development

1. Always register all dependencies explicitly in test setup
2. Use concrete type registration followed by interface registration
3. Avoid relying on implicit type resolution
4. Configure test doubles with expected values
5. Ensure complete dependency chains are registered
6. Use lambda-based factory methods instead of reflection-based ones
7. Test with both JIT and AOT compilation to catch issues early
