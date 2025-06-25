# Please - AI-Powered Script Generation

![Please Banner](https://img.shields.io/badge/Please-Dual%20Implementation-blue?style=for-the-badge&logo=robot)
![Go Version](https://img.shields.io/badge/Go-v5.0--stable-00ADD8?style=for-the-badge&logo=go)
![C# Version](https://img.shields.io/badge/C%23-v6.0--production-239120?style=for-the-badge&logo=csharp)

**Please** is an AI-powered command-line tool that generates scripts for any task. Simply describe what you need in plain English, and Please creates the script for you.

## ✨ **What Please Does**

```bash
# Tell Please what you need
.\Please.Console.exe "create a PowerShell script to backup my documents"

# Get a complete, ready-to-run script
# Output: A PowerShell script with error handling, progress indicators, and comments
```

### **Real Examples**
- **"clean temporary files"** → PowerShell script that safely removes temp files
- **"backup my photos to OneDrive"** → Script with progress bars and error recovery
- **"install development tools"** → Automated setup script for your dev environment
- **"monitor disk usage"** → System monitoring script with alerts

## 🚀 **Get Started in 2 Minutes**

### **Windows (Recommended)**
```powershell
# 1. Quick setup with our automated script
.\scripts\setup-environment.ps1

# 2. Start generating scripts
.\Please.Console.exe "your task description here"
```

### **Linux/macOS**
```bash
# 1. Quick setup
./scripts/setup-environment.sh

# 2. Start using
./please "your task description here"
```

**📖 Complete setup guide:** [GETTING-STARTED.md](GETTING-STARTED.md)

## 🎯 **Two Implementations**

Please offers two production-ready implementations:

| Version | Status | Best For |
|---------|--------|----------|
| **🟢 C# v6.0** | ✅ Production Ready | **Recommended** - Modern architecture, 5 AI providers, automated setup |
| **🔵 Go v5.0** | ✅ Production Ready | Lightweight alternative, minimal dependencies |

## 🔧 **Supported AI Providers**

- **OpenAI** (GPT-4, GPT-3.5)
- **Anthropic** (Claude 3)
- **Google Gemini**
- **OpenRouter** (Multiple models)
- **Ollama** (Local models)

**⚙️ Configuration guide:** [CONFIGURATION.md](CONFIGURATION.md)

## 📚 **Documentation**

| Document | Description |
|----------|-------------|
| **[GETTING-STARTED.md](GETTING-STARTED.md)** | Setup instructions, usage examples, troubleshooting |
| **[CONFIGURATION.md](CONFIGURATION.md)** | AI provider setup, API keys, advanced options |
| **[ARCHITECTURE.md](ARCHITECTURE.md)** | Technical design, dual implementation strategy |
| **[DEVELOPMENT.md](DEVELOPMENT.md)** | Contributing, building, testing, roadmap |
| **[scripts/README.md](scripts/README.md)** | Automated setup script documentation |

## 🆘 **Need Help?**

- **Quick Start Issues**: See [GETTING-STARTED.md](GETTING-STARTED.md#troubleshooting)
- **Configuration Help**: Check [CONFIGURATION.md](CONFIGURATION.md)
- **Report Bugs**: Create a GitHub issue with `[C#]` or `[Go]` tag
- **Feature Requests**: Open a GitHub discussion

## 🤝 **Contributing**

Please is actively developed with both implementations welcoming contributions:

- **🟢 C# Development**: Modern clean architecture ([DEVELOPMENT.md](DEVELOPMENT.md))
- **🔵 Go Development**: Stable, lightweight implementation
- **📖 Documentation**: Help improve guides and examples

## ⭐ **Why Please?**

- **🎯 Simple**: Just describe what you need in plain English
- **🔒 Safe**: Generated scripts include error handling and safety checks
- **⚡ Fast**: Get working scripts in seconds, not hours
- **🎨 Flexible**: Choose from multiple AI providers and models
- **🔧 Production Ready**: Both implementations are battle-tested

---

## 🌟 **Ready to Start?**

1. **[Get Started](GETTING-STARTED.md)** - Setup and first script
2. **[Explore Examples](GETTING-STARTED.md#usage-examples)** - See what Please can do
3. **[Configure Providers](CONFIGURATION.md)** - Set up your preferred AI service
4. **[Join Development](DEVELOPMENT.md)** - Help make Please even better

*Transform your ideas into working scripts with Please! 🎉*

---

*Updated: June 25, 2025 | Status: Production Ready | [Issues](../../issues) | [Discussions](../../discussions)*
