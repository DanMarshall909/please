# Please - Cross-Platform Releases

🤖 **Please** is your overly helpful digital assistant for AI-powered script generation!

## Available Releases

### Windows
- **`please-windows-amd64.exe`** - Windows 64-bit (Intel/AMD processors)
  - Compatible with Windows 10, 11, and Server versions
  - Run directly from command line or PowerShell

### Linux
- **`please-linux-amd64`** - Linux 64-bit (Intel/AMD processors)
  - Compatible with most Linux distributions (Ubuntu, CentOS, Debian, etc.)
  - Make executable: `chmod +x please-linux-amd64`
- **`please-linux-arm64`** - Linux ARM64 (ARM processors)
  - Compatible with ARM-based Linux systems (Raspberry Pi 4+, AWS Graviton, etc.)
  - Make executable: `chmod +x please-linux-arm64`

### macOS
- **`please-macos-amd64`** - macOS Intel (x86_64)
  - Compatible with Intel-based Macs
  - Make executable: `chmod +x please-macos-amd64`
- **`please-macos-arm64`** - macOS Apple Silicon (M1/M2/M3)
  - Compatible with Apple Silicon Macs (M1, M2, M3 chips)
  - Make executable: `chmod +x please-macos-arm64`

## Quick Start

1. **Download** the appropriate binary for your platform
2. **Make executable** (Linux/macOS only): `chmod +x <binary-name>`
3. **Run**: `./please-<platform> help` or just `./please-<platform>`
4. **Optional**: Add to PATH or create an alias for easier access

## Installation Examples

### Windows
```cmd
# Download please-windows-amd64.exe
# Run directly
please-windows-amd64.exe help

# Or rename for easier use
ren please-windows-amd64.exe please.exe
please.exe help
```

### Linux/macOS
```bash
# Download appropriate binary
chmod +x please-linux-amd64  # or please-macos-amd64, etc.

# Run directly
./please-linux-amd64 help

# Or install system-wide
sudo mv please-linux-amd64 /usr/local/bin/please
please help
```

## Usage

Once installed, you can use natural language to generate scripts:

```bash
please backup my documents folder
please show system memory usage  
please create a git commit script
please organize photos by date
```

## Requirements

- **No dependencies** - single binary executable
- **AI Provider**: Requires one of:
  - Ollama (local AI - recommended)
  - OpenAI API key
  - Anthropic API key

For full documentation, visit the main repository README.

---

🎉 **Happy scripting with Please!**
