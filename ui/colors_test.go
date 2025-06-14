package ui

import (
	"strings"
	"testing"
)

// TestWhenCheckingColorConstants_ShouldProvideEscapeCodes verifies color constants are escape sequences
func Test_when_checking_color_constants_then_provide_escape_codes(t *testing.T) {
	colors := []string{ColorReset, ColorRed, ColorGreen, ColorYellow, ColorBlue, ColorPurple, ColorMagenta, ColorCyan, ColorWhite, ColorBold, ColorDim,
		BgRed, BgGreen, BgYellow, BgBlue, BgPurple, BgCyan,
		Rainbow1, Rainbow2, Rainbow3, Rainbow4, Rainbow5, Rainbow6, Rainbow7}
	for _, c := range colors {
		if !strings.HasPrefix(c, "\033[") {
			t.Errorf("color constant %q does not look like escape code", c)
		}
	}
}

// TestWhenCheckingColorAliases_ShouldMatch ensures magenta equals purple
func Test_when_checking_color_aliases_then_match(t *testing.T) {
	if ColorMagenta != ColorPurple {
		t.Errorf("expected ColorMagenta to equal ColorPurple")
	}
}

// TestWhenCheckingRainbowColors_ShouldBeUnique verifies rainbow colors are unique
func Test_when_checking_rainbow_colors_then_be_unique(t *testing.T) {
	colors := []string{Rainbow1, Rainbow2, Rainbow3, Rainbow4, Rainbow5, Rainbow6, Rainbow7}
	seen := make(map[string]bool)
	for _, c := range colors {
		if seen[c] {
			t.Errorf("duplicate rainbow color: %q", c)
		}
		seen[c] = true
	}
}
