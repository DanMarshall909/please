package ui

import (
	"fmt"
	"runtime"
)

// ShowHelp displays colorful help information
func ShowHelp() {
	PrintRainbowBanner()
	fmt.Printf("\n%s%s🤖 Please - Your Overly Helpful Digital Assistant%s\n", ColorBold, ColorCyan, ColorReset)
	fmt.Printf("%s%s✨ Politely Silly AI-Powered Cross-Platform Script Generation%s\n\n", ColorBold, ColorPurple, ColorReset)

	fmt.Printf("%s📖 Natural Language Usage:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %spls%s %slist all files older than 10 years%s\n", ColorGreen, ColorReset, ColorCyan, ColorReset)
	fmt.Printf("  %splease%s %sbackup my documents folder%s\n", ColorGreen, ColorReset, ColorCyan, ColorReset)
	fmt.Printf("  %sol%s %s\"quoted strings work too\"%s %s(legacy)%s\n\n", ColorGreen, ColorReset, ColorCyan, ColorReset, ColorDim, ColorReset)

	fmt.Printf("%s🎯 Examples:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %spls%s %slist all files in the current directory%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %spls%s %screate a backup script for my documents%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %spls%s %sfind and delete temporary files%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %spls%s %smonitor system memory usage%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %spls%s %sshow system information%s\n\n", ColorGreen, ColorReset, ColorDim, ColorReset)

	fmt.Printf("%s⚙️  Setup Commands:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s--install-alias%s   %sInstall 'pls' and 'ol' shortcuts%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
	fmt.Printf("  %s--uninstall-alias%s %sRemove shortcuts%s\n", ColorGreen, ColorReset, ColorDim, ColorReset)
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
	fmt.Printf("\n%s%s🤖 Please v5.0.0%s\n", ColorBold, ColorPurple, ColorReset)
	fmt.Printf("%s✨ Your Overly Helpful Digital Assistant%s\n\n", ColorCyan, ColorReset)

	fmt.Printf("%s📦 New in v5.0:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s🗣️  Natural language interface (no quotes needed!)%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🤖 Complete rebrand from OohLama to Please%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🎪 Politely silly personality and tone%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s⚡ 'pls' shortcut (plus legacy 'ol' support)%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🌍 Cross-platform support (Windows/Linux/macOS)%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🧠 Multiple AI provider support%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s📋 Smart model auto-selection%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🛡️  Enhanced safety warnings%s\n", ColorGreen, ColorReset)
	fmt.Printf("  %s🎨 Improved user experience%s\n\n", ColorGreen, ColorReset)

	fmt.Printf("%s💻 System Information:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %sPlatform:%s %s%s%s\n", ColorCyan, ColorReset, ColorWhite, runtime.GOOS, ColorReset)
	fmt.Printf("  %sArchitecture:%s %s%s%s\n", ColorCyan, ColorReset, ColorWhite, runtime.GOARCH, ColorReset)
	fmt.Printf("  %sGo Version:%s %s%s%s\n\n", ColorCyan, ColorReset, ColorWhite, runtime.Version(), ColorReset)

	PrintFooter()
}
