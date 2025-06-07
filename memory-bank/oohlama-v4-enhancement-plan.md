# OohLama v4.0 - Advanced User Experience Enhancement Plan

## Overview
Building on the successful modular refactoring of v3.0, OohLama v4.0 focuses on advanced user experience features including script refinement, comprehensive safety warnings, immediate editing capabilities, and execution history tracking.

## 🎯 New Feature Requirements

### 1. 🔄 Feedback & Script Refinement System
**Objective:** Allow users to iteratively improve generated scripts through AI-powered refinement

**Features:**
- **Interactive Feedback Collection**: New menu option "🔄 Give feedback and refine script"
- **Iterative Improvement**: Multiple rounds of refinement until user satisfaction
- **Contextual Refinement**: AI understands original task, current script, and user feedback
- **Refinement Templates**: Guided feedback prompts for better results

**Implementation Plan:**
- Create `script/feedback.go` package
- Enhance Provider interface to support refinement requests
- Add refinement workflow to interactive menu
- Implement feedback prompt system with examples

**User Workflow:**
1. Generate initial script
2. Review script and identify issues/improvements needed
3. Select "Give feedback and refine"
4. Provide specific feedback (e.g., "make it more robust", "add error handling")
5. AI generates improved version based on feedback
6. Repeat until satisfied

### 2. ⚠️ Enhanced Warning System
**Objective:** Provide comprehensive, clearly explained safety warnings with severity levels

**Features:**
- **Severity Levels**: 🟢 Info, 🟡 Caution, 🔴 Danger, ⛔ Critical
- **Detailed Explanations**: Clear descriptions of why commands are flagged
- **Risk Categories**: System modification, network operations, privilege escalation, data destruction
- **Safety Recommendations**: Alternative approaches when possible
- **Explicit Acknowledgment**: Required confirmation for high-risk operations

**Enhanced Detection:**
```
⛔ CRITICAL: rm -rf / (Complete system deletion)
🔴 DANGER: shutdown -h now (System shutdown) 
🟡 CAUTION: chmod 777 (Overly permissive permissions)
🟢 INFO: Consider adding error handling
```

**Implementation Plan:**
- Expand `script/validation.go` (rename from ValidateScript)
- Create warning severity classification system
- Add detailed warning explanations and alternatives
- Implement acknowledgment workflow for dangerous operations
- Enhanced visual presentation with colors and clear formatting

### 3. ✏️ Script Editing & Browser Viewing
**Objective:** Enable users to edit scripts and view them with syntax highlighting

**Features:**
- **Pre-execution Editing**: Offer editing before script execution
- **Multiple Editor Options**: System default, simple terminal editor, line-by-line editing
- **Cross-platform Support**: notepad (Windows), nano/vim (Linux/macOS)
- **Browser-based Viewing**: Generate HTML with syntax highlighting and open in default browser
- **Validation After Edit**: Re-run safety checks on modified scripts
- **Edit History**: Track changes made to scripts

**Browser Viewing Features:**
- **Syntax Highlighting**: PowerShell and Bash syntax highlighting using highlight.js or Prism.js
- **Clean HTML Generation**: Professional layout with script metadata
- **Copy to Clipboard**: JavaScript-based clipboard functionality in browser
- **Print Support**: CSS optimized for printing
- **Responsive Design**: Works on desktop and mobile browsers
- **Dark/Light Themes**: Automatic theme detection or user preference

**Implementation Plan:**
- Create `script/editor.go` package for editing functionality
- Create `script/browser.go` package for HTML generation and browser integration
- Implement platform-specific editor detection and launching
- Add HTML template system with embedded CSS/JavaScript
- Integrate browser viewing into interactive menu
- Add post-edit validation

**User Workflows:**

**Editing Workflow:**
1. Generate script
2. Choose "Execute script now"
3. System offers: "Would you like to edit the script first? (y/n)"
4. If yes, opens editor of choice
5. After editing, re-validates and shows changes
6. Confirms execution of modified script

**Browser Viewing Workflow:**
1. Generate script
2. Choose "🌐 View in browser"
3. System generates HTML file with syntax highlighting
4. Opens in default browser automatically
5. User can copy, print, or save from browser
6. Returns to interactive menu for further actions

### 4. 📚 Script Execution History
**Objective:** Maintain comprehensive history of all executed scripts with full traceability

**Features:**
- **Persistent Storage**: `~/.oohlama/history/` directory structure
- **Rich Metadata**: Timestamp, task description, AI model used, exit codes
- **Execution Results**: Track success/failure, output, errors
- **History Management**: View, search, delete old entries, export
- **Quick Re-execution**: Run previous scripts again
- **Learning Integration**: Use history to improve future script generation

**History Structure:**
```
~/.oohlama/
├── history/
│   ├── scripts/
│   │   ├── 2025-06-07_17-45-23_list_files.ps1
│   │   ├── 2025-06-07_17-46-12_backup_docs.sh
│   │   └── 2025-06-07_18-00-45_network_scan.sh
│   ├── execution_log.json
│   └── settings.json
└── config.json
```

**Execution Log Entry:**
```json
{
  "id": "2025-06-07_17-45-23",
  "timestamp": "2025-06-07T17:45:23Z",
  "task_description": "list files in current directory",
  "script_type": "powershell",
  "provider": "ollama",
  "model": "deepseek-coder:6.7b",
  "script_file": "2025-06-07_17-45-23_list_files.ps1",
  "execution_result": {
    "executed": true,
    "exit_code": 0,
    "duration_ms": 234,
    "output_lines": 15,
    "errors": []
  },
  "warnings_acknowledged": ["🟡 No error handling"],
  "modifications": {
    "edited": false,
    "refinement_rounds": 0
  }
}
```

**Implementation Plan:**
- Create `script/history.go` package
- Implement history storage and retrieval
- Add history viewing and management interface
- Integrate history logging into execution workflow
- Create history-based learning system

## 🔄 Enhanced Interactive Menu

**Updated Menu Options:**
```
🎯 What would you like to do with this script?

1. 📋 Copy to clipboard
2. 🔄 Give feedback and refine script
3. ✏️  Edit script
4. ▶️  Execute script now
5. 💾 Save to file
6. 🌐 View in browser (with syntax highlighting)
7. 📜 View execution history
8. 📖 Show detailed explanation
9. 🚪 Exit
```

**New Browser Viewing Option Details:**
- **🌐 View in browser**: Generates a professional HTML page with syntax highlighting
  - **Syntax Highlighting**: Uses highlight.js or Prism.js for PowerShell/Bash syntax
  - **Professional Layout**: Clean, responsive design with script metadata
  - **Interactive Features**: Browser-based copy to clipboard, print support
  - **Theme Support**: Dark/light themes with automatic detection
  - **Cross-platform**: Opens in default browser on Windows, macOS, Linux

## 🏗️ Architecture Enhancements

### New Package Structure
```
oohlama/
├── script/
│   ├── operations.go      # Existing: clipboard, file ops, execution
│   ├── feedback.go        # NEW: Feedback collection and refinement
│   ├── editor.go          # NEW: Script editing functionality
│   ├── history.go         # NEW: Execution history and logging
│   └── validation.go      # ENHANCED: Advanced warning system
├── ui/
│   ├── interactive.go     # ENHANCED: Updated menu with new options
│   └── [existing files]
└── [existing packages]
```

### Enhanced Provider Interface
```go
type Provider interface {
    GenerateScript(request *ScriptRequest) (*ScriptResponse, error)
    RefineScript(request *RefinementRequest) (*ScriptResponse, error) // NEW
    Name() string
    IsConfigured(config *Config) bool
}

type RefinementRequest struct {
    OriginalTask    string
    CurrentScript   string
    UserFeedback    string
    PreviousRounds  []RefinementRound
    ScriptType      string
    Provider        string
    Model           string
}
```

## 🧪 Testing Strategy

### Feature Testing
- **Feedback System**: Test refinement iterations with various feedback types
- **Warning System**: Validate all warning categories and severity levels
- **Editor Integration**: Test cross-platform editor launching and file handling
- **History System**: Verify storage, retrieval, and management operations

### Safety Testing
- **Dangerous Command Detection**: Comprehensive test suite for risky operations
- **Permission Validation**: Test acknowledgment workflows
- **Script Modification**: Ensure edited scripts are properly re-validated

### User Experience Testing
- **Workflow Integration**: Test seamless flow between all menu options
- **Error Handling**: Graceful handling of editor failures, history corruption
- **Performance**: Ensure history storage doesn't impact generation speed

## 📈 Success Metrics

### User Experience
- **Refinement Usage**: % of users who use feedback refinement feature
- **Safety Compliance**: Reduction in dangerous script executions
- **Edit Adoption**: % of users who edit scripts before execution
- **History Utilization**: % of users who reference execution history

### Safety Improvements
- **Warning Effectiveness**: User response rates to different warning levels
- **Risk Reduction**: Decreased execution of high-risk operations
- **User Education**: Improved understanding of script safety

## 🚀 Implementation Roadmap

### Phase 1: Enhanced Safety (Week 1)
- Implement comprehensive warning system
- Add severity levels and detailed explanations
- Test dangerous command detection

### Phase 2: Script Editing (Week 2)  
- Create editor integration system
- Implement pre-execution editing workflow
- Add post-edit validation

### Phase 3: Execution History (Week 3)
- Build history storage and management
- Create history viewing interface
- Integrate logging into execution flow

### Phase 4: Feedback & Refinement (Week 4)
- Implement feedback collection system
- Add refinement workflow to providers
- Create iterative improvement interface

### Phase 5: Integration & Polish (Week 5)
- Integrate all features into cohesive menu system
- Comprehensive testing and bug fixes
- Documentation and user guides

## 🎯 Next Actions

1. **Start with Enhanced Warning System** - Critical for user safety
2. **Implement Script Editing** - High user value, moderate complexity
3. **Build Execution History** - Foundation for learning and tracking
4. **Add Feedback System** - Most complex, highest potential impact

This enhancement plan transforms OohLama from a script generator into a comprehensive script development and management platform while maintaining its ease of use and safety focus.
