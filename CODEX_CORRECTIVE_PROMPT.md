# URGENT: Complete UI Coverage to 95%+ (Phase 1)

## 🚨 **CRITICAL ISSUE IDENTIFIED**

**Current Status**: UI Coverage at 45.5% (REQUIRED: 95%+)  
**Blocker**: Cannot proceed to language/theming refactoring until coverage safety net complete  
**Action**: STOP all refactoring, FOCUS exclusively on comprehensive UI testing  

---

## ⛔ **BLOCKING REQUIREMENTS**

### **Coverage Gate**: 95%+ Required Before ANY Refactoring
Per `test-driven-development.clinerules` and `AGENTS.md`:
- **MANDATORY**: 95%+ coverage on files being refactored
- **NO EXCEPTIONS**: Safety net must be complete before extraction
- **VERIFICATION**: `go test ./ui -cover` must show 95%+

### **Current Gap Analysis**
```bash
# Current Status
go test ./ui -cover
# Result: 45.5% coverage (INSUFFICIENT)

# Target: 95%+ coverage required
# Gap: Need ~50% more coverage
```

---

## 🎯 **IMMEDIATE TASKS FOR CODEX**

### **PHASE 1 COMPLETION: Write Comprehensive BDD Tests**

**Priority Order** (most complex first):
1. **`ui/interactive.go`** - User interaction flows (NOT STARTED)
2. **`ui/menu.go`** - Menu rendering and display (NOT STARTED)  
3. **`ui/banner.go`** - Complete ALL banner functions (INCOMPLETE)
4. **`ui/help.go`** - Complete ALL help functions (PARTIALLY DONE)
5. **`ui/colors.go`** - Color constants testing (BASIC DONE)

### **Required Test Coverage by File**

#### **1. `ui/interactive.go` Tests** (PRIORITY 1 - NOT STARTED)
```go
// Create: ui/interactive_comprehensive_test.go
// Test ALL functions in interactive.go:

func TestWhenHandlingMainMenuChoice_ShouldProcessValidOptions(t *testing.T)
func TestWhenHandlingMainMenuChoice_ShouldRejectInvalidOptions(t *testing.T)
func TestWhenShowingMainMenu_ShouldDisplayAllOptions(t *testing.T)
func TestWhenGettingConfigDir_ShouldReturnPlatformSpecificPath(t *testing.T)
func TestWhenExtractingJSONField_ShouldParseValidContent(t *testing.T)
func TestWhenExtractingJSONField_ShouldHandleMalformedJSON(t *testing.T)
func TestWhenDeterminingRiskLevel_ShouldCategorizeProperly(t *testing.T)
func TestWhenSavingLastScript_ShouldPersistData(t *testing.T)
func TestWhenLoadingLastScript_ShouldRetrieveData(t *testing.T)
func TestWhenSavingToHistory_ShouldAppendEntry(t *testing.T)
// ... Cover ALL functions in interactive.go
```

#### **2. `ui/menu.go` Tests** (PRIORITY 2 - NOT STARTED)
```go
// Create: ui/menu_comprehensive_test.go
// Test ALL functions in menu.go:

func TestWhenRenderingMenu_ShouldDisplayAllOptions(t *testing.T)
func TestWhenHandlingUserInput_ShouldValidateChoices(t *testing.T)
func TestWhenHandlingEnterKey_ShouldExitGracefully(t *testing.T)
func TestWhenHandlingInvalidInput_ShouldPromptRetry(t *testing.T)
// ... Cover ALL functions in menu.go
```

#### **3. `ui/banner.go` Tests** (COMPLETE ALL FUNCTIONS)
```go
// Expand existing ui/banner_test.go
// Test EVERY function in banner.go:

func TestWhenPrintingBannerWithRainbow_ShouldApplyColors(t *testing.T)
func TestWhenPrintingInstallationMessages_ShouldShowSuccess(t *testing.T)
func TestWhenPrintingFooter_ShouldDisplayTips(t *testing.T)
func TestWhenAnimatingBanner_ShouldRespectDelay(t *testing.T)
// ... Every single function must be tested
```

#### **4. `ui/help.go` Tests** (COMPLETE REMAINING FUNCTIONS)
```go
// Expand existing ui/help_test.go
// Test ALL remaining functions:

func TestWhenDisplayingUsageExamples_ShouldShowPatterns(t *testing.T)
func TestWhenDisplayingFeatures_ShouldListCapabilities(t *testing.T)
func TestWhenDisplayingSetupCommands_ShouldShowInstallation(t *testing.T)
func TestWhenDisplayingTestMonitoring_ShouldExplainFeature(t *testing.T)
// ... Every help function must be tested
```

---

## 🧪 **BDD TESTING REQUIREMENTS**

### **Test Naming Convention** (MANDATORY)
```go
func TestWhen[Context]_Should[ExpectedBehavior](t *testing.T) {
    // Arrange
    setup()
    
    // Act  
    result := FunctionUnderTest()
    
    // Assert
    if result != expected {
        t.Errorf("Expected %v, got %v", expected, result)
    }
}
```

### **Standard Test Utilities** (USE THESE)
```go
// Stdout capture for output testing
func captureOutput(fn func()) string {
    r, w, _ := os.Pipe()
    orig := os.Stdout
    os.Stdout = w
    outC := make(chan string)
    go func() {
        var buf bytes.Buffer
        io.Copy(&buf, r)
        outC <- buf.String()
    }()
    fn()
    w.Close()
    os.Stdout = orig
    return <-outC
}

// Input injection for interactive testing
func injectInput(input string, fn func()) {
    oldStdin := os.Stdin
    r, w, _ := os.Pipe()
    os.Stdin = r
    
    go func() {
        defer w.Close()
        w.WriteString(input)
    }()
    
    fn()
    os.Stdin = oldStdin
}
```

### **What to Test** (FOCUS ON THESE)
- ✅ **Function behavior** - input/output relationships
- ✅ **Error handling** - invalid inputs, edge cases
- ✅ **Integration points** - how functions work together
- ✅ **Business logic** - decision making, validation
- ✅ **Data persistence** - file operations, config handling

### **What NOT to Test** (IGNORE THESE)
- ❌ **Color codes** - visual formatting details
- ❌ **ASCII art** - decorative elements
- ❌ **Exact spacing** - cosmetic formatting
- ❌ **Font styles** - appearance details

---

## 📊 **COVERAGE VERIFICATION PROCESS**

### **Mandatory Commands** (RUN AFTER EACH TEST FILE)
```bash
# 1. Check overall coverage improvement
go test ./ui -cover

# 2. Generate detailed coverage report
go test ./ui -coverprofile=ui_coverage.out

# 3. View coverage by function
go tool cover -func=ui_coverage.out

# 4. Generate HTML coverage report
go tool cover -html=ui_coverage.out

# 5. Verify 95%+ threshold met
go test ./ui -cover | grep "coverage:" | awk '{if($2 < 95.0) {print "FAILED: Coverage ",$2," < 95%"; exit 1} else {print "PASSED: Coverage ",$2," >= 95%"}}'
```

### **Coverage Progression Targets**
- **After interactive.go tests**: ~70%+ coverage
- **After menu.go tests**: ~85%+ coverage  
- **After banner.go completion**: ~90%+ coverage
- **After help.go completion**: ~95%+ coverage
- **FINAL TARGET**: 95%+ coverage confirmed

---

## 🚦 **SUCCESS CRITERIA**

### **Phase 1 Complete When**:
- [ ] `go test ./ui -cover` shows 95%+ coverage
- [ ] ALL functions in ui/ package have BDD tests
- [ ] ALL tests pass consistently 
- [ ] Coverage report shows no red/uncovered areas
- [ ] Tests verify current behavior (no regressions)

### **ONLY THEN** proceed to Phase 2 (Architecture Restructure)

---

## 📋 **STEP-BY-STEP EXECUTION**

### **Immediate Actions**:
1. **READ** `AGENTS.md` for full context
2. **ANALYZE** each ui/*.go file to identify untested functions
3. **WRITE** comprehensive BDD tests for each function
4. **VERIFY** coverage improvement after each test file
5. **CONTINUE** until 95%+ coverage achieved
6. **COMMIT** working state after reaching 95%+

### **Test Creation Order**:
```bash
# 1. Start with most complex
touch ui/interactive_comprehensive_test.go
# Write ALL interactive.go function tests

# 2. Continue with menu functionality  
touch ui/menu_comprehensive_test.go
# Write ALL menu.go function tests

# 3. Complete banner testing
# Expand existing ui/banner_test.go
# Add missing function tests

# 4. Complete help testing
# Expand existing ui/help_test.go  
# Add missing function tests

# 5. Verify final coverage
go test ./ui -cover
# Must show: coverage: 95.X% of statements
```

---

## 🎯 **DELIVERABLE**

**Completion Evidence Required**:
```bash
# Final verification command
go test ./ui -cover -v

# Expected output:
# === RUN TestWhen... (many tests)
# PASS
# coverage: 95.2% of statements (or higher)
# ok please/ui 2.xxx
```

**Only after seeing 95%+ coverage may you proceed to Phase 2.**

---

## 🔗 **REFERENCE FILES**

- `AGENTS.md` - Full project rules and context
- `memory-bank/please-v6-language-theming-implementation.md` - Implementation plan
- `test-driven-development.clinerules` - TDD requirements
- Existing test files: `ui/*_test.go` - Examples of BDD format

**FOCUS EXCLUSIVELY ON COVERAGE UNTIL 95%+ ACHIEVED**
