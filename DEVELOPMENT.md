# Please - Development Guide

## 🎯 **Development Strategy**

### **Current Status (Complete)**
1. ✅ **C# Infrastructure**: Complete with all AI providers and services
2. ✅ **Console App**: Fully wired with dependency injection and CLI
3. ✅ **Integration Testing**: End-to-end functionality verified
4. ✅ **Feature Parity**: C# version matches and exceeds Go capabilities
5. ✅ **Automated Setup**: Environment configuration scripts added

### **Release Strategy**
- **Go v5**: Maintenance mode for bug fixes and minor features
- **C# v6**: Primary development focus with production releases
- **Migration Path**: Users can choose between implementations based on needs

## 📈 **Roadmap**

### **✅ Phase 1: C# Foundation (Completed)**
- ✅ Domain and Application layers
- ✅ Infrastructure implementation with all 5 AI providers
- ✅ Console application wiring with dependency injection
- ✅ Comprehensive integration tests

### **✅ Phase 2: Feature Parity (Completed)**  
- ✅ All Go features implemented in C#
- ✅ Performance optimization with async patterns
- ✅ Comprehensive testing across all layers
- ✅ Complete documentation and setup automation

### **✅ Phase 3: Production Release (Completed)**
- ✅ C# v6.0 production release ready
- ✅ Automated environment setup scripts
- ✅ Cross-platform compatibility
- 🔄 User feedback and iteration (ongoing)

### **🔄 Phase 4: Future Enhancements (Ongoing)**
- 🔄 Advanced script validation and safety features
- 🔄 Enhanced error handling and retry mechanisms
- 🔄 Performance monitoring and analytics
- 🔄 Additional AI provider integrations

## 🔧 **Building Both Versions**

### **Go Version**
```bash
cd legacy-go
go build -o please-go.exe
```

### **C# Version** 
```bash
dotnet build src/Presentation/Please.Console
# Output: src/Presentation/Please.Console/bin/Debug/net8.0/Please.Console.exe
```

## 📝 **Git Branch Strategy**

```
main                     # Current development state
├── release/please-v5-stable    # Go v5.0 stable branch
├── feature/please-v6-csharp    # C# v6.0 development branch
└── legacy/archive              # Previous experimental work
```

### **Branch Usage**
- **main**: Integration and coordination branch
- **release/please-v5-stable**: Go production releases
- **feature/please-v6-csharp**: C# development and testing

## 🤝 **Contributing**

### **Go v5 Contributions**
- Work in `legacy-go/` directory
- Focus on bug fixes and minor enhancements
- Maintain backward compatibility

### **C# v6 Contributions**
- Work in `src/` directory
- Follow Clean Architecture principles
- Implement features from Go version
- Add comprehensive tests

## 📊 **Implementation Status**

### **✅ Go v5.0 (Production Ready)**
- ✅ Multi-provider AI support (OpenAI, Anthropic, Ollama)
- ✅ Cross-platform script generation
- ✅ Interactive menu system
- ✅ Script validation and safety
- ✅ Localization support
- ✅ Test monitoring with AI analysis
- ✅ Builds successfully
- ⚠️ Minor localization test failures (non-blocking)

### **✅ C# v6.0 (Production Ready)**
- ✅ **Domain Layer**: Complete with entities, enums, interfaces, exceptions
- ✅ **Application Layer**: Clean architecture without MediatR for Native AOT compatibility
- ✅ **Infrastructure Layer**: Complete with 5 AI providers (OpenAI, Anthropic, Gemini, OpenRouter, Ollama)
- ✅ **Console Application**: Fully functional with dependency injection and CLI interface
- ✅ **Automated Setup**: Interactive scripts for environment configuration
- ✅ **Test Infrastructure**: Comprehensive unit testing across all layers
- ✅ **Integration Tests**: End-to-end testing with real AI providers
- ✅ **Documentation**: Complete setup and configuration guides

## 🔧 **Supported Providers**

| Provider | API Key Required | Base URL | Default Model |
|----------|------------------|----------|---------------|
| **OpenAI** | Yes | https://api.openai.com/v1 | gpt-4o-mini |
| **Anthropic** | Yes | https://api.anthropic.com/v1 | claude-3-haiku-20240307 |
| **Gemini** | Yes | https://generativelanguage.googleapis.com/v1beta | gemini-pro |
| **OpenRouter** | Yes | https://openrouter.ai/api/v1 | microsoft/wizardlm-2-8x22b |
| **Ollama** | No | http://localhost:11434 | llama2 |

## 🆘 **Support**

- **Go v5 Issues**: Report with `[Go]` prefix
- **C# v6 Issues**: Report with `[C#]` prefix  
- **General Issues**: Architecture or strategy questions

## 🧪 **Testing**

### **Running Tests**

#### **Go Tests**
```bash
cd legacy-go
go test ./...
```

#### **C# Tests**
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Infrastructure.UnitTests/Please.Infrastructure.UnitTests
```

### **Test Coverage**
- **Go**: Manual testing and integration tests
- **C#**: Comprehensive unit testing with NUnit framework

## 🚀 **Development Workflow**

### **Setting Up Development Environment**

1. **Clone Repository**
```bash
git clone <repository-url>
cd please
```

2. **Go Development**
```bash
cd legacy-go
go mod download
go build
```

3. **C# Development**
```bash
dotnet restore
dotnet build
```

4. **Environment Configuration**
```bash
# Use automated setup
.\scripts\setup-environment.ps1    # Windows
./scripts/setup-environment.sh     # Linux/macOS
```

### **Making Changes**

1. **Choose Implementation**: Work in either `legacy-go/` or `src/` directory
2. **Follow Patterns**: Maintain consistency with existing code
3. **Add Tests**: Ensure new features are tested
4. **Update Documentation**: Keep docs in sync with changes

### **Release Process**

#### **Go v5 Releases**
- Focus on bug fixes and minor features
- Maintain backward compatibility
- Use semantic versioning

#### **C# v6 Releases**
- Primary development focus
- Follow Clean Architecture principles
- Comprehensive testing required

---

*For architecture details, see [ARCHITECTURE.md](ARCHITECTURE.md)*  
*For setup instructions, see [GETTING-STARTED.md](GETTING-STARTED.md)*  
*For configuration help, see [CONFIGURATION.md](CONFIGURATION.md)*
