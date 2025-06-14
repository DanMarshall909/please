# Agents Development Guide for Please v6

## 🎯 **CRITICAL PROJECT RULES**

### **Test-Driven Development Protocol (MANDATORY)**
- **95%+ Coverage Required** BEFORE any refactoring
- **BDD Test Naming**: `TestWhen[Context]_Should[ExpectedBehavior]`
- **Red-Green-Refactor-Cover Cycle**: Always write failing tests first
- **Coverage Verification**: `go test -cover ./...` must show 95%+ before proceeding
- **NO REFACTORING** until coverage safety net is complete

### **Current Project Status**
- **Phase**: Language & Theming Implementation
- **Current Coverage**: UI package at 45.5% (NEEDS: 95%+)
- **Blocker**: Cannot proceed to refactoring until coverage requirements met
- **Next Step**: Complete comprehensive UI testing

---

## 📋 **ACTIVE TASK: Language & Theming System**

### **Implementation Phases**
1. **PHASE 1** (CURRENT): UI Coverage Safety Net - 95%+ required
2. **PHASE 2**: Architecture Restructure (language/ + theme/ packages)  
3. **PHASE 3**: Configuration File Design
4. **PHASE 4**: TDD Refactoring Implementation
5. **PHASE 5**: Config Integration & Persistence
6. **PHASE 6**: Test Migration

### **Architecture Plan**
```
language/           # Renamed from localization/
├── manager.go      # Language pack loading & selection
├── loader.go       # JSON file loading  
├── defaults.go     # Built-in English (silly)
└── manager_test.go # Comprehensive tests

theme/              # NEW - Separate concern
├── manager.go      # Theme loading & selection
├── colors.go       # Color scheme definitions
├── defaults.go     # Built-in themes  
└── manager_test.go # Comprehensive tests

config/             # Enhanced
├── language.go     # Language-specific config
├── theme.go        # Theme-specific config
├── persistence.go  # User settings persistence (~/.please/config.json)
└── integration_test.go
```

---

## 🚦 **CURRENT BLOCKING ISSUES**

### **Coverage Requirements Not Met**
- **UI Package**: 45.5% actual vs 95%+ required
- **Files Needing Tests**:
  - `ui/banner.go` - ASCII art, success messages (incomplete)
  - `ui/help.go` - Help text display (improved but insufficient)  
  - `ui/colors.go` - Color constants (started, needs completion)
  - `ui/interactive.go` - User interaction flows (not started)
  - `ui/menu.go` - Menu rendering (not started)
  - `ui/progress.go` - Progress indicators (existing tests, may need more)

### **TDD Rule Violations**
- ⛔ **CRITICAL**: 95% coverage required before ANY refactoring
- ⛔ **CRITICAL**: Must test existing behavior before extraction
- ⛔ **CRITICAL**: Cannot proceed to Phase 2 until Phase 1 complete

---

## 🧪 **TESTING STANDARDS**

### **BDD Test Format (Required)**
```go
func TestWhen[Context]_Should[ExpectedBehavior](t *testing.T) {
    // Arrange
    input := setupTestData()
    
    // Act
    result := FunctionToTest(input)
    
    // Assert
    if result != expected {
        t.Errorf("Expected %v, got %v", expected, result)
    }
}
```

### **Coverage Verification Commands**
```bash
# Check package coverage
go test ./ui -cover

# Generate detailed coverage report  
go test ./ui -coverprofile=ui_coverage.out
go tool cover -html=ui_coverage.out

# Coverage by function
go tool cover -func=ui_coverage.out
```

### **What to Test (Business Logic)**
- ✅ **Function behavior** - what it does
- ✅ **Input/output validation** - edge cases
- ✅ **Error handling** - failure scenarios
- ✅ **Integration points** - component interactions

### **What NOT to Test (Cosmetic)**
- ❌ **Color codes** - visual formatting  
- ❌ **ASCII art** - decorative elements
- ❌ **Text alignment** - spacing/positioning
- ❌ **Font styles** - appearance

---

## 📁 **KEY PROJECT FILES**

### **Documentation**
- `memory-bank/please-v6-language-theming-implementation.md` - Full implementation plan
- `README.md` - Project overview
- `AGENTS.md` - This file (agent guidance)

### **Configuration & Rules**
- `please-v5-cleanup-tracker.clinerules` - Active cleanup rules
- `C:/Users/danma/OneDrive/Documents/Cline/Rules/test-driven-development.clinerules` - TDD requirements
- `C:/Users/danma/OneDrive/Documents/Cline/Rules/` - Global cline rules directory

### **Core Implementation Files**
- `ui/` - UI package requiring 95%+ coverage
- `localization/` - Current 89.2% coverage (will rename to `language/`)
- `config/` - Configuration management
- `types/` - Type definitions

---

## ⚙️ **DEVELOPMENT ENVIRONMENT**

### **Platform**: Windows 11
- **Shell**: Command Prompt (cmd.exe)
- **Go Version**: 1.24.4
- **IDE**: VS Code with Cline + JetBrains Rider

### **Commands for Development**
```bash
# Run tests with coverage
go test ./... -cover

# Build project
go build

# Run specific package tests
go test ./ui -v

# Coverage analysis
go test ./ui -coverprofile=coverage.out && go tool cover -html=coverage.out
```

---

## 🎯 **SUCCESS CRITERIA**

### **Phase 1 (Current) - Coverage Requirements**
- [ ] UI package achieves 95%+ coverage
- [ ] All UI functions have comprehensive BDD tests
- [ ] Tests verify current behavior (no regressions)
- [ ] Coverage report shows green across all files

### **Overall Project Goals**
- **Zero Behavior Change**: Exact same output, just configurable
- **Clean Architecture**: Language ≠ Theme, single responsibility  
- **User-Friendly Config**: JSON files humans can easily edit
- **Persistent Settings**: User preferences automatically saved
- **Safe Refactoring**: High coverage enables confident changes

---

## 🚨 **IMMEDIATE ACTIONS REQUIRED**

### **For Current Agent (Codex)**
1. **STOP** all refactoring work immediately
2. **FOCUS** exclusively on achieving 95%+ UI coverage
3. **WRITE** comprehensive BDD tests for ALL UI functions
4. **VERIFY** coverage with `go test ./ui -cover` 
5. **PROCEED** to Phase 2 only after 95%+ coverage achieved

### **Coverage Priority Order**
1. `ui/interactive.go` - Most complex, user interaction flows
2. `ui/menu.go` - Menu rendering and display
3. `ui/banner.go` - Complete all banner functions
4. `ui/help.go` - Complete all help functions
5. `ui/colors.go` - Complete color constant testing

---

## 📞 **ESCALATION PROCESS**

### **When Coverage Cannot Be Achieved**
- Document specific functions that cannot be tested
- Explain why (external dependencies, platform-specific, etc.)
- Propose alternative testing strategies
- Get explicit approval before proceeding

### **When TDD Rules Conflict**
- TDD rules take precedence over speed
- Safety and quality over delivery timeline
- Document any rule conflicts for review
- Never compromise on 95% coverage requirement

---

*This document serves as the definitive guide for all agents working on Please v6.*  
*Last Updated: June 14, 2025*  
*Current Phase: 1 - UI Coverage Safety Net*
