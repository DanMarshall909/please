package ui

import (
	"runtime"
	"strings"
	"testing"
)

func TestWhenShowingVersion_ShouldIncludeRuntimeDetails(t *testing.T) {
	output := captureStdoutForHelp(ShowVersion)
	if !strings.Contains(output, runtime.GOOS) || !strings.Contains(output, runtime.GOARCH) || !strings.Contains(output, runtime.Version()) {
		t.Errorf("expected runtime details in version output: %s", output)
	}
}
