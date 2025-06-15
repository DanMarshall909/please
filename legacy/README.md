# Please - Go Implementation (Legacy)

This folder contains the original Go implementation of the Please CLI tool, preserved as reference during the C# rewrite.

## 📂 **Contents**

### **Go Source Files (Already Moved)**
- `main.go` - Main entry point and CLI logic
- `core_logic_test.go` - Core business logic tests
- `integration_contract_test.go` - Integration contract tests

### **Go Packages (To Be Moved)**
When VS Code tabs are closed, these should be moved here:
- `ui/` - User interface and menu system
- `config/` - Configuration management
- `localization/` - Internationalization system
- `models/` - Data models and selection logic
- `providers/` - AI provider implementations (OpenAI, Anthropic, Ollama)
- `script/` - Script operations and validation
- `types/` - Type definitions
- `themes/` - Localization theme files
- `releases/` - Binary releases

### **Build Files**
- `please` - Go binary
- `pls.bat` - Windows alias script
- Various coverage files

## 🎯 **Purpose**

This legacy implementation serves as:

1. **Functional Reference** - Defines what the C# version must do
2. **Test Specifications** - Go tests become C# test requirements  
3. **Business Logic Reference** - Complex algorithms and decision trees
4. **Provider Integration Examples** - API usage patterns for AI providers
5. **CLI Behavior Specification** - Exact command-line interface requirements

## 🏗️ **Architecture Lessons**

### **What Worked Well**
- Provider abstraction pattern
- CQRS-like command structure
- Comprehensive test coverage for main business logic
- Localization system design
- Risk assessment for script safety

### **Technical Debt Identified**
- Global variables throughout the codebase
- Tight coupling between UI and business logic
- Retrofitted localization (not designed-in)
- Mixed responsibilities in single functions
- Limited dependency injection

### **C# Improvements**
- Clean Architecture with strict layer boundaries
- Dependency injection throughout
- CQRS pattern with MediatR
- Async/await for all I/O operations
- Proper error handling with exceptions
- Structured logging with Serilog

## 📋 **Migration Notes**

### **Key Functions to Preserve**
- `isLastScriptCommand()` - Natural language command recognition
- `getFallbackModel()` - Provider-specific model selection
- `generateScript()` - Core script generation workflow
- Risk assessment logic in UI layer
- Provider configuration validation

### **Test Translation Priority**
1. **integration_contract_test.go** → C# Integration Tests
2. **core_logic_test.go** → C# Unit Tests  
3. **main_test.go** → C# CLI Tests
4. Package-specific tests → C# Component Tests

### **Business Rules to Preserve**
- Command-line argument parsing behavior
- Provider selection algorithm
- Model fallback logic
- Script risk classification
- User confirmation workflows
- History and configuration file formats

---

*Preserved: June 15, 2025*  
*Status: Reference Implementation for C# Rewrite*  
*Next: C# Clean Architecture Implementation*
