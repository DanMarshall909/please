# Please v5 - Go Legacy Implementation

This directory contains the stable Go implementation of Please from commit `ba2db8dd654f86c71155309ab7ad0bb45f3f2117`.

## ✅ Status
- **Build**: ✅ Working (`go build -o please.exe`)
- **Core Tests**: ✅ Passing (config, localization, models, providers, script, types)
- **Main Tests**: ⚠️ Minor localization test failures (non-blocking)
- **Tagged**: `v5.0-stable`

## 🚀 Quick Start

```bash
# Build Go version
cd legacy-go
go build -o please.exe

# Test Go version
go test ./...

# Run Go version
./please.exe --help
```

## 📦 Structure

```
legacy-go/
├── main.go                  # Entry point
├── config/                  # Configuration management
├── localization/            # Internationalization
├── models/                  # Selection logic
├── providers/               # AI provider implementations
├── script/                  # Script operations
├── types/                   # Type definitions
├── ui/                      # User interface
└── themes/                  # Localization themes
```

## 🎯 Features

- ✅ Multi-provider AI support (OpenAI, Anthropic, Ollama)
- ✅ Cross-platform script generation (Windows, Linux, macOS)
- ✅ Interactive menu system
- ✅ Script validation and safety checks
- ✅ Localization support (multiple languages)
- ✅ Test monitoring with AI analysis

## 🔄 Relationship to v6

This Go implementation serves as:
- **Reference implementation** for C# migration
- **Stable fallback** while v6 is developed
- **Test specification source** for C# feature parity

## 📝 Notes

- This is the **last stable Go version** before C# migration
- All future Go development should happen in this directory
- Main branch focuses on C# Clean Architecture implementation
- Both versions will be maintained as separate executables

---

*Generated from commit: ba2db8dd654f86c71155309ab7ad0bb45f3f2117*  
*Branch: release/please-v5-stable*  
*Date: June 15, 2025*
