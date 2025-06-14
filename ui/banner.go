package ui

import (
	"fmt"
	"time"

	"please/localization"
)

var locMgr *localization.LocalizationManager

// SetLocalizationManager sets the manager for banner messages
func SetLocalizationManager(mgr *localization.LocalizationManager) {
	locMgr = mgr
}

// PrintRainbowBanner displays a colorful animated banner
func PrintRainbowBanner() {
	PrintRainbowBannerWithDelay(50 * time.Millisecond)
}

// PrintRainbowBannerWithDelay displays a colorful banner with configurable delay
func PrintRainbowBannerWithDelay(delay time.Duration) {
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
		if delay > 0 {
			time.Sleep(delay)
		}
	}

	if locMgr != nil {
		title := locMgr.GetMessage("banner.title")
		subtitle := locMgr.GetMessage("banner.subtitle")
		if title != "" {
			fmt.Printf("%s%s%s\n", ColorCyan, title, ColorReset)
		}
		if subtitle != "" {
			fmt.Printf("%s%s%s\n", ColorPurple, subtitle, ColorReset)
		}
	}
}

// PrintInstallationSuccess shows a fun success message
func PrintInstallationSuccess() {
	success := "🎉 Installation complete! 🎉"
	tryIt := "🚀 Try it out:"
	magic := "✨ Magic happens with just 3 letters: 'pls' ✨"
	if locMgr != nil {
		if v := locMgr.GetMessage("installation.success"); v != "" {
			success = v
		}
		if v := locMgr.GetMessage("installation.try_it"); v != "" {
			tryIt = v
		}
		if v := locMgr.GetMessage("installation.magic"); v != "" {
			magic = v
		}
	}

	fmt.Printf("%s%s%s\n\n", ColorBold+ColorGreen, success, ColorReset)
	fmt.Printf("%s%s%s\n", ColorBold+ColorCyan, tryIt, ColorReset)
	fmt.Printf("  %spls create a hello world script%s\n", ColorYellow, ColorReset)
	fmt.Printf("  %sol create a hello world script%s (legacy)\n\n", ColorYellow, ColorReset)

	// Fun ASCII art
	fmt.Printf("%s    %s%s\n", ColorPurple, magic, ColorReset)
}

// PrintFooter shows colorful footer information
func PrintFooter() {
	tips := "💡 Tips:"
	happy := "🌟 Happy scripting! 🌟"
	if locMgr != nil {
		if v := locMgr.GetMessage("footer.tips"); v != "" {
			tips = v
		}
		if v := locMgr.GetMessage("footer.happy"); v != "" {
			happy = v
		}
	}

	fmt.Printf("%s%s%s\n", ColorBold+ColorYellow, tips, ColorReset)
	fmt.Printf("  %s• Use natural language - no quotes needed!%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Be specific for better results%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Always review scripts before execution%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Set PLEASE_PROVIDER=openai for OpenAI%s\n", ColorCyan, ColorReset)
	fmt.Printf("  %s• Set PLEASE_PROVIDER=anthropic for Claude%s\n\n", ColorCyan, ColorReset)

	fmt.Printf("%s%s%s\n", ColorBold+ColorPurple, happy, ColorReset)
}
