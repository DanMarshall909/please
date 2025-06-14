package ui

import (
	"testing"
	"time"
)

// Test banner functions for business logic only (no cosmetic testing)

func Test_when_banner_with_zero_delay_should_complete_immediately(t *testing.T) {
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

func Test_when_banner_with_delay_should_respect_timing(t *testing.T) {
	// Arrange
	testDelay := 5 * time.Millisecond
	start := time.Now()

	// Act
	PrintRainbowBannerWithDelay(testDelay)

	// Assert - Should take approximately the expected time (6 lines * delay)
	duration := time.Since(start)
	expectedMin := 6 * testDelay                   // 6 lines minimum
	expectedMax := expectedMin + 10*time.Millisecond // small overhead allowance
	
	if duration < expectedMin || duration > expectedMax {
		t.Errorf("Expected banner with %v delay to take %v-%v, took %v", 
			testDelay, expectedMin, expectedMax, duration)
	}
}
