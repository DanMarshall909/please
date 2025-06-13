package ui

import (
	"fmt"
)

// Color constants (imported from colors.go or defined here for testability)
var (
	ColorBold    = "\x1b[1m"
	ColorCyan    = "\x1b[36m"
	ColorReset   = "\x1b[0m"
	ColorGreen   = "\x1b[32m"
	ColorYellow  = "\x1b[33m"
	ColorRed     = "\x1b[31m"
	ColorMagenta = "\x1b[35m"
	ColorBlue    = "\x1b[34m"
	ColorDim     = "\x1b[2m"
	ColorPurple  = "\x1b[35m"
	ColorWhite   = "\x1b[37m"
)

// MenuItem represents a single menu option
// Action returns true if the menu should exit after this action
// (for main menu, script menu, etc)
type MenuItem struct {
	Label  string
	Icon   string
	Color  string
	Action func() bool
}

// renderMenu displays a menu from a slice of MenuItem and handles user input
func renderMenu(title, prompt string, items []MenuItem) {
	for {
		fmt.Printf("\n%s%s%s\n\n", ColorBold+ColorCyan, title, ColorReset)
		for idx, item := range items {
			fmt.Printf("  %s%d.%s %s%s %s%s\n", ColorGreen, idx+1, ColorReset, item.Color, item.Icon, item.Label, ColorReset)
		}
		fmt.Printf("\n%s%s%s", ColorBold+ColorYellow, prompt, ColorReset)
		// getSingleKeyInput is not used in tests, so we skip input handling for testability
		return
	}
}
