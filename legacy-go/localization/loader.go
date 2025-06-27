package localization

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"please/types"
)

// LoadLanguagePack loads a language pack from a JSON file
func LoadLanguagePack(path string) (*types.LanguagePack, error) {
	file, err := os.Open(path)
	if err != nil {
		return nil, fmt.Errorf("failed to open language pack: %w", err)
	}
	defer file.Close()

	var pack types.LanguagePack
	decoder := json.NewDecoder(file)
	if err := decoder.Decode(&pack); err != nil {
		return nil, fmt.Errorf("failed to decode language pack: %w", err)
	}
	return &pack, nil
}

// FindLanguagePacks returns a map of code -> path for all language packs in the directory
func FindLanguagePacks(dir string) (map[string]string, error) {
	packs := make(map[string]string)
	entries, err := os.ReadDir(dir)
	if err != nil {
		return nil, err
	}
	for _, entry := range entries {
		if entry.IsDir() || filepath.Ext(entry.Name()) != ".json" {
			continue
		}
		code := entry.Name()[:len(entry.Name())-len(filepath.Ext(entry.Name()))]
		packs[code] = filepath.Join(dir, entry.Name())
	}
	return packs, nil
}

// LoadFromFile loads a LocalizationConfig from a JSON file
func LoadFromFile(path string) (*types.LocalizationConfig, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}
	var cfg types.LocalizationConfig
	if err := json.Unmarshal(data, &cfg); err != nil {
		return nil, err
	}
	return &cfg, nil
}
