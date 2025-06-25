# Please v6 Configuration Guide

## 🔑 API Key Configuration

The Please v6 C# application supports multiple AI providers. Here's how to configure them:

### Method 1: Environment Variables (Recommended)

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

### Method 2: Configuration File

Edit the `src/Presentation/Please.Console/appsettings.json` file:

```json
{
  "OPENAI_API_KEY": "sk-your-openai-key-here",
  "OPENAI_DEFAULT_MODEL": "gpt-4o-mini",
  "OPENAI_BASE_URL": "https://api.openai.com/v1",
  
  "ANTHROPIC_API_KEY": "sk-ant-your-anthropic-key-here",
  "ANTHROPIC_DEFAULT_MODEL": "claude-3-haiku-20240307",
  "ANTHROPIC_BASE_URL": "https://api.anthropic.com/v1",
  
  "GEMINI_API_KEY": "your-gemini-key-here",
  "GEMINI_DEFAULT_MODEL": "gemini-pro",
  "GEMINI_BASE_URL": "https://generativelanguage.googleapis.com/v1beta",
  
  "OPENROUTER_API_KEY": "sk-or-your-openrouter-key-here",
  "OPENROUTER_DEFAULT_MODEL": "microsoft/wizardlm-2-8x22b",
  "OPENROUTER_BASE_URL": "https://openrouter.ai/api/v1",
  
  "OLLAMA_BASE_URL": "http://localhost:11434",
  "OLLAMA_DEFAULT_MODEL": "llama2"
}
```

**⚠️ Security Note**: Never commit your API keys to version control. Add `appsettings.json` to `.gitignore` if it contains real keys.

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

1. **Environment Variables** (highest priority)
2. **appsettings.json** file
3. **Default values** (lowest priority)

This means environment variables will override values in `appsettings.json`.

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

1. **Never commit API keys** to version control
2. **Use environment variables** for production deployments
3. **Restrict API key permissions** when possible
4. **Rotate API keys** regularly
5. **Use different keys** for development and production

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
