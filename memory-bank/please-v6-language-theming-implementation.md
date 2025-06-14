# Please v6 - Language & Theming System Implementation

## 🎯 **IMPLEMENTATION STATUS**: 🚀 **PHASE 1 IN PROGRESS**

**DATE**: June 14, 2025  
**OBJECTIVE**: Implement comprehensive language and theming system using TDD  
**APPROACH**: Coverage-first refactoring with clean separation of concerns  

---

## 📋 **IMPLEMENTATION PLAN**

### **Phase 1: Pre-Refactoring Coverage Safety Net**
**CRITICAL REQUIREMENT**: 95%+ coverage before any refactoring

**Current Coverage Baseline**:
- `ui` package: 44.7% → **Target: 95%+**
- `localization` package: 89.2% ✅ (will be renamed to `language`)

**Files Requiring Coverage Enhancement**:
1. **ui/banner.go** → 95%+ (ASCII art, success messages, animations)
2. **ui/help.go** → 95%+ (complex help text with formatting)  
3. **ui/colors.go** → 95%+ (color constants and schemes)
4. **ui/interactive.go** → 95%+ (user interaction flows)

### **Phase 2: Architecture Restructure**
**Clean Separation of Concerns**:

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
├── persistence.go  # User settings persistence
└── integration_test.go
```

### **Phase 3: Configuration File Design**

**User Settings** (`~/.please/config.json`):
```json
{
  "language": "en-silly",
  "theme": "rainbow",
  "provider": "ollama",
  "preferred_model": "llama3.2"
}
```

**Language Pack** (`~/.please/languages/en-silly.json`):
```json
{
  "metadata": {
    "name": "English (Silly)",
    "code": "en-silly",
    "version": "1.0.0",
    "author": "Please Team"
  },
  "messages": {
    "banner": {
      "title": "Please - Your Overly Helpful Digital Assistant",
      "subtitle": "Politely Silly AI-Powered Cross-Platform Script Generation",
      "ascii_art": [
        "██████╗ ██╗     ███████╗ █████╗ ███████╗███████╗",
        "██╔══██╗██║     ██╔════╝██╔══██╗██╔════╝██╔════╝",
        "██████╔╝██║     █████╗  ███████║███████╗█████╗  ",
        "██╔═══╝ ██║     ██╔══╝  ██╔══██║╚════██║██╔══╝  ",
        "██║     ███████╗███████╗██║  ██║███████║███████╗",
        "╚═╝     ╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝"
      ]
    },
    "installation": {
      "success": "🎉 Installation complete! 🎉",
      "try_command": "🚀 Try it out:",
      "command_examples": [
        "pls create a hello world script",
        "ol create a hello world script (legacy)"
      ],
      "magic_text": "✨ Magic happens with just 3 letters: 'pls' ✨"
    },
    "help": {
      "title": "🤖 Please - Your Overly Helpful Digital Assistant",
      "subtitle": "✨ Politely Silly AI-Powered Cross-Platform Script Generation",
      "usage_title": "📖 Natural Language Usage:",
      "usage_examples": [
        "list all files older than 10 years",
        "backup my documents folder",
        "\"quoted strings work too\""
      ],
      "examples_title": "🎯 Examples:",
      "command_examples": [
        "list all files in the current directory",
        "create a backup script for my documents",
        "find and delete temporary files",
        "monitor system memory usage",
        "show system information"
      ],
      "setup_title": "⚙️ Setup Commands:",
      "test_title": "🧪 Test Monitoring:",
      "features_title": "🎨 Features:",
      "features": [
        "🌍 Cross-platform (Windows PowerShell, Linux/macOS Bash)",
        "🧠 Multiple AI providers (Ollama, OpenAI, Anthropic)",
        "📋 Smart model selection (automatically picks best model)",
        "🛡️ Safety first (always shows script before execution)",
        "🎯 Multiple output options (clipboard, execute, save)"
      ]
    },
    "footer": {
      "tips_title": "💡 Tips:",
      "tips": [
        "• Use natural language - no quotes needed!",
        "• Be specific for better results",
        "• Always review scripts before execution",
        "• Set PLEASE_PROVIDER=openai for OpenAI",
        "• Set PLEASE_PROVIDER=anthropic for Claude"
      ],
      "happy_scripting": "🌟 Happy scripting! 🌟"
    }
  }
}
```

**Theme Pack** (`~/.please/themes/rainbow.json`):
```json
{
  "metadata": {
    "name": "Rainbow",
    "code": "rainbow",
    "version": "1.0.0",
    "author": "Please Team"
  },
  "colors": {
    "reset": "\u001b[0m",
    "bold": "\u001b[1m",
    "dim": "\u001b[2m",
    "primary": "\u001b[36m",
    "success": "\u001b[32m",
    "warning": "\u001b[33m",
    "error": "\u001b[31m",
    "purple": "\u001b[35m",
    "white": "\u001b[37m",
    "rainbow": [
      "\u001b[31m",
      "\u001b[33m", 
      "\u001b[32m",
      "\u001b[36m",
      "\u001b[34m",
      "\u001b[35m",
      "\u001b[37m"
    ]
  },
  "animations": {
    "banner_delay_ms": 50,
    "enable_rainbow_banner": true,
    "enable_colors": true
  }
}
```

### **Phase 4: TDD Refactoring Implementation**

**BDD Test Requirements (per test-driven-development.clinerules)**:
- Test naming: `TestWhen[Context]_Should[ExpectedBehavior]`
- Behavior-focused, not implementation-focused
- No user interaction required
- Fast execution (milliseconds)
- Deterministic results

**Refactoring Priority Order**:
1. **ui/banner.go** - Simple extraction, good starting point
2. **ui/colors.go** - Pure theme extraction  
3. **ui/help.go** - Most complex, needs careful test coverage
4. **ui/interactive.go** - Mixed content and theming

**For Each File**:
1. **RED**: Write comprehensive tests verifying current exact output
2. **GREEN**: Ensure all tests pass with hardcoded strings
3. **REFACTOR**: Extract to language/theme systems, tests still pass
4. **VERIFY**: Identical output, just loaded from config

### **Phase 5: Config Integration & Persistence**

**Enhanced config package**:
- Automatic user settings directory detection (`~/.please/`)
- Config file validation and migration
- Fallback to defaults if config missing/invalid
- Easy programmatic access: `language.Get("banner.title")`, `theme.Color("primary")`

### **Phase 6: Test Migration**

**After refactoring complete**:
- Update all existing tests to use language system
- Verify test isolation (no dependency on specific language pack)
- Add comprehensive tests for language/theme switching
- Integration tests for config persistence

---

## 🎯 **SUCCESS CRITERIA**

### **Technical Requirements**:
- **Zero Behavior Change**: Exact same output as current version
- **95%+ Coverage**: All refactored packages maintain high coverage
- **Clean Architecture**: Language ≠ Theme, single responsibility
- **User-Friendly**: JSON configs humans can easily edit
- **Persistent Settings**: User preferences automatically saved

### **Quality Gates**:
- All existing tests continue to pass
- New language/theme tests achieve 95%+ coverage
- Integration tests verify end-to-end functionality
- Performance impact minimal (< 5ms startup overhead)

### **User Experience**:
- Seamless migration from current behavior
- Easy language/theme switching via config
- Backward compatibility with existing workflows
- Clear documentation for customization

---

## 🔧 **IMPLEMENTATION WORKFLOW**

### **Step 1: Coverage Enhancement** (Current Phase)
- Achieve 95%+ coverage on ui/banner.go
- Achieve 95%+ coverage on ui/help.go  
- Achieve 95%+ coverage on ui/colors.go
- Achieve 95%+ coverage on ui/interactive.go

### **Step 2: Architecture Setup**
- Rename `localization/` → `language/`
- Create new `theme/` package
- Enhance `config/` package for persistence

### **Step 3: TDD Refactoring**
- Extract banner content using TDD
- Extract help content using TDD
- Extract color themes using TDD
- Extract interactive content using TDD

### **Step 4: Integration & Testing**
- Config persistence implementation
- End-to-end integration tests
- Migration testing with existing configs

---

## 📊 **EXPECTED OUTCOMES**

### **For Users**:
- Easy customization of language and appearance
- Professional themes for business use
- Fun themes for personal use
- Multi-language support foundation

### **For Developers**:
- Clean separation of content and presentation
- Easy addition of new languages
- Easy addition of new themes
- Maintainable, testable codebase

### **For Project**:
- Enhanced internationalization capability
- Modern configuration management
- Improved user experience
- Strong foundation for future features

---

## 🚀 **NEXT ACTIONS**

**Phase 1 - Immediate**: UI Coverage Enhancement
1. Start with `ui/banner.go` coverage improvement
2. Write comprehensive BDD tests for current behavior
3. Achieve 95%+ coverage before any refactoring
4. Continue with remaining UI files

**Estimated Timeline**: 
- Phase 1 (Coverage): 4-6 hours
- Phase 2-3 (Architecture): 3-4 hours  
- Phase 4 (Refactoring): 6-8 hours
- Phase 5-6 (Integration): 2-3 hours
- **Total**: 15-20 hours for complete implementation

---

*Started: June 14, 2025*  
*Status: PHASE 1 - UI Coverage Enhancement*
*Progress: banner, help, and colors tests complete; interactive coverage nearing 52%*
*Next: Continue expanding interactive.go tests toward 95% coverage*
