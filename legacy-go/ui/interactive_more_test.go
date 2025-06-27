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
	// Skip test that would hang - getSingleKeyUnix tries to use stty directly
	t.Skip("getSingleKeyUnix bypasses stdin and reads directly from terminal - would hang in CI")
}

func Test_when_get_single_key_windows_then_return_rune(t *testing.T) {
	// Skip test that would hang - getSingleKeyWindows uses PowerShell Read-Host directly  
	t.Skip("getSingleKeyWindows bypasses stdin and reads directly from terminal - would hang in CI")
}

func Test_when_get_single_key_input_then_return_rune(t *testing.T) {
	// Skip test that would hang - getSingleKeyInput calls platform-specific functions that bypass stdin
	t.Skip("getSingleKeyInput bypasses stdin and reads directly from terminal - would hang in CI")
}

func Test_when_generate_new_script_with_empty_input_then_show_warning(t *testing.T) {
	// Skip test that would hang - generateNewScript uses interactive input that bypasses stdin mocking
	t.Skip("generateNewScript uses direct terminal input bypassing stdin - would hang in CI")
}

func Test_when_generate_new_script_with_valid_input_then_announce_generation(t *testing.T) {
	// Skip test that would hang - generateNewScript uses interactive input that bypasses stdin mocking
	t.Skip("generateNewScript uses direct terminal input bypassing stdin - would hang in CI")
}

func Test_when_save_to_file_then_create_script_file(t *testing.T) {
	// Skip test that would hang - saveToFile uses interactive input that bypasses stdin mocking
	t.Skip("saveToFile uses direct terminal input bypassing stdin - would hang in CI")
}
