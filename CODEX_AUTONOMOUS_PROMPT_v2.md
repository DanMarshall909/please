# CODEX v2.0: C# Result Pattern Implementation

## 🎯 **CURRENT AUTONOMOUS DIRECTIVE**

**MISSION**: Implement single executable C# application with Result pattern and strongly typed IDs
**TARGET**: 2-5MB cross-platform executable using .NET 8 AOT
**STRATEGY**: Replace multi-project MediatR solution with single project direct services

---

## 🚨 **IMMEDIATE FOCUS: Single Project Result Pattern Foundation**

### **Phase 1 Objectives (CURRENT)**
1. **Create single Please project** (replace multi-project structure)
2. **Implement Result<T> pattern** with comprehensive test coverage  
3. **Add strongly typed ID infrastructure** with validation
4. **Remove MediatR dependencies** entirely
5. **TDD approach**: Write tests first, then implementation

### **Success Criteria for Phase 1**
- [ ] Single project builds successfully
- [ ] Result<T> pattern with 85%+ test coverage
- [ ] Strongly typed IDs working correctly
- [ ] All existing tests still pass
- [ ] No MediatR references remaining

---

## 🏗️ **IMPLEMENTATION PRIORITIES**

### **Step 1: Result Pattern Foundation (45 min)**
```csharp
// Create src/Please/Domain/Common/Result.cs
public abstract record Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; init; } = string.Empty;
    
    public static Result Success() => new SuccessResult();
    public static Result Failure(string error) => new FailureResult(error);
}

public sealed record Result<T> : Result
{
    public T? Value { get; init; }
    
    public static Result<T> Success(T value) => new() 
    { IsSuccess = true, Value = value };
    
    public static Result<T> Failure(string error) => new() 
    { IsSuccess = false, Error = error };
}
```

### **Step 2: Strongly Typed IDs (30 min)**
```csharp
// Create src/Please/Domain/Common/StronglyTypedId.cs
public abstract record StronglyTypedId<T>(T Value) 
    where T : IComparable<T>, IEquatable<T>
{
    public override string ToString() => Value?.ToString() ?? string.Empty;
    public static implicit operator T(StronglyTypedId<T> id) => id.Value;
}

// Create src/Please/Domain/Entities/ScriptId.cs
public sealed record ScriptId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ScriptId New() => new(Guid.NewGuid());
    public static ScriptId From(string value) => new(Guid.Parse(value));
}
```

### **Step 3: Entity Migration with TDD (45 min)**
```csharp
// First write tests for ScriptResponse.Create()
[Test]
public void Test_script_response_with_valid_data_creates_successfully()
{
    // Arrange
    var script = "echo 'hello'";
    var taskDescription = "Print greeting";
    var provider = ProviderType.OpenAI;
    var model = "gpt-4";
    
    // Act
    var result = ScriptResponse.Create(script, taskDescription, provider, model);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value!.Script, Is.EqualTo(script));
}

// Then implement the factory method
public static Result<ScriptResponse> Create(
    string script, string taskDescription, 
    ProviderType provider, string model)
{
    if (string.IsNullOrWhiteSpace(script))
        return Result<ScriptResponse>.Failure("Script cannot be empty");
        
    return Result<ScriptResponse>.Success(new ScriptResponse 
    {
        Id = ScriptId.New(),
        Script = script,
        TaskDescription = taskDescription,
        Provider = provider,
        Model = model,
        CreatedAt = DateTime.UtcNow
    });
}
```

### **Step 4: Single Project Structure (20 min)**
```
src/Please/
├── Domain/
│   ├── Common/Result.cs
│   ├── Common/StronglyTypedId.cs
│   ├── Entities/ScriptId.cs
│   ├── Entities/ScriptRequest.cs
│   ├── Entities/ScriptResponse.cs
│   ├── Enums/(existing enums)
│   └── Interfaces/(updated interfaces)
├── Application/Services/ScriptService.cs
├── Infrastructure/(providers, repositories)
├── Presentation/(console app)
└── Program.cs
```

---

## 🧪 **TESTING STRATEGY**

### **TDD Cycle (Mandatory)**
1. **RED**: Write failing test for Result pattern behavior
2. **GREEN**: Implement minimal code to make test pass
3. **REFACTOR**: Improve implementation while keeping tests green
4. **COVER**: Verify 85%+ coverage maintained

### **Test Examples Required (Plain English Naming)**
```csharp
[Test] public void Test_result_success_contains_value()
[Test] public void Test_result_failure_contains_error_message()
[Test] public void Test_script_id_generates_unique_guid()
[Test] public void Test_script_response_has_correct_properties()
[Test] public void Test_invalid_input_returns_failure_result()
```

### **C# Test Naming Guidelines (Enterprise Craftsmanship)**
- **No rigid naming policy** - allow freedom for complex behaviors
- **Name as describing to a non-programmer** familiar with the domain
- **Separate words with underscores** for improved readability
- **Don't include method names** - focus on behavior, not implementation
- **Use plain English** - avoid "should", prefer "is" or action verbs
- **Add articles** like "a", "the" for natural language flow
- **State facts** - tests verify behavior that exists

### **Coverage Commands**
```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet build && dotnet test
```

---

## 🚫 **CONSTRAINTS & RULES**

### **Must Follow**
- **TDD First**: No implementation without tests
- **Result Pattern**: All operations return Result<T>
- **Strongly Typed IDs**: Use ScriptId, never Guid directly
- **Single Project**: No multi-assembly complexity
- **No MediatR**: Direct service dependencies only

### **Must Avoid**
- **Exception-based errors**: Use Result.Failure() instead
- **Primitive obsession**: Wrap IDs in strongly typed wrappers
- **Large dependencies**: Keep executable size minimal
- **Complex abstractions**: Direct, simple service calls

---

## 📊 **SUCCESS METRICS**

### **Phase 1 Complete When**
- [ ] `dotnet build` succeeds on single project
- [ ] `dotnet test` shows 85%+ coverage
- [ ] Result<T> pattern working correctly
- [ ] Strongly typed IDs implemented
- [ ] No MediatR references remain

### **Quality Gates**
- **Build Time**: <30 seconds
- **Test Execution**: <5 seconds  
- **Code Coverage**: 85%+ minimum
- **Executable Size**: <50MB (before AOT optimization)

---

## 🔄 **AUTONOMOUS EXECUTION RULES**

### **Decision Making**
1. **Always choose TDD** over speed of implementation
2. **Always choose Result pattern** over exceptions
3. **Always choose strongly typed IDs** over primitives
4. **Always choose simple services** over complex abstractions

### **When Stuck**
1. **Write the test first** - clarifies requirements
2. **Start with simplest implementation** - can refactor later
3. **Check existing Go code** - for specification reference
4. **Follow SOLID principles** - but keep it simple

### **Commit Strategy**
- **Commit after each step** (Result pattern, IDs, entity migration)
- **Meaningful commit messages** describing what was implemented
- **Working state only** - all tests passing

---

## 🎯 **DELIVERABLE**

**Working single project C# application with:**
- Result<T> pattern for all operations
- Strongly typed IDs throughout
- Comprehensive test coverage (85%+)
- Single executable capability
- Foundation for direct service implementation

**Timeline**: 2-3 hours of focused implementation following TDD principles

**Next Phase**: Once foundation complete, implement direct service layer and infrastructure.
