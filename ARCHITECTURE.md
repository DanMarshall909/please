# Please - Architecture Documentation

## 🏗️ **Architecture Comparison**

### **Go v5 Architecture**
```
legacy-go/
├── main.go              # Monolithic entry point
├── config/              # Configuration
├── providers/           # AI provider implementations
├── script/              # Script operations
├── ui/                  # User interface
└── types/               # Shared types
```

### **C# v6 Clean Architecture**
```
src/
├── Domain/              # ZERO dependencies
│   ├── Entities/        # Core business models
│   ├── Enums/           # Domain enums
│   └── Interfaces/      # Repository abstractions
├── Application/         # MediatR only
│   ├── Commands/        # CQRS commands
│   └── Queries/         # CQRS queries
├── Infrastructure/      # ALL external dependencies
│   ├── Providers/       # AI implementations
│   └── Repositories/    # Data persistence
└── Presentation/        # Console application
    └── Console/         # CLI interface
```

## 🎪 **Why Dual Implementation?**

### **Business Continuity**
- **Always releasable**: Go version ensures users always have working software
- **Risk mitigation**: C# development doesn't block Go improvements
- **Gradual transition**: Users can test C# version while Go remains available

### **Technical Benefits**
- **Architecture evolution**: Clean Architecture vs monolithic Go structure
- **Tooling improvement**: VS/Rider debugging vs VS Code Go
- **Performance gains**: Better async patterns and HTTP clients
- **Maintainability**: Clear separation of concerns

### **Development Experience**
- **Parallel development**: Teams can work on both implementations
- **Learning opportunity**: Compare patterns and approaches
- **Future flexibility**: Can maintain both or sunset one based on usage

## 📦 **Dependencies**

### **Go v5 Dependencies**
- Go 1.21+
- Standard library only (no external dependencies)

### **C# v6 Dependencies**
- .NET 8
- MediatR (CQRS)
- Microsoft.Extensions.* (Configuration, DI, Logging)
- NUnit (Testing)

## 🔧 **Clean Architecture Principles**

### **Domain Layer (Core)**
- **Zero Dependencies**: No external references
- **Business Logic**: Core entities and business rules
- **Interfaces**: Contracts for infrastructure

### **Application Layer**
- **Use Cases**: Application-specific business rules
- **CQRS**: Command and Query separation
- **Dependency Inversion**: Depends on Domain abstractions

### **Infrastructure Layer**
- **External Concerns**: Databases, APIs, file systems
- **Implementation**: Concrete implementations of Domain interfaces
- **Configuration**: Environment-specific settings

### **Presentation Layer**
- **User Interface**: Console application
- **Controllers**: Entry points for user interactions
- **Dependency Injection**: Wiring up the application

## 🎯 **Design Decisions**

### **No MediatR in Production**
- **Reason**: Native AOT compatibility
- **Alternative**: Direct dependency injection
- **Benefit**: Smaller binary size and faster startup

### **Multiple AI Providers**
- **Flexibility**: Users can choose their preferred provider
- **Reliability**: Fallback options if one provider fails
- **Cost Optimization**: Different pricing models

### **Async/Await Patterns**
- **Performance**: Non-blocking I/O operations
- **Scalability**: Better resource utilization
- **User Experience**: Responsive application

## 🧪 **Testing Strategy**

### **Unit Testing**
- **Domain**: Business logic validation
- **Application**: Use case testing
- **Infrastructure**: Provider integration testing

### **Integration Testing**
- **End-to-End**: Full workflow testing
- **Provider Testing**: Real API integration
- **Configuration**: Environment setup validation

### **Test Structure**
```
tests/
├── Domain.UnitTests/
├── Application.UnitTests/
├── Infrastructure.UnitTests/
├── Presentation.UnitTests/
├── Application.IntegrationTests/
└── TestUtilities/
```

## 🔍 **Implementation Details**

### **Provider Pattern**
- **IProvider Interface**: Common contract for all AI providers
- **Factory Pattern**: Dynamic provider selection
- **Configuration**: Provider-specific settings

### **Result Pattern**
- **Error Handling**: Explicit success/failure states
- **No Exceptions**: Predictable error handling
- **Composability**: Chainable operations

### **Dependency Injection**
- **Composition Root**: Application startup
- **Lifetime Management**: Scoped, singleton, transient
- **Configuration**: Environment-based settings

---

*For implementation details, see [DEVELOPMENT.md](DEVELOPMENT.md)*  
*For quick start instructions, see [GETTING-STARTED.md](GETTING-STARTED.md)*
