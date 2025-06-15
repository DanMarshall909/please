# Agents Development Guide for Please v6

## 🎯 **CRITICAL PROJECT RULES**

### **Test-Driven Development Protocol (MANDATORY)**
- **85%+ Coverage Required** for new C# development
- **Plain English Test Naming**: Focus on behavior in natural language
- **Red-Green-Refactor-Cover Cycle**: Always write failing tests first
- **Coverage Verification**: `dotnet test --collect:"XPlat Code Coverage"` must show 85%+ 
- **NO IMPLEMENTATION** until Result pattern tests are in place

### **Current Project Status**
- **Phase**: C# Result Pattern Migration (Single Executable Focus)
- **Strategy Pivot**: From Go enhancement to C# clean architecture 
- **Target**: 2-5MB cross-platform executable with Result<T> pattern
- **Next Step**: Implement single project structure with Result pattern

---

## 📋 **ACTIVE TASK: C# Result Pattern Architecture Implementation**

### **Strategic Decision (June 15, 2025)**
**DECISION**: Pursue C# Result Pattern single executable over Go UI testing enhancement
**RATIONALE**: 
- Higher ROI on modern architecture vs legacy enhancement
- Single executable deployment requirement favors C# AOT
- Result pattern + strongly typed IDs provide better maintainability
- Cross-platform 2-5MB target achievable with .NET 8 trimming

### **Implementation Phases**
1. **PHASE 1** (CURRENT): Single Project Structure Setup
2. **PHASE 2**: Result Pattern & Strongly Typed ID Implementation  
3. **PHASE 3**: Domain Entity Migration with TDD
4. **PHASE 4**: Direct Service Layer (No MediatR)
5. **PHASE 5**: Infrastructure & Console Application
6. **PHASE 6**: AOT Build Optimization & Cross-Platform Testing

### **Target Architecture**
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
├── Application/Services/
│       └── ScriptService.cs        # Direct service (no MediatR)
├── Infrastructure/
│   ├── Providers/                  # AI provider implementations
│   ├── Repositories/               # File-based storage
│   └── DependencyInjection.cs     # Simple DI setup
├── Presentation/
│   ├── Commands/                   # CLI command handlers
│   └── ConsoleApp.cs              # Command parsing & execution
└── Program.cs                     # Entry point + DI configuration
```

---

## 🚦 **CURRENT FOCUS AREAS**

### **Core Pattern Implementation Priority**
1. **Result<T> Pattern** - Explicit error handling foundation
2. **Strongly Typed IDs** - Type-safe entity identification  
3. **Domain Entity Migration** - Convert existing entities with tests
4. **Direct Services** - Replace MediatR with simple service calls
5. **Single Project Structure** - Merge multi-project solution

### **Legacy Go Codebase Status**
- **DEFERRED**: Go UI testing and localization enhancement
- **MAINTAINED**: Existing Go functionality preserved in `legacy/` folder
- **REFERENCE**: Use Go implementation as specification for C# migration

---

## 🧪 **TESTING STANDARDS (C# Focus)**

### **Plain English Test Format (Required for C#)**
```csharp
[Test]
public void Test_user_with_valid_credentials_can_login()
{
    // Arrange
    var credentials = SetupValidCredentials();
    
    // Act
    var result = AuthenticationService.Login(credentials);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value, Is.EqualTo(expectedUser));
}
```

### **C# Test Naming Guidelines (Following Enterprise Craftsmanship)**
- **No rigid naming policy** - allow freedom for complex behaviors
- **Name as describing to a non-programmer** familiar with the domain
- **Separate words with underscores** for improved readability  
- **Don't include method names** - focus on behavior, not implementation
- **Use plain English** - avoid "should", prefer "is" or action verbs
- **Add articles** like "a", "the" for natural language flow
- **State facts** - tests verify behavior that exists

### **Examples of Good C# Test Names**
- `Test_script_response_with_empty_content_is_invalid`
- `Test_user_with_expired_token_cannot_access_api`
- `Test_result_success_contains_expected_value`
- `Test_strongly_typed_id_converts_to_underlying_value`

### **Coverage Verification Commands**
```bash
# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage.xml" -targetdir:"coveragereport"

# Build and test pipeline
dotnet build && dotnet test
```

### **What to Test (Business Logic)**
- ✅ **Result Pattern** - Success/failure scenarios
- ✅ **Entity Creation** - Factory methods with validation
- ✅ **Service Operations** - Input/output behavior
- ✅ **Strongly Typed IDs** - Type safety and conversion

### **What NOT to Test (Infrastructure)**
- ❌ **DI Container** - Framework functionality
- ❌ **JSON Serialization** - System.Text.Json behavior
- ❌ **File I/O** - Use mocks for repositories
- ❌ **Console Output** - Test logic, not presentation

---

## 📁 **KEY PROJECT FILES**

### **Multi-Agent Coordination**
- `memory-bank/multi-agent-workflow-strategy.md` - **CRITICAL** CODEX + CLINE + dRAGster coordination strategy
- `AGENTS.md` - Primary agent coordination document (this file)

### **C# Architecture Documentation**
- `memory-bank/please-v6-csharp-result-pattern-architecture.md` - **PRIMARY** implementation plan
- `memory-bank/codex-execution-strategy-v2.md` - Strategic decision documentation
- `memory-bank/strategic-testing-c#-migration-progress.md` - Progress tracking

### **Legacy Go Documentation** (Reference Only)
- `memory-bank/please-v6-language-theming-implementation.md` - Go enhancement plan (deferred)
- `CODEX_AUTONOMOUS_PROMPT.md` - Original Go autonomous plan
- `CODEX_CORRECTIVE_PROMPT.md` - Go UI testing requirements

### **Configuration & Rules**
- `C:/Users/danma/OneDrive/Documents/Cline/Rules/test-driven-development.clinerules` - TDD requirements
- `C:/Users/danma/OneDrive/Documents/Cline/Rules/development-environment.clinerules` - C# + Rider preferences

### **Current Implementation Files**
- `src/` - C# project structure (in progress)
- `legacy/` - Go implementation (reference/preserved)
- `Please.sln` - Solution file for C# development

---

## ⚙️ **DEVELOPMENT ENVIRONMENT**

### **Platform**: Windows 11
- **Shell**: Command Prompt (cmd.exe)  
- **IDE Primary**: JetBrains Rider (for C# development)
- **IDE Secondary**: VS Code with Cline (for exploration/AI assistance)
- **.NET Version**: 8.0 (for AOT support)

### **Commands for Development**
```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Publish optimized executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishAot=true

# Cross-platform builds
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishAot=true
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishAot=true
```

---

## 🎯 **SUCCESS CRITERIA**

### **Phase 1 (Current) - Single Project Setup**
- [ ] Create unified Please project structure (single project)
- [ ] Implement Result<T> pattern with comprehensive tests
- [ ] Add strongly typed IDs with validation
- [ ] Remove MediatR dependencies entirely
- [ ] Basic project builds and tests pass

### **Phase 2 - Domain Migration**
- [ ] Migrate ScriptRequest with Result pattern
- [ ] Migrate ScriptResponse with Result pattern  
- [ ] Update all interfaces to use Result<T>
- [ ] Achieve 85%+ test coverage on domain layer

### **Phase 3 - Service Layer**
- [ ] Implement ScriptService with direct dependencies
- [ ] Add Infrastructure layer implementations
- [ ] Console application with basic functionality
- [ ] End-to-end script generation working

### **Phase 4 - Build Optimization**
- [ ] AOT compilation working across platforms
- [ ] Single file output under 5MB
- [ ] Cross-platform executables (Windows/Linux/macOS)
- [ ] Performance benchmarks meet requirements

### **Overall Project Goals**
- **Single Executable**: 2-5MB cross-platform deployment
- **Result Pattern**: Explicit error handling throughout
- **Type Safety**: Strongly typed IDs prevent runtime errors
- **No MediatR**: Direct service dependencies for minimal overhead
- **Clean Architecture**: Maintained separation of concerns

---

## 🚨 **IMMEDIATE ACTIONS REQUIRED**

### **For Current Agent (Codex)**
1. **FOCUS** on Result pattern implementation first
2. **CREATE** single project structure replacing multi-project solution
3. **WRITE** comprehensive tests for Result<T> and strongly typed IDs
4. **MIGRATE** existing entities using TDD approach
5. **VERIFY** builds produce small executables

### **Development Priority Order**
1. Result<T> pattern implementation with tests
2. Strongly typed ID base classes
3. ScriptResponse entity migration
4. ScriptRequest entity migration  
5. Service layer with direct dependencies

---

## 🤖 **MULTI-AGENT WORKFLOW COORDINATION**

### **CRITICAL**: See `memory-bank/multi-agent-workflow-strategy.md` for detailed coordination strategy

### **Agent Responsibilities**:

**🤖 CODEX - Development Implementation**:
- 2-3 hour autonomous coding sessions
- TDD with 85%+ coverage implementation
- Result<T> pattern and strongly typed ID development
- Following `CODEX_AUTONOMOUS_PROMPT_v2.md`

**📋 CLINE - Strategic Coordination**:
- Architecture planning and quality gates
- Documentation updates and progress tracking
- Multi-phase coordination and review
- Global rules and workflow management

**🧠 dRAGster - Intelligence Enhancement** (Integration Target):
- RAG-powered context enhancement for Please commands
- User behavior learning and intelligent defaults
- Command pattern storage and retrieval
- Progressive improvement of script generation

### **Current Workflow Phase**: Foundation (Result Pattern Implementation)
- **CODEX Focus**: Implement Result<T> with comprehensive tests
- **CLINE Focus**: Monitor quality gates and plan Phase 2
- **dRAGster Integration**: Prepare IContextService interface

---

## 📞 **ESCALATION PROCESS**

### **Multi-Agent Coordination Issues**:
- Refer to `memory-bank/multi-agent-workflow-strategy.md`
- Clear handoff protocols between CODEX and CLINE
- Documentation-driven communication via memory-bank updates

### **When AOT Compilation Issues Arise**:
- Document specific AOT compatibility problems
- Research .NET 8 AOT limitations and workarounds
- Consider alternative approaches for problematic dependencies
- Test minimal reproduction cases

### **When Single Executable Size Exceeds Target**:
- Analyze executable size with .NET tools
- Identify largest dependencies for elimination
- Consider feature reduction for size optimization
- Document trade-offs between features and size

### **dRAGster Integration Challenges**:
- Start with basic IContextService interface
- Implement pattern storage before retrieval
- Focus on command-heavy scenarios for dogfooding
- Prioritize transparency and user control

---

## 📈 **PROGRESS TRACKING**

### **Current Status**: Phase 1 (Foundation - Result Pattern Implementation)
- ✅ Strategic decision made (C# over Go enhancement)
- ✅ Architecture plan documented
- ✅ Multi-agent workflow strategy defined
- ✅ Multi-project foundation exists
- ⚠️ **Next**: CODEX implements Result<T> pattern with tests
- ❌ Result pattern implementation pending
- ❌ dRAGster integration pending

### **Key Metrics**:
- **Code Coverage**: Target 85%+ (C# tests)
- **Executable Size**: Target 2-5MB
- **Build Time**: Target <30 seconds
- **Test Suite**: Target <5 seconds execution
- **Agent Coordination**: Clear handoffs, documentation updates

---

*This document serves as the definitive guide for all agents working on Please v6 C# migration with dRAGster integration.*  
*Last Updated: June 15, 2025*  
*Current Phase: 1 - Foundation (Result Pattern Implementation)*  
*See: `memory-bank/multi-agent-workflow-strategy.md` for detailed coordination protocols*
