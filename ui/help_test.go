package ui

import (
	"bytes"
	"io"
	"os"
	"strings"
	"testing"
)

func captureStdoutForHelp(fn func()) string {
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
	return <-outC
}

func TestWhenShowingHelp_ShouldDisplayUsageExamples(t *testing.T) {
	output := captureStdoutForHelp(ShowHelp)
	if !strings.Contains(output, "Natural Language Usage") {
		t.Errorf("expected help to mention usage, got: %s", output)
	}
	if !strings.Contains(output, "Examples") {
		t.Errorf("expected help to include examples section")
	}
}

func TestWhenShowingVersion_ShouldDisplayVersionInfo(t *testing.T) {
	output := captureStdoutForHelp(ShowVersion)
	if !strings.Contains(output, "Please v5.0.0") {
		t.Errorf("expected version output, got: %s", output)
	}
	if !strings.Contains(output, "System Information") {
		t.Errorf("expected system info in version output")
	}
}

func TestWhenUsingInjectedBanner_ShouldInvokeBannerFunction(t *testing.T) {
	called := false
	showHelpWithBanner(func() { called = true })
	if !called {
		t.Error("expected banner function to be invoked")
	}
}
