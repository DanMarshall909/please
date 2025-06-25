# Console UI Design Specification

## 🎯 DESIGN OBJECTIVE

**Immediate Goal**: Create the simplest possible working UI, then iterate to polished version  
**Philosophy**: Working tool first, polish second  
**Phase 1**: Basic text output with functional commands  
**Phase 2**: Add animations, borders, and visual polish  

## 📋 IMPLEMENTATION PHASES

### **Phase 1: Minimal Working UI (30 min)**

**Simple Text Output**:
```
> please create a PowerShell script to list files

Generating script...

Generated script:
Get-ChildItem -Path . | Format-Table

Commands: (m)odify, (q)uit, (h)elp
> _
```

**Core Features**:
- Basic text status messages
- Simple script output (no borders)
- Single-letter commands (m, q, h)
- Enter to quit
- No animations or colors initially

### **Phase 2: Polished UI (60 min)**

**Enhanced Visual Design**:
```
> please create a PowerShell script to list files

🔄 Generating script with OpenAI GPT-4... ●○○ 3s

✅ Script generated successfully (3.2s)

┌─────────────────────────────────────────┐
│ Get-ChildItem -Path . | Format-Table    │
└─────────────────────────────────────────┘

[M]odify • [Q]uit/Enter • [H]elp
> _
```

**Enhanced Features**:
- Pulse animation with elapsed time
- ASCII borders around scripts
- Color coding for status/menu
- Proper visual hierarchy
- Session management

## 🏗️ TECHNICAL IMPLEMENTATION

### **Phase 1: Basic Infrastructure**

**Required Services**:
1. **ScriptRepository** - In-memory storage
2. **ScriptGenerator** - Mock AI providers with delays
3. **Basic Console I/O** - Simple text output
4. **Command Parser** - Handle m/q/h commands

**Files to Create**:
- `ScriptRepository.cs` - In-memory List<ScriptResponse>
- `MockProvider.cs` - Simple AI provider stub
- `ScriptGenerator.cs` - Orchestrates providers
- `Program.cs` - Basic console loop

### **Phase 2: UI Polish**

**Enhanced Services**:
- `StatusDisplay.cs` - Animated status with pulse
- `ScriptRenderer.cs` - ASCII borders and colors
- `MenuRenderer.cs` - Formatted menu display
- `SessionManager.cs` - Session persistence

## 🎯 SUCCESS CRITERIA

### **Phase 1 Complete**
- [ ] `please create a script` → generates mock script
- [ ] `m` → asks for modification
- [ ] `q` or Enter → exits
- [ ] Basic error handling
- [ ] All tests passing

### **Phase 2 Complete**
- [ ] Animated status display
- [ ] Bordered script output
- [ ] Color-coded interface
- [ ] Session management
- [ ] Polished user experience

## 🚀 IMMEDIATE NEXT STEPS

1. **Complete Infrastructure Layer** (TDD approach)
2. **Build minimal console UI** (basic text output)
3. **Test end-to-end functionality**
4. **Add polish incrementally**

This approach ensures we have a working tool quickly, then can iterate to the polished version.
