package ui

import (
	"bytes"
	"io"
	"os"
	"strings"
	"testing"
	"time"
)

// Test banner functions for business logic only (no cosmetic testing)

func Test_when_banner_with_zero_delay_then_complete_immediately(t *testing.T) {
	// Arrange
	start := time.Now()

	// Act - Test zero delay performance
	PrintRainbowBannerWithDelay(0)

	// Assert - Should complete nearly instantly
	duration := time.Since(start)
	if duration > 10*time.Millisecond {
		t.Errorf("Expected zero delay banner to complete in <10ms, took %v", duration)
	}
}

func Test_when_banner_with_delay_then_respect_timing(t *testing.T) {
	// Arrange
	testDelay := 5 * time.Millisecond
	start := time.Now()

	// Act
	PrintRainbowBannerWithDelay(testDelay)

	// Assert - Should take approximately the expected time (6 lines * delay)
	duration := time.Since(start)
	expectedMin := 6 * testDelay                     // 6 lines minimum
	expectedMax := expectedMin + 10*time.Millisecond // small overhead allowance

	if duration < expectedMin || duration > expectedMax {
		t.Errorf("Expected banner with %v delay to take %v-%v, took %v",
			testDelay, expectedMin, expectedMax, duration)
	}
}

func captureStdout(fn func()) string {
	r, w, _ := os.Pipe()
	orig := os.Stdout
	os.Stdout = w
	outC := make(chan string)
	go func() {
		var buf bytes.Buffer
		io.Copy(&buf, r)
		outC <- buf.String()
	}()
	fn()
	w.Close()
	os.Stdout = orig
	out := <-outC
	return out
}

func Test_when_printing_rainbow_banner_with_zero_delay_then_print_ascii_art(t *testing.T) {
	output := captureStdout(func() { PrintRainbowBannerWithDelay(0) })
	if !strings.Contains(output, "██████╗") {
		t.Errorf("Expected banner to include ASCII art, got: %s", output)
	}
	lineCount := strings.Count(output, "\n")
	if lineCount < 6 {
		t.Errorf("Expected banner to print at least 6 lines, got %d", lineCount)
	}
}

func Test_when_calling_print_installation_success_then_show_magic_message(t *testing.T) {
	output := captureStdout(PrintInstallationSuccess)
	if !strings.Contains(output, "Installation complete") {
		t.Errorf("Expected installation message, got: %s", output)
	}
	if !strings.Contains(output, "Magic happens") {
		t.Errorf("Expected magic message, got: %s", output)
	}
}

func Test_when_calling_print_footer_then_display_helpful_tips(t *testing.T) {
	output := captureStdout(PrintFooter)
	if !strings.Contains(output, "Happy scripting") {
		t.Errorf("Expected footer tips, got: %s", output)
	}
	if !strings.Contains(output, "Use natural language") {
		t.Errorf("Expected usage tips, got: %s", output)
	}
}

func Test_when_calling_print_rainbow_banner_then_use_default_delay(t *testing.T) {
	start := time.Now()
	PrintRainbowBanner()
	if time.Since(start) < 50*time.Millisecond {
		t.Errorf("Expected default delay to be at least 50ms")
	}
}
