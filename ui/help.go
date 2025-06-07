package ui

import (
	"fmt"
	"runtime"
)

// ShowHelp displays colorful help information
func ShowHelp() {
	PrintRainbowBanner()
	fmt.Printf("\n%s%s🚀 OohLama - AI-Powered Cross-Platform Script Generator%s\n\n", ColorBold, ColorCyan, ColorReset)

	fmt.Printf("%s📖 Usage:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %sol%s %s<task description>%s\n", ColorGreen, ColorReset, ColorCyan, ColorReset)
	fmt.Printf("  %soohlama%s %s<task description>%s\n\n", ColorGreen, ColorReset, ColorCyan, ColorReset)

	fmt.Printf("%s🎯 Examples:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %sol%s %s\"list all files in the current directory\"%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %sol%s %s\"create a backup script for my documents\"%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %sol%s %s\"find and delete temporary files\"%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %sol%s %s\"monitor system resources\"%s\n\n", ColorGreen, ColorReset, ColorDim, ColorReset)

	fmt.Printf("%s⚙️  Setup Commands:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s--install-alias%s   %sInstall 'ol' shortcut%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %s--uninstall-alias%s %sRemove 'ol' shortcut%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %s--version%s         %sShow version information%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %s--help, -h%s        %sShow this help message%s\n\n", ColorGreen, ColorReset, ColorDim, ColorReset)

	fmt.Printf("%s🎨 Features:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s🌍 Cross-platform%s (Windows PowerShell, Linux/macOS Bash)\n", ColorCyan, ColorReset)
	fmt.Printf("  %s🧠 Multiple AI providers%s (Ollama, OpenAI, Anthropic)\n", ColorCyan, ColorReset)
	fmt.Printf("  %s📋 Smart model selection%s (automatically picks best model)\n", ColorCyan, ColorReset)
	fmt.Printf("  %s🛡️  Safety first%s (always shows script before execution)\n", ColorCyan, ColorReset)
	fmt.Printf("  %s🎯 Multiple output options%s (clipboard, execute, save)\n\n", ColorCyan, ColorReset)

	PrintFooter()
}

// ShowVersion displays version information with colors
func ShowVersion() {
	PrintRainbowBanner()
	fmt.Printf("\n%s%s🤖 OohLama v2.0.0%s\n", ColorBold, ColorPurple, ColorReset)
	fmt.Printf("%s🌟 AI-Powered Cross-Platform Script Generator%s\n\n", ColorCyan, ColorReset)

	fmt.Printf("%s📦 Features in this version:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s✨ Colorful and interactive interface%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🌍 Cross-platform support (Windows/Linux/macOS)%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🧠 Multiple AI provider support%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s📋 Smart model auto-selection%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s⚡ Short 'ol' alias support%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🎨 Enhanced user experience%s\n\n", ColorGreen, ColorReset)

	fmt.Printf("%s💻 System Information:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %sPlatform:%s %s%s%s\n", ColorCyan, ColorReset, ColorWhite, runtime.GOOS, ColorReset)
	fmt.Printf("  %sArchitecture:%s %s%s%s\n", ColorCyan, ColorReset, ColorWhite, runtime.GOARCH, ColorReset)
	fmt.Printf("  %sGo Version:%s %s%s%s\n\n", ColorCyan, ColorReset, ColorWhite, runtime.Version(), ColorReset)

	PrintFooter()
}
