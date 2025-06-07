package ui

import (
	"fmt"
	"time"
)

// PrintRainbowBanner displays a colorful animated banner
func PrintRainbowBanner() {
	banner := []string{
		"██████╗ ██╗     ███████╗ █████╗ ███████╗███████╗",
		"██╔══██╗██║     ██╔════╝██╔══██╗██╔════╝██╔════╝",
		"██████╔╝██║     █████╗  ███████║███████╗█████╗  ",
		"██╔═══╝ ██║     ██╔══╝  ██╔══██║╚════██║██╔══╝  ",
		"██║     ███████╗███████╗██║  ██║███████║███████╗",
		"╚═╝     ╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝",
	}

	colors := []string{Rainbow1, Rainbow2, Rainbow3, Rainbow4, Rainbow5, Rainbow6, Rainbow7}

	for i, line := range banner {
		color := colors[i%len(colors)]
		fmt.Printf("%s%s%s\n", color, line, ColorReset)
		time.Sleep(50 * time.Millisecond) // Slight animation delay
	}
}

// PrintInstallationSuccess shows a fun success message
func PrintInstallationSuccess() {
	fmt.Printf("%s🎉 Installation complete! 🎉%s\n\n", ColorBold+ColorGreen, ColorReset)
	fmt.Printf("%s🚀 Try it out:%s\n", ColorBold+ColorCyan, ColorReset)
	fmt.Printf("  %spls create a hello world script%s\n", ColorYellow, ColorReset)
	fmt.Printf("  %sol create a hello world script%s (legacy)\n\n", ColorYellow, ColorReset)

	// Fun ASCII art
	fmt.Printf("%s    ✨ Magic happens with just 3 letters: 'pls' ✨%s\n", ColorPurple, ColorReset)
}

// PrintFooter shows colorful footer information
func PrintFooter() {
	fmt.Printf("%s💡 Tips:%s\n", ColorBold+ColorYellow, ColorReset)
	fmt.Printf("  %s• Use natural language - no quotes needed!%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Be specific for better results%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Always review scripts before execution%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Set PLEASE_PROVIDER=openai for OpenAI%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Set PLEASE_PROVIDER=anthropic for Claude%s\n\n", ColorCyan, ColorReset)

	fmt.Printf("%s🌟 Happy scripting! 🌟%s\n", ColorBold+ColorPurple, ColorReset)
}
