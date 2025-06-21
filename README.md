# Please v5.0 - Your Overly Helpful Digital Assistant

![Please Banner](https://img.shields.io/badge/Please-v5.0-blue?style=for-the-badge&logo=robot)
![Go Version](https://img.shields.io/badge/Go-1.21+-00ADD8?style=for-the-badge&logo=go)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey?style=for-the-badge)

**Please** is a politely silly AI-powered script generator that turns natural language into safe, executable scripts across Windows PowerShell, Linux Bash, and macOS.

## 🌟 New in v5.0: AI Test Monitoring

**Please** now includes intelligent test failure analysis! Run your tests and get AI-powered insights into failures with detailed recommendations for fixes.

## 🚀 Quick Start

### Installation
```bash
# Download the latest release for your platform
# Windows: please.exe
# Linux/macOS: please

# Install shortcuts (optional)
please --install-alias
```

### Basic Usage
```bash
# Natural language - no quotes needed!
pls list all files older than 10 years
pls backup my documents folder
pls find and delete temporary files

# With quotes (if you prefer)
please "show system information"
```

### 🧪 AI Test Monitoring
```bash
# Monitor all tests with AI analysis
pls --test-monitor

# Monitor specific test pattern
pls --test-monitor TestParseFailures

# Alternative command
pls --monitor-tests
```

## 🎯 Key Features

### 🧠 Smart AI Integration
- **Multiple Providers**: Ollama (local), OpenAI, Anthropic Claude
- **Automatic Model Selection**: Picks the best model for your task
- **Context-Aware**: Understands your platform and requirements

### 🛡️ Safety First
- **Script Preview**: Always shows generated scripts before execution
- **Intelligent Validation**: Warns about potentially dangerous operations
- **Cross-Platform**: Generates appropriate scripts for your OS

### 🧪 AI Test Monitoring
- **Automatic Failure Detection**: Parses Go test output for failures
- **Intelligent Analysis**: AI analyzes failures and provides detailed insights
- **Structured Recommendations**: Get categorized suggestions and code fixes
- **Report Generation**: Saves detailed failure reports for future reference

## 📖 Test Monitoring in Detail

When you run `pls --test-monitor`, the system:

1. **Executes Tests**: Runs `go test -v ./...` (or your specified pattern)
2. **Captures Failures**: Parses test output to extract failure details
3. **AI Analysis**: Sends failure context to your configured AI provider
4. **Structured Insights**: Displays formatted analysis with:
   - **Summary**: Brief description of what went wrong
   - **Root Cause**: Detailed explanation of the underlying issue
   - **Suggestions**: Specific recommendations to fix the problem
   - **Code Fixes**: Suggested code changes (when applicable)
   - **Test Strategy**: Recommendations for better testing approaches
   - **Priority Actions**: Ranked steps to resolve the issue

### Example AI Analysis Output
```
🤖 AI TEST FAILURE ANALYSIS
═══════════════════════════════════════════════

🔍 Test: TestParseTestFailures
📁 Package: script
📄 File: test_monitor_test.go:37
🏷️  Category: assertion_failure

📝 Summary:
Test expectations don't match the actual parsing behavior due to incorrect indexing of parsed failures.

🎯 Root Cause:
The test assumes failures are parsed in a specific order, but the parsing logic processes them differently than expected.

💡 Suggestions:
1. Check the actual order of failures in the test output
2. Debug the parseTestFailures function to understand the parsing sequence
3. Update test expectations to match actual behavior
4. Consider making the parser more deterministic

🔧 Suggested Code Fix:
// Fix test expectations to match actual parsing behavior
// Debug first: print failures[0] and failures[1] to see actual values

📋 Recommended Actions:
1. [HIGH] Debug Test: Add debug prints to see actual parsed values
2. [MEDIUM] Fix Assertions: Update test expectations to match reality
3. [LOW] Improve Parser: Make parsing order more predictable
```

## ⚙️ Configuration

### Environment Variables
```bash
# Set AI Provider
export PLEASE_PROVIDER=openai    # or anthropic, ollama
export OPENAI_API_KEY=your_key
export ANTHROPIC_API_KEY=your_key

# Set preferred model (optional)
export PLEASE_MODEL=gpt-4o-mini  # or claude-3-haiku-20240307, llama3.2
```

### Config File
Configuration is stored in `~/.please/config.json`:
```json
{
  "provider": "ollama",
  "preferred_model": "llama3.2",
  "script_type": "auto",
  "openai_api_key": "",
  "anthropic_api_key": "",
  "ollama_url": "http://localhost:11434"
}
```

## 🎨 Examples

### Script Generation
```bash
# File operations
pls create a backup script for my documents folder
pls list all files larger than 100MB
pls organize photos by date into folders

# System administration  
pls show disk usage by directory
pls check for running services on port 80
pls create a system health check script

# Development tasks
pls build and test my Go project
pls set up a Git pre-commit hook
pls generate a project structure for a web app
```

### Test Monitoring
```bash
# Basic monitoring
pls --test-monitor

# Specific test pattern
pls --test-monitor TestHTTP
pls --test-monitor "TestDatabase.*"

# Monitor specific package
cd mypackage && pls --test-monitor
```

## 🔧 Advanced Usage

### Multiple Providers
```bash
# Use different providers for different tasks
PLEASE_PROVIDER=anthropic pls analyze this complex algorithm
PLEASE_PROVIDER=openai pls write unit tests for my function
PLEASE_PROVIDER=ollama pls create a simple backup script
```

### Custom Models
```bash
# Use specific models
PLEASE_MODEL=gpt-4o pls create a complex deployment script
PLEASE_MODEL=claude-3-opus-20240229 pls review this code for security issues
```

## 📁 Project Structure

```
please/
├── main.go              # Main application entry point
├── config/              # Configuration management
├── providers/           # AI provider implementations
│   ├── ollama.go       # Local Ollama integration
│   ├── openai.go       # OpenAI API integration
│   └── anthropic.go    # Anthropic Claude integration
├── script/              # Script operations and test monitoring
│   ├── operations.go   # Script execution, validation, etc.
│   ├── test_monitor.go # AI-powered test failure analysis
│   └── editor.go       # Interactive script editing
├── ui/                  # User interface components
├── models/              # Model selection logic
└── types/               # Type definitions
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎪 Why "Please"?

Because we believe in polite computing! **Please** is your courteous digital assistant that:
- Always asks before executing potentially dangerous operations
- Explains what it's doing in friendly, understandable language
- Treats your system with respect and care
- Maintains a delightfully silly yet helpful personality

## 🆘 Support

- **Documentation**: Check the `--help` flag for detailed usage information
- **Issues**: Report bugs and request features via GitHub Issues
- **Discussions**: Join the conversation in GitHub Discussions

## 🌟 Acknowledgments

- **Ollama**: For making local AI accessible and easy
- **OpenAI**: For advancing AI accessibility
- **Anthropic**: For Claude's thoughtful AI capabilities
- **Go Community**: For the amazing tools and ecosystem

---

*Happy scripting with Please! 🎉*

## 🔨 Building the Legacy Go CLI

The Go implementation lives in the `legacy` folder. Use the provided workspace file to build from the repository root or run the build script inside `legacy`.

```bash
# Build for the current platform from the repository root
GOOS=$(go env GOOS) GOARCH=$(go env GOARCH) go build -ldflags="-s -w" -o releases/please-$GOOS-$GOARCH ./legacy

# Or run the cross-platform build script
cd legacy && bash build.sh
```

All Go packages should compile and their tests should pass:

```bash
cd legacy && go test ./...
cd ../ui && go test ./...
```

