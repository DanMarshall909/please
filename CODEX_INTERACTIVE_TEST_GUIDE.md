# CODEX INTERACTIVE TEST REFACTORING GUIDE

## 🚨 CRITICAL: Interactive Test Problem Detected

### **PROBLEM IDENTIFIED:**
Current tests in `ui/interactive_more_test.go` use real terminal I/O operations:
- `os.Stdin` redirection to temp files
- `getSingleKeyInput()`, `getSingleKeyWindows()`, `getSingleKeyUnix()` 
- Functions that require actual keyboard input
- These tests are FLAKY, SLOW, and FAIL in CI environments

### **MANDATORY REFACTORING APPROACH:**

#### **Phase 1A: Extract Pure Business Logic (IMMEDIATE)**

**BEFORE continuing Phase 1 coverage, you MUST refactor interactive tests using this pattern:**

```go
// ❌ BAD - Tests real I/O mechanics
func Test_when_get_single_key_input_then_return_rune(t *testing.T) {
    tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
    tmp.WriteString("c\n")
    // ... complex setup
}

// ✅ GOOD - Tests business logic only
func Test_when_parsing_user_choice_should_return_correct_action(t *testing.T) {
    action := parseUserChoice('1')
    if action != "generate_script" {
        t.Errorf("Expected generate_script, got %s", action)
    }
}
```

#### **REFACTORING STEPS FOR EACH INTERACTIVE FUNCTION:**

**1. Extract Validation Logic:**
```go
// Extract from generateNewScript()
func validateTaskDescription(input string) ValidationResult {
    return ValidationResult{
        IsEmpty: strings.TrimSpace(input) == "",
        IsValid: len(strings.TrimSpace(input)) > 0,
        Message: getValidationMessage(input),
    }
}

// Test the validation, not the I/O
func Test_when_validating_empty_task_should_detect_invalid_input(t *testing.T) {
    result := validateTaskDescription("")
    if !result.IsEmpty {
        t.Error("Expected empty task to be detected")
    }
}
```

**2. Extract Menu Choice Logic:**
```go
// Extract from interactive menu functions
func parseMenuChoice(choice rune) MenuAction {
    switch choice {
    case '1': return MenuAction{Type: "generate", Valid: true}
    case '2': return MenuAction{Type: "run_last", Valid: true}
    default: return MenuAction{Type: "invalid", Valid: false}
    }
}

func Test_when_parsing_menu_choice_1_should_return_generate_action(t *testing.T) {
    action := parseMenuChoice('1')
    if action.Type != "generate" || !action.Valid {
        t.Errorf("Expected valid generate action, got %+v", action)
    }
}
```

**3. Extract Message Generation:**
```go
// Extract from display functions
func generateStatusMessage(task string, isEmpty bool) string {
    if isEmpty {
        return "❌ No task description provided. Please enter a task."
    }
    return fmt.Sprintf("🔄 Generating script for: %s", task)
}

func Test_when_generating_status_for_empty_task_should_show_warning(t *testing.T) {
    msg := generateStatusMessage("", true)
    if !strings.Contains(msg, "No task description provided") {
        t.Errorf("Expected warning message, got: %s", msg)
    }
}
```

#### **DEPENDENCY INJECTION PATTERN (For Complex Cases):**

```go
// Create testable interface
type UserInputReader interface {
    ReadLine() (string, error)
    ReadKey() (rune, error)
}

// Production implementation
type TerminalReader struct{}
func (t TerminalReader) ReadLine() (string, error) { /* real stdin */ }
func (t TerminalReader) ReadKey() (rune, error) { /* real keyboard */ }

// Test implementation
type MockInputReader struct {
    Lines    []string
    Keys     []rune
    LineIndex int
    KeyIndex  int
}
func (m *MockInputReader) ReadLine() (string, error) {
    if m.LineIndex < len(m.Lines) {
        line := m.Lines[m.LineIndex]
        m.LineIndex++
        return line, nil
    }
    return "", io.EOF
}

// Refactor functions to accept interface
func generateNewScriptWithReader(reader UserInputReader) *types.ScriptResponse {
    input, err := reader.ReadLine()
    if err != nil {
        return &types.ScriptResponse{Error: "Failed to read input"}
    }
    
    validation := validateTaskDescription(input)
    if !validation.IsValid {
        return &types.ScriptResponse{Error: validation.Message}
    }
    
    // ... business logic without I/O
    return &types.ScriptResponse{TaskDescription: input}
}

// Test becomes clean and fast
func Test_when_generating_script_with_empty_input_should_return_error(t *testing.T) {
    mockReader := &MockInputReader{Lines: []string{""}}
    result := generateNewScriptWithReader(mockReader)
    
    if result.Error == "" {
        t.Error("Expected error for empty input")
    }
    if !strings.Contains(result.Error, "No task description") {
        t.Errorf("Expected task description error, got: %s", result.Error)
    }
}
```

#### **FUNCTIONS TO COMPLETELY SKIP TESTING:**

**❌ DO NOT attempt to test these I/O functions:**
- `getSingleKeyInput()`
- `getSingleKeyWindows()`  
- `getSingleKeyUnix()`
- `ShowMainMenu()` (if it blocks for user input)
- `ShowScriptMenu()` (if it blocks for user input)
- Any function that calls `fmt.Scanf()`, `bufio.Scanner.Scan()`, or blocks for keyboard

**✅ INSTEAD test their extracted business logic:**
- Choice parsing logic
- Validation logic  
- Message generation logic
- State transition logic

#### **SPECIFIC REFACTORING FOR ui/interactive_more_test.go:**

**Current problematic tests to refactor:**

1. **`Test_when_get_single_key_*` functions** → Extract choice parsing logic
2. **`Test_when_generate_new_script_*` functions** → Extract validation and message generation
3. **`Test_when_save_to_file_*` functions** → Extract file naming and validation logic

**Example refactoring:**

```go
// BEFORE - Tests I/O mechanics
func Test_when_generate_new_script_with_empty_input_then_show_warning(t *testing.T) {
    tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
    tmp.WriteString("\n")
    // ... complex setup
}

// AFTER - Tests business logic
func Test_when_validating_empty_task_input_should_return_warning_message(t *testing.T) {
    result := validateTaskInput("")
    expected := "❌ No task description provided. Please enter a task."
    if result.Message != expected {
        t.Errorf("Expected warning message, got: %s", result.Message)
    }
}

func Test_when_validating_valid_task_input_should_return_success_message(t *testing.T) {
    result := validateTaskInput("test task")
    if !result.IsValid {
        t.Error("Expected valid task to be accepted")
    }
    expected := "🔄 Generating script for: test task"
    if result.Message != expected {
        t.Errorf("Expected success message, got: %s", result.Message)
    }
}
```

#### **TYPES TO CREATE:**

```go
// Add to types/types.go or create types/ui.go
type ValidationResult struct {
    IsValid bool
    IsEmpty bool
    Message string
}

type MenuAction struct {
    Type    string // "generate", "run_last", "help", "exit", "invalid"
    Valid   bool
    Command string
}

type UserInputReader interface {
    ReadLine() (string, error)
    ReadKey() (rune, error)
}
```

#### **UPDATED COVERAGE TARGET:**

**Realistic UI Coverage Goal**: 90%+ of **testable business logic**
- Focus on pure functions and extracted logic
- Skip OS-dependent I/O operations
- Test decision making, not mechanics

#### **COMMIT STRATEGY:**

```bash
# Commit 1: Extract business logic functions
git commit -m "Phase 1A: ui/interactive - Extract testable business logic from I/O operations"

# Commit 2: Add comprehensive tests for extracted logic  
git commit -m "Phase 1A: ui/interactive - Add BDD tests for validation and parsing logic"

# Commit 3: Refactor remaining functions with dependency injection
git commit -m "Phase 1A: ui/interactive - Implement dependency injection for complex I/O functions"
```

#### **SUCCESS CRITERIA UPDATED:**

- ✅ No tests depend on real terminal I/O
- ✅ All business logic extracted and tested
- ✅ Fast test execution (< 100ms per test)
- ✅ Tests pass consistently in CI environment
- ✅ 90%+ coverage of testable business logic
- ✅ Zero flaky tests

#### **EXECUTION ORDER:**

1. **IMMEDIATE**: Refactor `ui/interactive_more_test.go` using patterns above
2. **NEXT**: Apply same patterns to other interactive test files
3. **THEN**: Continue with Phase 1 coverage for non-interactive UI files
4. **FINALLY**: Proceed to Phase 2 localization implementation

**CRITICAL**: Complete this refactoring BEFORE continuing with Phase 1 coverage work on other UI files. This establishes the pattern for testable interactive code throughout the project.

---

## **IMPLEMENTATION CHECKLIST:**

- [ ] Extract validation logic from `generateNewScript()`
- [ ] Extract menu choice parsing from interactive functions
- [ ] Extract message generation logic
- [ ] Create `ValidationResult` and `MenuAction` types
- [ ] Implement `UserInputReader` interface for complex cases
- [ ] Replace problematic I/O tests with business logic tests
- [ ] Verify all tests run fast (< 100ms each)
- [ ] Achieve 90%+ coverage of testable business logic
- [ ] Commit refactored code before proceeding to other UI files

**This refactoring is MANDATORY before continuing with the localization implementation.**
