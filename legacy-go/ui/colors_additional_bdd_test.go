package ui

import "testing"

func TestWhenCountingRainbowColors_ShouldEqualSeven(t *testing.T) {
	colors := []string{Rainbow1, Rainbow2, Rainbow3, Rainbow4, Rainbow5, Rainbow6, Rainbow7}
	if len(colors) != 7 {
		t.Errorf("expected 7 rainbow colors, got %d", len(colors))
	}
}
