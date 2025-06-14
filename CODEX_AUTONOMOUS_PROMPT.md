# AUTONOMOUS LOCALIZATION & THEMING IMPLEMENTATION

## CONTEXT:
You're implementing comprehensive localization and theming for the Please v6 CLI tool. Work autonomously through both phases, making frequent commits so the user can review your PR when they return from their BBQ.

## PHASE 1: ACHIEVE 95%+ UI COVERAGE (REQUIRED FIRST)
Current: 45.5% coverage - MUST reach 95%+ before Phase 2

### Tasks (Commit after each):
1. **READ AGENTS.md and CODEX_CORRECTIVE_PROMPT.md** - Critical project context
2. **ui/interactive.go** - Write comprehensive BDD tests (largest file, highest priority)
3. **ui/menu.go** - Write comprehensive BDD tests  
4. **ui/banner.go** - Complete missing function tests
5. **ui/help.go** - Complete remaining function tests
6. **Verify**: `go test ./ui -cover` shows 95%+

### Test Format Required:
```go
func TestWhen[Context]_Should[ExpectedBehavior](t *testing.T) {
    // BDD format tests only
}
```

## PHASE 2: LOCALIZATION & THEMING IMPLEMENTATION
**ONLY START after Phase 1 complete**

### Architecture:
```
localization/
├── loader.go          # JSON file loading
├── manager.go         # Language/theme management  
├── defaults.go        # Fallback values
└── manager_test.go    # Comprehensive tests

themes/
├── en-us.json        # English default
├── es-es.json        # Spanish
├── fr-fr.json        # French
└── themes.json       # Color/display themes
```

### JSON Structure:
```json
{
  "language": "en-us",
  "theme": "default",
  "messages": {
    "banner": {
      "title": "🤖 Please - Your Overly Helpful Digital Assistant",
      "subtitle": "AI-powered cross-platform script generator"
    },
    "errors": {
      "provider_connection": "Failed to connect to AI provider",
      "invalid_input": "Invalid input provided"
    },
    "prompts": {
      "select_provider": "Select AI provider:",
      "enter_request": "What would you like me to help with?"
    }
  },
  "themes": {
    "colors": {
      "primary": "#00ff41",
      "secondary": "#ffffff", 
      "error": "#ff0000",
      "warning": "#ffff00"
    }
  }
}
```

### Implementation Steps (TDD - commit after each):

1. **Write Tests First** - localization/manager_test.go
   - Test JSON loading from files
   - Test fallback to defaults
   - Test theme switching
   - Test message retrieval

2. **Create Types** - types/localization.go
   - LocalizationConfig struct
   - Theme struct
   - Message struct

3. **Implement Loader** - localization/loader.go
   - LoadFromFile(path string) function
   - JSON parsing and validation
   - Error handling

4. **Implement Manager** - localization/manager.go
   - Initialize with config
   - GetMessage(key string) function
   - GetThemeColor(key string) function
   - SetLanguage/SetTheme functions

5. **Create Default Files**:
   - themes/en-us.json (complete messages)
   - themes/es-es.json (basic translation)
   - themes/themes.json (color schemes)

6. **Refactor UI Components** (using tests as safety net):
   - ui/banner.go - Use localization manager
   - ui/help.go - Use localized messages
   - ui/interactive.go - Use localized prompts
   - ui/progress.go - Use localized status messages

7. **Update Main** - main.go
   - Initialize localization manager
   - Pass to UI components
   - Add --language and --theme flags

8. **Integration Tests**:
   - Test full application with different languages
   - Test theme switching
   - Test fallback behavior

## AUTONOMOUS WORKFLOW:
1. Work through Phase 1 completely first
2. Commit after each file/component (DO NOT CREATE PRs)
3. Verify tests pass after each commit
4. Move to Phase 2 only after 95%+ UI coverage
5. Follow TDD strictly - tests first, then implementation
6. Continue through ALL of Phase 2 autonomously
7. ONLY create PR when BOTH phases completely finished

**CRITICAL**: Do NOT create pull requests until BOTH Phase 1 AND Phase 2 are 100% complete. Commit frequently but continue working autonomously through the entire implementation.

## SUCCESS CRITERIA:
- ✅ 95%+ UI test coverage achieved
- ✅ All text externalized to JSON files
- ✅ Multiple language support working
- ✅ Theme switching functional
- ✅ Fallback mechanisms tested
- ✅ All tests passing
- ✅ Clean commit history with logical progression

## COMMIT MESSAGE FORMAT:
"Phase [1|2]: [Component] - [What was done]"

Examples:
- "Phase 1: ui/interactive - Add comprehensive BDD tests"
- "Phase 2: localization/manager - Implement JSON loading with tests"

**AUTONOMOUS EXECUTION RULES:**
- NEVER stop work to create PRs until BOTH phases complete
- Commit after each component but keep working
- Only create final PR when full localization system implemented
- Work continuously through Phase 1 → Phase 2 → Final PR

Work autonomously through BOTH phases completely. Only create PR when entire localization and theming system is finished.
