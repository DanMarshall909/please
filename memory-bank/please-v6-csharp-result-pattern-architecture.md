# Please v6: C# Result Pattern Architecture

## 🎯 ARCHITECTURE OVERVIEW

**Objective**: Single executable, cross-platform CLI tool with Result pattern and strongly typed IDs
**No Dependencies**: Remove MediatR, use direct services with Result<T> pattern
**Target Size**: 2-5MB cross-platform executable

## 🏗️ REFINED ARCHITECTURE

### **Core Patterns**
- ✅ **Result Pattern**: Explicit error handling, no exceptions for business logic
- ✅ **Strongly Typed IDs**: Type-safe entity identification
- ✅ **Direct Services**: No MediatR, simple dependency injection
- ✅ **Single Project**: Folder-based organization for minimal binary size

### **Project Structure**
```
src/Please/
├── Domain/
│   ├── Common/
│   │   ├── Result.cs               # Result<T> pattern implementation
│   │   └── StronglyTypedId.cs      # Base for typed IDs
│   ├── Entities/
│   │   ├── ScriptRequest.cs        # Updated with Result pattern
│   │   ├── ScriptResponse.cs       # Updated with Result pattern
│   │   └── ScriptId.cs            # Strongly typed ID
│   ├── Enums/
│   │   ├── ProviderType.cs
│   │   ├── ScriptType.cs
│   │   └── RiskLevel.cs
│   └── Interfaces/
│       ├── IScriptGenerator.cs     # Updated with Result<T>
│       └── IScriptRepository.cs    # Updated with Result<T>
├── Application/
│   └── Services/
│       └── ScriptService.cs        # Direct service (no MediatR)
├── Infrastructure/
│   ├── Providers/
│   │   ├── OpenAIProvider.cs
│   │   ├── AnthropicProvider.cs
│   │   └── OllamaProvider.cs
│   ├── Repositories/
│   │   └── FileScriptRepository.cs # File-based storage
│   └── DependencyInjection.cs     # Simple DI setup
├── Presentation/
│   ├── Commands/
│   │   ├── GenerateCommand.cs
│   │   └── HistoryCommand.cs
│   └── ConsoleApp.cs              # Command parsing & execution
└── Program.cs                     # Entry point + DI configuration
```

## 🔧 CORE IMPLEMENTATIONS

### **1. Result Pattern**
```csharp
// Domain/Common/Result.cs
public abstract record Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; init; } = string.Empty;
    
    public static Result Success() => new SuccessResult();
    public static Result Failure(string error) => new FailureResult(error);
    
    protected Result() { }
}

public sealed record SuccessResult : Result
{
    public SuccessResult() { IsSuccess = true; }
}

public sealed record FailureResult(string Error) : Result
{
    public FailureResult(string error) : base() 
    { 
        IsSuccess = false; 
        Error = error; 
    }
}

public sealed record Result<T> : Result
{
    public T? Value { get; init; }
    
    public static Result<T> Success(T value) => new() 
    { 
        IsSuccess = true, 
        Value = value 
    };
    
    public static Result<T> Failure(string error) => new() 
    { 
        IsSuccess = false, 
        Error = error 
    };
    
    // Railway-oriented programming extensions
    public Result<TNext> Map<TNext>(Func<T, TNext> map)
    {
        return IsSuccess 
            ? Result<TNext>.Success(map(Value!))
            : Result<TNext>.Failure(Error);
    }
    
    public async Task<Result<TNext>> MapAsync<TNext>(Func<T, Task<TNext>> map)
    {
        return IsSuccess 
            ? Result<TNext>.Success(await map(Value!))
            : Result<TNext>.Failure(Error);
    }
}
```

### **2. Strongly Typed IDs**
```csharp
// Domain/Common/StronglyTypedId.cs
public abstract record StronglyTypedId<T>(T Value) 
    where T : IComparable<T>, IEquatable<T>
{
    public override string ToString() => Value?.ToString() ?? string.Empty;
    
    public static implicit operator T(StronglyTypedId<T> id) => id.Value;
}

// Domain/Entities/ScriptId.cs
public sealed record ScriptId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ScriptId New() => new(Guid.NewGuid());
    public static ScriptId From(string value) => new(Guid.Parse(value));
    public static ScriptId Empty => new(Guid.Empty);
}

public sealed record ProviderId(string Value) : StronglyTypedId<string>(Value)
{
    public static ProviderId OpenAI => new("openai");
    public static ProviderId Anthropic => new("anthropic");
    public static ProviderId Ollama => new("ollama");
}
```

### **3. Updated Domain Entities**
```csharp
// Domain/Entities/ScriptResponse.cs
public sealed class ScriptResponse
{
    public ScriptId Id { get; private set; }
    public string Script { get; private set; }
    public string TaskDescription { get; private set; }
    public ProviderType Provider { get; private set; }
    public string Model { get; private set; }
    public ScriptType ScriptType { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public List<string> Warnings { get; private set; } = new();
    public List<string> SafetyNotes { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    private ScriptResponse() { } // EF Constructor
    
    public static Result<ScriptResponse> Create(
        string script,
        string taskDescription,
        ProviderType provider,
        string model,
        ScriptType scriptType = ScriptType.Bash,
        RiskLevel riskLevel = RiskLevel.Low)
    {
        if (string.IsNullOrWhiteSpace(script))
            return Result<ScriptResponse>.Failure("Script content cannot be empty");
            
        if (string.IsNullOrWhiteSpace(taskDescription))
            return Result<ScriptResponse>.Failure("Task description cannot be empty");
            
        if (string.IsNullOrWhiteSpace(model))
            return Result<ScriptResponse>.Failure("Model cannot be empty");

        return Result<ScriptResponse>.Success(new ScriptResponse
        {
            Id = ScriptId.New(),
            Script = script.Trim(),
            TaskDescription = taskDescription.Trim(),
            Provider = provider,
            Model = model.Trim(),
            ScriptType = scriptType,
            RiskLevel = riskLevel,
            CreatedAt = DateTime.UtcNow
        });
    }
    
    public ScriptResponse WithWarning(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning))
            Warnings.Add(warning);
        return this;
    }
    
    public ScriptResponse WithSafetyNote(string note)
    {
        if (!string.IsNullOrWhiteSpace(note))
            SafetyNotes.Add(note);
        return this;
    }
    
    public bool RequiresConfirmation => 
        RiskLevel >= RiskLevel.Medium || Warnings.Any();
        
    public bool IsDangerous => RiskLevel == RiskLevel.High;
}
```

### **4. Service Layer (No MediatR)**
```csharp
// Application/Services/IScriptService.cs
public interface IScriptService
{
    Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request, 
        CancellationToken cancellationToken = default);
        
    Task<Result<ScriptResponse>> GetScriptAsync(
        ScriptId id, 
        CancellationToken cancellationToken = default);
        
    Task<Result<IEnumerable<ScriptResponse>>> GetRecentScriptsAsync(
        int count = 10, 
        CancellationToken cancellationToken = default);
}

// Application/Services/ScriptService.cs
public sealed class ScriptService : IScriptService
{
    private readonly IScriptGenerator _generator;
    private readonly IScriptRepository _repository;
    
    public ScriptService(IScriptGenerator generator, IScriptRepository repository)
    {
        _generator = generator;
        _repository = repository;
    }
    
    public async Task<Result<ScriptResponse>> GenerateScriptAsync(
        ScriptRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var generationResult = await _generator.GenerateScriptAsync(request, cancellationToken);
            if (generationResult.IsFailure)
                return Result<ScriptResponse>.Failure(generationResult.Error);
                
            var saveResult = await _repository.SaveScriptAsync(generationResult.Value!, cancellationToken);
            if (saveResult.IsFailure)
                return Result<ScriptResponse>.Failure($"Failed to save script: {saveResult.Error}");
                
            return Result<ScriptResponse>.Success(generationResult.Value!);
        }
        catch (Exception ex)
        {
            return Result<ScriptResponse>.Failure($"Script generation failed: {ex.Message}");
        }
    }
}
```

## 🎯 MIGRATION STRATEGY

### **Phase 1: Infrastructure Setup (30 min)**
1. **Create single Please project** (replace multi-project solution)
2. **Implement Result pattern** and strongly typed IDs
3. **Remove MediatR dependencies** entirely

### **Phase 2: Domain Migration (45 min)**
1. **Update entities** with Result pattern
2. **Convert interfaces** to use Result<T>
3. **Add strongly typed IDs** throughout

### **Phase 3: Service Layer (30 min)**
1. **Replace Command/Query handlers** with direct services
2. **Implement ScriptService** with Result pattern
3. **Update dependency injection** for simple DI

### **Phase 4: Build Optimization (15 min)**
1. **Configure AOT publishing**
2. **Enable trimming and single-file output**
3. **Test cross-platform builds**

## 🔧 BUILD CONFIGURATION

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <PublishAot>true</PublishAot>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>link</TrimMode>
    <RuntimeIdentifiers>win-x64;linux-x64;osx-x64;osx-arm64</RuntimeIdentifiers>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
    <!-- Minimal dependencies only -->
  </ItemGroup>
</Project>
```

## 📊 EXPECTED BENEFITS

### **Technical Benefits**
- ✅ **Small Binary**: 2-5MB (vs 30-50MB with MediatR)
- ✅ **Fast Startup**: Direct service calls, no reflection
- ✅ **Explicit Errors**: Result pattern eliminates hidden exceptions
- ✅ **Type Safety**: Strongly typed IDs prevent ID mix-ups

### **Development Benefits**
- ✅ **Testability**: Easy to mock Result<T> patterns
- ✅ **Maintainability**: Simple, direct service calls
- ✅ **Railway Programming**: Chain operations safely
- ✅ **Cross-Platform**: Single codebase, multiple targets

## 🧪 TESTING STRATEGY

### **TDD Approach** (Following test-driven-development.clinerules)
1. **RED**: Write failing test for Result pattern
2. **GREEN**: Implement minimal code to pass
3. **REFACTOR**: Improve while maintaining tests
4. **COVER**: Verify 85%+ coverage maintained

### **Test Examples**
```csharp
[Test]
public void ScriptResponse_Create_WithValidInput_ShouldReturnSuccess()
{
    // Arrange
    var script = "echo 'hello'";
    var task = "Print greeting";
    var provider = ProviderType.OpenAI;
    var model = "gpt-4";
    
    // Act
    var result = ScriptResponse.Create(script, task, provider, model);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value!.Script, Is.EqualTo(script));
    Assert.That(result.Value.TaskDescription, Is.EqualTo(task));
}

[Test]
public void ScriptResponse_Create_WithEmptyScript_ShouldReturnFailure()
{
    // Act
    var result = ScriptResponse.Create("", "task", ProviderType.OpenAI, "gpt-4");
    
    // Assert
    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Contains.Substring("Script content cannot be empty"));
}
```

## 🚀 IMMEDIATE NEXT STEPS

1. **Create new single project structure**
2. **Implement Result pattern and strongly typed IDs**
3. **Migrate existing entities with comprehensive tests**
4. **Replace MediatR with direct service calls**
5. **Configure optimized build for small executable**

**Timeline**: 2 hours to working single executable with Result pattern implementation.
