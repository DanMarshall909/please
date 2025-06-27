package ui

import (
	"bytes"
	"io"
	"os"
	"runtime"
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

func Test_when_showing_help_then_display_usage_examples(t *testing.T) {
	output := captureStdoutForHelp(ShowHelp)
	if !strings.Contains(output, "Natural Language Usage") {
		t.Errorf("expected help to mention usage, got: %s", output)
	}
	if !strings.Contains(output, "Examples") {
		t.Errorf("expected help to include examples section")
	}
}

func Test_when_showing_version_then_display_version_info(t *testing.T) {
	output := captureStdoutForHelp(ShowVersion)
	if !strings.Contains(output, "Please v5.0.0") {
		t.Errorf("expected version output, got: %s", output)
	}
	if !strings.Contains(output, "System Information") {
		t.Errorf("expected system info in version output")
	}
}

func Test_when_using_injected_banner_then_invoke_banner_function(t *testing.T) {
	called := false
	showHelpWithBanner(func() { called = true })
	if !called {
		t.Error("expected banner function to be invoked")
	}
}

func Test_when_showing_version_then_include_runtime_details(t *testing.T) {
	output := captureStdoutForHelp(ShowVersion)
	if !strings.Contains(output, runtime.GOOS) {
		t.Errorf("expected GOOS %s in output", runtime.GOOS)
	}
	if !strings.Contains(output, runtime.GOARCH) {
		t.Errorf("expected GOARCH %s in output", runtime.GOARCH)
	}
	if !strings.Contains(output, runtime.Version()) {
		t.Errorf("expected Go version in output")
	}
}
