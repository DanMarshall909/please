# Please v6 Configuration Guide

## 🚀 **Quick Setup (Recommended)**

For the fastest setup experience, use our automated configuration scripts:

### Windows (PowerShell)
```powershell
# Interactive setup with intelligent defaults
.\scripts\setup-environment.ps1

# Direct provider setup with permanent storage
.\scripts\setup-environment.ps1 -Provider OpenAI -Permanent
```

### Linux/macOS/WSL (Bash)
```bash
# Interactive setup
./scripts/setup-environment.sh

# Direct provider setup with permanent storage  
./scripts/setup-environment.sh --provider openai --permanent
```

📖 **For detailed setup script documentation, see [scripts/README.md](scripts/README.md)**

---

## 🔑 Manual API Key Configuration

The Please v6 C# application supports multiple AI providers with **secure configuration options**:

### Method 1: Environment Variables (🔐 **RECOMMENDED & SECURE**)

**Why Environment Variables?**
- ✅ **Secure**: API keys never stored in source control
- ✅ **Production-ready**: Industry standard for sensitive data
- ✅ **Cross-platform**: Works on Windows, Linux, macOS
- ✅ **Already supported**: .NET configuration automatically reads them

Set environment variables for the providers you want to use:

#### Windows PowerShell:
```powershell
# OpenAI Configuration
$env:OPENAI_API_KEY = "sk-your-openai-key-here"
$env:OPENAI_DEFAULT_MODEL = "gpt-4o-mini"

# Anthropic Configuration  
$env:ANTHROPIC_API_KEY = "sk-ant-your-anthropic-key-here"
$env:ANTHROPIC_DEFAULT_MODEL = "claude-3-haiku-20240307"

# Gemini Configuration
$env:GEMINI_API_KEY = "your-gemini-key-here"
$env:GEMINI_DEFAULT_MODEL = "gemini-pro"

# OpenRouter Configuration
$env:OPENROUTER_API_KEY = "sk-or-your-openrouter-key-here"
$env:OPENROUTER_DEFAULT_MODEL = "microsoft/wizardlm-2-8x22b"

# Ollama Configuration (local)
$env:OLLAMA_BASE_URL = "http://localhost:11434"
$env:OLLAMA_DEFAULT_MODEL = "llama2"
```

#### Windows Command Prompt:
```cmd
rem OpenAI Configuration
set OPENAI_API_KEY=sk-your-openai-key-here
set OPENAI_DEFAULT_MODEL=gpt-4o-mini

rem Anthropic Configuration
set ANTHROPIC_API_KEY=sk-ant-your-anthropic-key-here
set ANTHROPIC_DEFAULT_MODEL=claude-3-haiku-20240307
```

#### To set permanently:
```powershell
# PowerShell - Set permanently for current user
[Environment]::SetEnvironmentVariable("OPENAI_API_KEY", "sk-your-key-here", "User")
```

```cmd
rem Command Prompt - Set permanently
setx OPENAI_API_KEY "sk-your-key-here"
```

### Method 2: User Secrets (🔐 **SECURE FOR DEVELOPMENT**)

For development, use .NET User Secrets to store sensitive data outside your project:

```powershell
# Navigate to the console project
cd src/Presentation/Please.Console

# Initialize user secrets
dotnet user-secrets init

# Set API keys (stored securely outside project)
dotnet user-secrets set "OPENAI_API_KEY" "sk-your-openai-key-here"
dotnet user-secrets set "ANTHROPIC_API_KEY" "sk-ant-your-anthropic-key-here"
dotnet user-secrets set "GEMINI_API_KEY" "your-gemini-key-here"
```

### Method 3: Configuration File (⚠️ **NON-SENSITIVE DATA ONLY**)

The `appsettings.json` file should **ONLY** contain non-sensitive configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "DefaultSettings": {
    "OPENAI_DEFAULT_MODEL": "gpt-4o-mini",
    "OLLAMA_BASE_URL": "http://localhost:11434",
    "ANTHROPIC_DEFAULT_MODEL": "claude-3-haiku-20240307",
    "GEMINI_DEFAULT_MODEL": "gemini-pro"
  }
}
```

**🔐 Security Best Practice**: Never store API keys in JSON files that might be committed to source control.

## 🚀 Usage Examples

### Basic Usage
```powershell
# After configuring your API key
.\Please.Console.exe "list all files in current directory"
.\Please.Console.exe "create a PowerShell script to backup my documents"
.\Please.Console.exe "write a batch file to clean temporary files"
```

### Advanced Usage
```powershell
# Specify different providers (when multiple are configured)
.\Please.Console.exe "generate a Python script" --provider anthropic
.\Please.Console.exe "create a bash script" --provider ollama
```

## 🔧 Supported Providers

| Provider | API Key Required | Base URL | Default Model |
|----------|------------------|----------|---------------|
| **OpenAI** | Yes | https://api.openai.com/v1 | gpt-4o-mini |
| **Anthropic** | Yes | https://api.anthropic.com/v1 | claude-3-haiku-20240307 |
| **Gemini** | Yes | https://generativelanguage.googleapis.com/v1beta | gemini-pro |
| **OpenRouter** | Yes | https://openrouter.ai/api/v1 | microsoft/wizardlm-2-8x22b |
| **Ollama** | No | http://localhost:11434 | llama2 |

## 🔍 Configuration Priority

The application loads configuration in this order (highest to lowest priority):

1. **Environment Variables** (highest priority) 🔐
2. **User Secrets** (development only) 🔐  
3. **appsettings.json** file (non-sensitive data only)
4. **Default values** (lowest priority)

**🔐 Security Note**: Environment variables and User Secrets will always override JSON file values, ensuring sensitive data stays secure.

## 🧪 Testing Your Configuration

After setting up your API key, test it:

```powershell
# Build the application
dotnet build src/Presentation/Please.Console

# Test with a simple request
cd src/Presentation/Please.Console/bin/Debug/net8.0/win-x64
.\Please.Console.exe "echo hello world"
```

### Expected Output (Success):
```
info: TaskProcessor[0]
      Processing task: echo hello world
info: Please.Application.Services.ScriptService[0]
      Generating script
info: Please.Infrastructure.Services.ScriptGenerator[0]
      Generating script using OpenAi for task: echo hello world
info: Please.Application.Services.ScriptService[0]
      Script generated successfully
```

### Expected Output (No API Key):
```
warn: Please.Infrastructure.Services.ScriptGenerator[0]
      Failed to generate script using OpenAi: OpenAI API key not configured
```

## 🆘 Troubleshooting

### Problem: "OpenAI API key not configured"
**Solution**: Set the `OPENAI_API_KEY` environment variable or add it to `appsettings.json`

### Problem: "Configuration file not found"
**Solution**: Make sure `appsettings.json` exists in the same directory as the executable

### Problem: "Invalid API key"
**Solution**: Verify your API key is correct and has sufficient credits/permissions

### Problem: Environment variables not working
**Solution**: 
1. Restart your terminal/command prompt after setting environment variables
2. Use `echo $env:OPENAI_API_KEY` (PowerShell) or `echo %OPENAI_API_KEY%` (CMD) to verify the variable is set

## 🔐 Security Best Practices

### ✅ DO:
1. **Use environment variables** for production deployments
2. **Use User Secrets** for development (.NET tooling)
3. **Use Azure Key Vault** for enterprise scenarios
4. **Restrict API key permissions** when possible
5. **Rotate API keys** regularly
6. **Use different keys** for development and production
7. **Add appsettings.json to .gitignore** if it contains any sensitive data

### ❌ DON'T:
1. **Never commit API keys** to version control
2. **Never store API keys** in appsettings.json files
3. **Never share API keys** in chat/email/documentation
4. **Never use production keys** in development

### 🚨 Emergency: If API Key is Compromised
1. **Immediately revoke** the compromised key
2. **Generate a new key** from your provider
3. **Update your configuration** with the new key
4. **Review access logs** for unauthorized usage

## 📖 Getting API Keys

### OpenAI
1. Go to https://platform.openai.com/api-keys
2. Create a new API key
3. Copy the key (starts with `sk-`)

### Anthropic
1. Go to https://console.anthropic.com/
2. Create an API key
3. Copy the key (starts with `sk-ant-`)

### Google Gemini
1. Go to https://makersuite.google.com/app/apikey
2. Create an API key
3. Copy the key

### OpenRouter
1. Go to https://openrouter.ai/keys
2. Create an API key
3. Copy the key (starts with `sk-or-`)

### Ollama (Local)
1. Install Ollama from https://ollama.ai/
2. Pull a model: `ollama pull llama2`
3. Start the service: `ollama serve`
4. No API key required for local usage
