package ui

import (
	"strings"
	"testing"
)

func TestWhenValidatingColorConstants_ShouldAllStartWithEscape(t *testing.T) {
	colors := []string{ColorReset, ColorRed, ColorGreen, ColorYellow, ColorBlue, ColorPurple, ColorMagenta, ColorCyan, ColorWhite, ColorBold, ColorDim,
		BgRed, BgGreen, BgYellow, BgBlue, BgPurple, BgCyan,
		Rainbow1, Rainbow2, Rainbow3, Rainbow4, Rainbow5, Rainbow6, Rainbow7}
	for _, c := range colors {
		if !strings.HasPrefix(c, "\033[") {
			t.Errorf("color %q missing escape prefix", c)
		}
	}
}

func TestWhenComparingMagentaAlias_ShouldEqualPurple(t *testing.T) {
	if ColorMagenta != ColorPurple {
		t.Errorf("expected magenta alias to equal purple")
	}
}

func TestWhenCheckingRainbowColors_ShouldAllBeUnique(t *testing.T) {
	colors := []string{Rainbow1, Rainbow2, Rainbow3, Rainbow4, Rainbow5, Rainbow6, Rainbow7}
	set := map[string]struct{}{}
	for _, c := range colors {
		if _, ok := set[c]; ok {
			t.Errorf("duplicate color %s", c)
		}
		set[c] = struct{}{}
	}
}
