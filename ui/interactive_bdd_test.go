package ui

import (
	"io"
	"os"
	"runtime"
	"strings"
	"testing"

	"please/types"
)

// helper to capture stdout
func captureInteractiveOutput(fn func()) string {
	r, w, _ := os.Pipe()
	old := os.Stdout
	os.Stdout = w
	fn()
	w.Close()
	os.Stdout = old
	data, _ := io.ReadAll(r)
	return string(data)
}

func setTempHome(t *testing.T) string {
	dir := t.TempDir()
	if runtime.GOOS == "windows" {
		t.Setenv("APPDATA", dir)
	} else {
		t.Setenv("HOME", dir)
	}
	return dir
}

func TestWhenShowingMainMenuWithService_ShouldExitOnEnter(t *testing.T) {
	dir := setTempHome(t)
	ui, err := NewUIService(dir)
	if err != nil {
		t.Fatalf("failed to create service: %v", err)
	}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { ui.ShowMainMenuWithService() })
	if !strings.Contains(out, "Please Script Generator") {
		t.Errorf("expected banner text in output, got: %s", out)
	}
}

func TestWhenExecutingGreenScript_ShouldShowCompletion(t *testing.T) {
	_ = setTempHome(t)
	resp := &types.ScriptResponse{TaskDescription: "test", Script: "echo hi", ScriptType: "bash"}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { executeScript(resp) })
	if !strings.Contains(out, "Executing safe script") {
		t.Errorf("expected execute message, got: %s", out)
	}
}

func TestWhenExecutingYellowScriptAndUserCancels_ShouldSkipExecution(t *testing.T) {
	resp := &types.ScriptResponse{Script: "rm -rf temp", ScriptType: "bash"}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("n\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { executeScript(resp) })
	if !strings.Contains(out, "cancelled") {
		t.Errorf("expected cancellation message, got: %s", out)
	}
}

func TestWhenExecutingRedScriptAndUserCancels_ShouldWarnAndReturn(t *testing.T) {
	resp := &types.ScriptResponse{Script: "rm -rf /", ScriptType: "bash"}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { executeScript(resp) })
	if !strings.Contains(out, "HIGH RISK") {
		t.Errorf("expected high risk warning, got: %s", out)
	}
}

func TestWhenPrintAutoFixError_ShouldDisplaySuggestions(t *testing.T) {
	resp := &types.ScriptResponse{Script: "echo hi"}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { printAutoFixError(io.EOF, resp) })
	if !strings.Contains(out, "Auto-fix failed") {
		t.Errorf("expected failure message, got: %s", out)
	}
}

func TestWhenPrintAutoFixSuccess_ShouldShowFixedScript(t *testing.T) {
	resp := &types.ScriptResponse{Script: "echo hi\necho bye"}
	out := captureInteractiveOutput(func() { printAutoFixSuccess(resp) })
	if !strings.Contains(out, "Auto-fix applied") {
		t.Errorf("expected success message, got: %s", out)
	}
}

func TestWhenTryAutoFixUnsupportedProvider_ShouldShowError(t *testing.T) {
	setTempHome(t)
	os.Setenv("PROGRESS_TEST_MODE", "1")
	resp := &types.ScriptResponse{Script: "echo hi", ScriptType: "bash", Provider: "unknown"}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { tryAutoFix(resp, "oops") })
	if !strings.Contains(out, "Auto-fix failed") {
		t.Errorf("expected auto-fix error, got: %s", out)
	}
}

func TestWhenShowPostActionMenuWithEnter_ShouldExitImmediately(t *testing.T) {
	resp := &types.ScriptResponse{Script: "echo hi"}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { showPostActionMenu(resp) })
	if !strings.Contains(out, "Quick exit") {
		t.Errorf("expected quick exit, got: %s", out)
	}
}

func TestWhenEditScriptWithEnter_ShouldExitMenu(t *testing.T) {
	resp := &types.ScriptResponse{Script: "echo hi"}
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	out := captureInteractiveOutput(func() { editScript(resp) })
	if !strings.Contains(out, "Quick exit") {
		t.Errorf("expected quick exit, got: %s", out)
	}
}
