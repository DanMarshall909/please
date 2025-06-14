package ui

import (
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"please/types"
)

func TestWhenSavingToHistory_ShouldWriteHistoryFile(t *testing.T) {
	temp := t.TempDir()
	if runtime.GOOS == "windows" {
		t.Setenv("APPDATA", temp)
	} else {
		t.Setenv("HOME", temp)
	}
	resp := &types.ScriptResponse{
		TaskDescription: "history task",
		Script:          "echo hi",
		ScriptType:      "bash",
		Model:           "test",
		Provider:        "p",
	}
	saveToHistory(resp)
	dir, err := getConfigDir()
	if err != nil {
		t.Fatalf("getConfigDir error: %v", err)
	}
	path := filepath.Join(dir, "script_history.json")
	data, err := os.ReadFile(path)
	if err != nil {
		t.Fatalf("expected history file: %v", err)
	}
	if !strings.Contains(string(data), "history task") {
		t.Errorf("history file missing task description: %s", data)
	}
}

func TestWhenSavingLastScript_ShouldWriteFile(t *testing.T) {
	temp := t.TempDir()
	if runtime.GOOS == "windows" {
		t.Setenv("APPDATA", temp)
	} else {
		t.Setenv("HOME", temp)
	}
	resp := &types.ScriptResponse{
		TaskDescription: "last task",
		Script:          "echo hi",
		ScriptType:      "bash",
		Model:           "m",
		Provider:        "p",
	}
	saveLastScript(resp)
	dir, err := getConfigDir()
	if err != nil {
		t.Fatalf("dir error: %v", err)
	}
	path := filepath.Join(dir, "last_script.json")
	data, err := os.ReadFile(path)
	if err != nil {
		t.Fatalf("expected last script file: %v", err)
	}
	if !strings.Contains(string(data), "last task") {
		t.Errorf("last script file missing task: %s", data)
	}
}

func TestWhenLoadingLastScriptData_ShouldReturnValues(t *testing.T) {
	temp := t.TempDir()
	if runtime.GOOS == "windows" {
		t.Setenv("APPDATA", temp)
	} else {
		t.Setenv("HOME", temp)
	}
	dir, _ := getConfigDir()
	path := filepath.Join(dir, "last_script.json")
	content := `{
  "task_description": "load task",
  "script": "echo hi",
  "script_type": "bash",
  "model": "m",
  "provider": "p"
}`
	os.WriteFile(path, []byte(content), 0644)
	resp := loadLastScriptData()
	if resp == nil || resp.TaskDescription != "load task" || resp.Script == "" {
		t.Errorf("unexpected response: %+v", resp)
	}
}

func TestWhenGetSingleKeyUnix_ShouldReturnRune(t *testing.T) {
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("a\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()
	r := getSingleKeyUnix()
	if r != 'a' {
		t.Errorf("expected 'a', got %c", r)
	}
}

func TestWhenGetSingleKeyWindows_ShouldReturnRune(t *testing.T) {
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("b\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()
	r := getSingleKeyWindows()
	if r != 'b' {
		t.Errorf("expected 'b', got %c", r)
	}
}

func TestWhenGetSingleKeyInput_ShouldReturnRune(t *testing.T) {
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("c\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()
	r := getSingleKeyInput()
	if r != 'c' {
		t.Errorf("expected 'c', got %c", r)
	}
}
