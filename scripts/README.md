# Please v6 Environment Setup Scripts

This directory contains automated setup scripts to configure AI provider environment variables for Please v6.

## Available Scripts

### 🪟 Windows PowerShell Script
**File:** `setup-environment.ps1`

Cross-platform PowerShell script that works on Windows, Linux, and macOS with PowerShell Core.

### 🐧 Unix Shell Script  
**File:** `setup-environment.sh`

Bash-compatible script for Linux, macOS, and WSL environments.

## Quick Start

### Windows (PowerShell)
```powershell
# Interactive setup
.\scripts\setup-environment.ps1

# Direct provider configuration
.\scripts\setup-environment.ps1 -Provider OpenAI -Permanent

# Configure multiple providers
.\scripts\setup-environment.ps1 -Provider Multiple
```

### Linux/macOS/WSL (Bash)
```bash
# Make script executable
chmod +x scripts/setup-environment.sh

# Interactive setup
./scripts/setup-environment.sh

# Direct provider configuration
./scripts/setup-environment.sh --provider openai --permanent

# Configure multiple providers
./scripts/setup-environment.sh --provider multiple
```

## Supported Providers

| Provider | Key Format | Default Model | Local |
|----------|------------|---------------|-------|
| **OpenAI** | `sk-...` | `gpt-4o-mini` | ❌ |
| **Anthropic** | `sk-ant-...` | `claude-3-haiku-20240307` | ❌ |
| **Google Gemini** | `[39 chars]` | `gemini-pro` | ❌ |
| **OpenRouter** | `sk-or-...` | `microsoft/wizardlm-2-8x22b` | ❌ |
| **Ollama** | N/A | `llama2` | ✅ |

## Environment Variables

The scripts configure these environment variables:

### OpenAI
- `OPENAI_API_KEY` - Your OpenAI API key
- `OPENAI_DEFAULT_MODEL` - Default model (e.g., `gpt-4o-mini`)
- `OPENAI_BASE_URL` - API base URL (default: `https://api.openai.com/v1`)

### Anthropic (Claude)
- `ANTHROPIC_API_KEY` - Your Anthropic API key  
- `ANTHROPIC_DEFAULT_MODEL` - Default model (e.g., `claude-3-haiku-20240307`)
- `ANTHROPIC_BASE_URL` - API base URL (default: `https://api.anthropic.com/v1`)

### Google Gemini
- `GEMINI_API_KEY` - Your Gemini API key
- `GEMINI_DEFAULT_MODEL` - Default model (e.g., `gemini-pro`)
- `GEMINI_BASE_URL` - API base URL (default: `https://generativelanguage.googleapis.com/v1beta`)

### OpenRouter
- `OPENROUTER_API_KEY` - Your OpenRouter API key
- `OPENROUTER_DEFAULT_MODEL` - Default model (e.g., `microsoft/wizardlm-2-8x22b`)
- `OPENROUTER_BASE_URL` - API base URL (default: `https://openrouter.ai/api/v1`)

### Ollama (Local)
- `OLLAMA_BASE_URL` - Ollama server URL (default: `http://localhost:11434`)
- `OLLAMA_DEFAULT_MODEL` - Default model (e.g., `llama2`)

## Features

### 🎯 Intelligent Defaults
- Pre-configured with recommended models for each provider
- Automatic API key format validation
- Smart base URL defaults

### 🔒 Security
- Secure input for API keys (hidden from terminal history)
- API key masking in status displays
- Option to keep existing configurations

### ⚙️ Flexible Configuration
- **Temporary**: Variables set for current session only
- **Permanent**: Variables saved to system/profile for persistence
- **Interactive**: Guided menu-driven setup
- **Direct**: Command-line provider specification

### 🧪 Testing & Validation
- Connection testing after configuration
- API key format validation
- Configuration status display

## Usage Examples

### PowerShell Examples

```powershell
# Interactive setup with menu
.\scripts\setup-environment.ps1

# Configure OpenAI with permanent variables
.\scripts\setup-environment.ps1 -Provider OpenAI -Permanent

# Configure Anthropic temporarily
.\scripts\setup-environment.ps1 -Provider Anthropic

# Configure multiple providers
.\scripts\setup-environment.ps1 -Provider Multiple
```

### Bash Examples

```bash
# Interactive setup
./scripts/setup-environment.sh

# Configure specific provider permanently
./scripts/setup-environment.sh --provider openai --permanent

# Configure Ollama (local)
./scripts/setup-environment.sh --provider ollama

# Show help
./scripts/setup-environment.sh --help
```

## Getting API Keys

### OpenAI
1. Visit: https://platform.openai.com/api-keys
2. Create new secret key
3. Copy the key (starts with `sk-`)

### Anthropic
1. Visit: https://console.anthropic.com/
2. Create API key  
3. Copy the key (starts with `sk-ant-`)

### Google Gemini
1. Visit: https://makersuite.google.com/app/apikey
2. Create API key
3. Copy the 39-character key

### OpenRouter
1. Visit: https://openrouter.ai/keys
2. Create API key
3. Copy the key (starts with `sk-or-`)

### Ollama (Local)
1. Install from: https://ollama.ai/
2. Pull a model: `ollama pull llama2`
3. Start service: `ollama serve`
4. No API key needed

## Configuration Priority

Environment variables are loaded in this order (highest to lowest):

1. **System Environment Variables** (highest)
2. **User Environment Variables** 
3. **Shell Profile Variables**
4. **appsettings.json** file
5. **Default Values** (lowest)

## Testing Your Setup

After running the setup script, test your configuration:

### Windows
```powershell
# Build the application
dotnet build src/Presentation/Please.Console

# Test with simple request  
cd src/Presentation/Please.Console/bin/Debug/net8.0/win-x64
.\Please.Console.exe "echo hello world"
```

### Linux/macOS
```bash
# Build the application
dotnet build src/Presentation/Please.Console

# Test with simple request
cd src/Presentation/Please.Console/bin/Debug/net8.0/linux-x64
./Please.Console "echo hello world"
```

## Expected Output

### Success ✅
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

### Configuration Issue ❌
```
warn: Please.Infrastructure.Services.ScriptGenerator[0]
      Failed to generate script using OpenAi: OpenAI API key not configured
```

## Troubleshooting

### "API key not configured"
**Solution**: Run the setup script and ensure your API key is correctly entered

### "Invalid API key format"  
**Solution**: Double-check your API key format matches the provider's expected pattern

### Environment variables not persisting
**Solutions**:
- Use the `--permanent` flag (bash) or `-Permanent` flag (PowerShell)
- Restart your terminal after running the script
- Manually source your shell profile: `source ~/.bashrc` (Linux) or restart PowerShell (Windows)

### Connection test fails
**Solutions**:
- Check your internet connection
- Verify the API key has sufficient permissions/credits
- For Ollama, ensure the service is running: `ollama serve`

### Script permission denied (Linux/macOS)
**Solution**: Make the script executable: `chmod +x scripts/setup-environment.sh`

## Manual Configuration

If you prefer manual setup, you can set environment variables directly:

### Windows (PowerShell)
```powershell
# Temporary (current session)
$env:OPENAI_API_KEY = "your-api-key-here"

# Permanent (current user)
[Environment]::SetEnvironmentVariable("OPENAI_API_KEY", "your-api-key-here", "User")
```

### Linux/macOS (Bash)
```bash
# Temporary (current session)
export OPENAI_API_KEY="your-api-key-here"

# Permanent (add to ~/.bashrc or ~/.zshrc)
echo 'export OPENAI_API_KEY="your-api-key-here"' >> ~/.bashrc
```

## Security Best Practices

1. **Never commit API keys** to version control
2. **Use environment variables** instead of hardcoding keys
3. **Rotate API keys** regularly
4. **Use separate keys** for development and production
5. **Monitor API usage** for unexpected activity
6. **Restrict key permissions** when possible

---

For additional configuration options and advanced usage, see [CONFIGURATION.md](../CONFIGURATION.md) in the project root.
