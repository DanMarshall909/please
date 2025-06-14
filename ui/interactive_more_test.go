package ui

import (
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"please/types"
)

func Test_when_saving_to_history_then_write_history_file(t *testing.T) {
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

func Test_when_saving_last_script_then_write_file(t *testing.T) {
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

func Test_when_loading_last_script_data_then_return_values(t *testing.T) {
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

func Test_when_get_single_key_unix_then_return_rune(t *testing.T) {
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

func Test_when_get_single_key_windows_then_return_rune(t *testing.T) {
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

func Test_when_get_single_key_input_then_return_rune(t *testing.T) {
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

func Test_when_generate_new_script_with_empty_input_then_show_warning(t *testing.T) {
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	output := captureOutput(func() { generateNewScript() })
	if !strings.Contains(output, "No task description provided") {
		t.Errorf("expected warning for empty input, got: %s", output)
	}
}

func Test_when_generate_new_script_with_valid_input_then_announce_generation(t *testing.T) {
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("test task\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()

	output := captureOutput(func() { generateNewScript() })
	if !strings.Contains(output, "Generating script for: test task") {
		t.Errorf("expected generation message, got: %s", output)
	}
}

func Test_when_save_to_file_then_create_script_file(t *testing.T) {
	tmpDir := t.TempDir()
	tmp, _ := os.CreateTemp(t.TempDir(), "stdin")
	tmp.WriteString("\n")
	tmp.Seek(0, 0)
	old := os.Stdin
	os.Stdin = tmp
	defer func() { os.Stdin = old }()
	oldWd, _ := os.Getwd()
	os.Chdir(tmpDir)
	defer os.Chdir(oldWd)

	resp := &types.ScriptResponse{TaskDescription: "hello world", Script: "echo hi", ScriptType: "bash"}
	saveToFile(resp)
	files, _ := os.ReadDir(tmpDir)
	if len(files) == 0 {
		t.Error("expected file to be created")
	}
}
