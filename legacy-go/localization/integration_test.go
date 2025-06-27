package localization

import "testing"

func TestWhenInitializingManager_ShouldLoadDefaultEnglish(t *testing.T) {
	mgr, err := NewLocalizationManager("..")
	if err != nil {
		t.Fatalf("init error: %v", err)
	}
	if msg := mgr.GetMessage("footer.happy"); msg == "" {
		t.Errorf("expected default footer message")
	}
}
