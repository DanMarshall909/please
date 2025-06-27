# Please - Getting Started Guide

## 🚀 **Quick Start**

### **🎯 Automated Setup (Recommended)**

The fastest way to get started is using our automated setup scripts:

#### Windows (PowerShell)
```powershell
# Interactive setup with intelligent defaults
.\scripts\setup-environment.ps1

# Direct OpenAI setup with permanent storage
.\scripts\setup-environment.ps1 -Provider OpenAI -Permanent
```

#### Linux/macOS/WSL (Bash)
```bash
# Interactive setup
./scripts/setup-environment.sh

# Direct provider setup with permanent storage
./scripts/setup-environment.sh --provider openai --permanent
```

📖 **For detailed setup script documentation, see [scripts/README.md](scripts/README.md)**

---

## 🔧 **Manual Setup**

If you prefer manual configuration, see [CONFIGURATION.md](CONFIGURATION.md) for detailed instructions.

## 🏃 **Running Please**

### **🟢 C# Version (Recommended)**

After configuration, run the C# version:

```bash
# Build the application
dotnet build src/Presentation/Please.Console

# Run from project directory
cd src/Presentation/Please.Console
dotnet run -- "list files in current directory"

# Or run the built executable
cd bin/Debug/net8.0/win-x64
.\Please.Console.exe "create a PowerShell script to backup my documents"
```

### **🔵 Go Version (Alternative)**

For the Go version:

```bash
# Build the Go version
cd legacy-go
go build -o please-go.exe

# Run it
./please-go.exe "list files in current directory"
```

## 💡 **Usage Examples**

### **Basic Script Generation**
```bash
# PowerShell scripts
.\Please.Console.exe "create a script to clean temporary files"

# Batch files
.\Please.Console.exe "write a batch file to backup my documents"

# Python scripts
.\Please.Console.exe "generate a Python script to process CSV files"
```

### **System Administration**
```bash
# File management
.\Please.Console.exe "create a PowerShell script to organize files by date"

# System monitoring
.\Please.Console.exe "write a script to check disk usage and send alerts"

# Network utilities
.\Please.Console.exe "generate a script to test network connectivity"
```

### **Development Tasks**
```bash
# Build automation
.\Please.Console.exe "create a script to build and deploy my application"

# Testing utilities
.\Please.Console.exe "write a script to run tests and generate reports"

# Environment setup
.\Please.Console.exe "generate a script to install development dependencies"
```

## 🔧 **Configuration Examples**

### **Using Different Providers**
```bash
# Use specific provider (if multiple configured)
.\Please.Console.exe "create a bash script" --provider anthropic

# Use local Ollama
.\Please.Console.exe "generate a Python script" --provider ollama
```

### **Environment Variables**
```powershell
# Set provider preference
$env:PLEASE_DEFAULT_PROVIDER = "openai"

# Set custom model
$env:OPENAI_DEFAULT_MODEL = "gpt-4"
```

## 🧪 **Testing Your Setup**

### **1. Basic Test**
```bash
.\Please.Console.exe "echo hello world"
```

**Expected Output:**
```
info: Processing task: echo hello world
info: Script generated successfully
```

### **2. Provider Test**
```bash
.\Please.Console.exe "list current directory contents"
```

**Expected Output:**
- A PowerShell or batch script that lists directory contents
- No error messages about missing API keys

### **3. Complex Test**
```bash
.\Please.Console.exe "create a script to backup my documents folder to a zip file"
```

**Expected Output:**
- A complete script with error handling
- Comments explaining each step

## 🆘 **Troubleshooting**

### **Common Issues**

#### **"OpenAI API key not configured"**
**Solution:** 
1. Run the setup script: `.\scripts\setup-environment.ps1`
2. Or set manually: `$env:OPENAI_API_KEY = "your-key-here"`

#### **"Could not find Please.Console.exe"**
**Solution:**
1. Build first: `dotnet build src/Presentation/Please.Console`
2. Navigate to output directory: `cd src/Presentation/Please.Console/bin/Debug/net8.0/win-x64`

#### **"Invalid API key"**
**Solution:**
1. Verify your API key is correct
2. Check if your API key has sufficient credits
3. Ensure the API key has proper permissions

#### **"No response from provider"**
**Solution:**
1. Check your internet connection
2. Verify the provider service is available
3. Try a different provider if configured

### **Getting Help**

#### **Configuration Issues**
- See [CONFIGURATION.md](CONFIGURATION.md) for detailed setup instructions
- Check [scripts/README.md](scripts/README.md) for setup script documentation

#### **Development Issues**
- See [DEVELOPMENT.md](DEVELOPMENT.md) for development workflow
- Check [ARCHITECTURE.md](ARCHITECTURE.md) for technical details

#### **Support**
- Create an issue in the GitHub repository
- Use appropriate tags: `[Go]`, `[C#]`, or `[Setup]`

## 📚 **Next Steps**

Once you have Please running:

1. **Explore Features**: Try different types of script generation
2. **Customize Configuration**: Adjust models and providers to your needs
3. **Integrate**: Use Please in your development workflow
4. **Contribute**: Help improve the project (see [DEVELOPMENT.md](DEVELOPMENT.md))

### **Advanced Usage**
- **Batch Processing**: Create multiple scripts in sequence
- **Custom Prompts**: Develop specialized prompts for your use cases
- **Integration**: Use Please as part of larger automation workflows

### **Provider Optimization**
- **Cost Management**: Use appropriate models for different tasks
- **Performance**: Choose faster models for simple tasks
- **Quality**: Use premium models for complex scripts

---

## 🌟 **What's Next?**

- **Read [ARCHITECTURE.md](ARCHITECTURE.md)** to understand the technical design
- **Check [DEVELOPMENT.md](DEVELOPMENT.md)** to contribute to the project  
- **Review [CONFIGURATION.md](CONFIGURATION.md)** for advanced configuration options
- **Explore [scripts/README.md](scripts/README.md)** for setup script details

*Happy scripting with Please! 🎉*
