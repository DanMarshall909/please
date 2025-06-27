# Testing the Updated Setup Scripts

## PowerShell Script Usage

```powershell
# Install just the alias
.\scripts\setup-environment.ps1 -InstallAlias

# Configure provider and install alias
.\scripts\setup-environment.ps1 -Provider OpenAI -Permanent -InstallAlias

# Interactive setup (will prompt for alias installation)
.\scripts\setup-environment.ps1
```

## Bash Script Usage  

```bash
# Install just the alias
./scripts/setup-environment.sh --install-alias

# Configure provider and install alias
./scripts/setup-environment.sh --provider ollama --permanent --install-alias

# Interactive setup (will prompt for alias installation)
./scripts/setup-environment.sh
```

## What the Updated Scripts Do

### PowerShell Script (`setup-environment.ps1`)
- **New Parameter**: `-InstallAlias` - Creates `pls.cmd` and `pls.ps1` files
- **Automatic PATH**: Adds project directory to user PATH
- **Interactive Prompt**: Asks if you want to install alias after provider setup
- **Cross-Platform**: Works in both Command Prompt and PowerShell

### Bash Script (`setup-environment.sh`)
- **New Parameter**: `--install-alias` - Creates `~/bin/pls` script  
- **Automatic PATH**: Adds `~/bin` to PATH in shell profile
- **Interactive Prompt**: Asks if you want to install alias after provider setup
- **Shell Detection**: Works with bash, zsh, and other shells

## Files Created

### Windows (PowerShell script)
- `C:\Code\please\pls.cmd` - Batch file for Command Prompt
- `C:\Code\please\pls.ps1` - PowerShell script
- Adds `C:\Code\please` to user PATH

### Linux/WSL (Bash script) 
- `~/bin/pls` - Executable shell script
- Adds `~/bin` to PATH in shell profile (`.bashrc`, `.zshrc`, etc.)

## Usage After Installation

Both setups allow you to use:
```
pls get current time
pls list running services
pls create backup script for my documents
```

The scripts automatically handle:
- ✅ Project directory navigation
- ✅ Running the correct dotnet command
- ✅ Passing arguments properly
- ✅ Cross-platform compatibility