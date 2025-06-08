package localization

import (
	"os"
	"path/filepath"
	"please/types"
)

// LocalizationManager manages the active language pack and available packs
// (Stub for now, to be expanded)
type LocalizationManager struct {
	System *types.LocalizationSystem
}

// NewLocalizationManager initializes the manager and loads available packs
func NewLocalizationManager(configDir string) (*LocalizationManager, error) {
	langDir := filepath.Join(configDir, "languages")
	packs, err := FindLanguagePacks(langDir)
	if err != nil && !os.IsNotExist(err) {
		return nil, err
	}
	ls := &types.LocalizationSystem{
		AvailablePacks:  make(map[string]*types.LanguagePack),
		ConfigDirectory: configDir,
	}
	for code, path := range packs {
		pack, err := LoadLanguagePack(path)
		if err == nil {
			ls.AvailablePacks[code] = pack
		}
	}
	// Always add built-in silly English as fallback
	defaultPack := DefaultEnglishSilly()
	ls.FallbackLanguage = defaultPack
	if len(ls.AvailablePacks) == 0 {
		ls.CurrentLanguage = defaultPack
	} else {
		// Use first available pack as current (improve later)
		for _, p := range ls.AvailablePacks {
			ls.CurrentLanguage = p
			break
		}
	}
	return &LocalizationManager{System: ls}, nil
}
