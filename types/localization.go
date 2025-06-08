package types

import (
	"strings"
)

// LanguagePack represents a loaded language pack for UI strings
// Messages is a nested map: category -> key -> string (or string[] for examples)
type LanguagePack struct {
	Metadata     LanguageMetadata       `json:"metadata"`
	Messages     map[string]interface{} `json:"messages"`
	Examples     map[string][]string    `json:"examples"`
	Placeholders map[string]string      `json:"placeholders"`
}

type LanguageMetadata struct {
	Name        string `json:"name"`
	Code        string `json:"code"`
	Version     string `json:"version"`
	Author      string `json:"author"`
	Description string `json:"description"`
}

// LocalizationSystem manages language packs and string lookup
// (Stub for now, to be implemented in localization/)
type LocalizationSystem struct {
	CurrentLanguage  *LanguagePack
	AvailablePacks   map[string]*LanguagePack
	ConfigDirectory  string
	FallbackLanguage *LanguagePack
}

// Get returns a string for a given key (dot notation: category.key)
func (ls *LocalizationSystem) Get(key string, params ...interface{}) string {
	if ls == nil {
		return key
	}
	getFrom := func(pack *LanguagePack) string {
		parts := strings.Split(key, ".")
		var curr interface{} = pack.Messages
		for _, part := range parts {
			switch m := curr.(type) {
			case map[string]interface{}:
				curr = m[part]
			case map[string]string:
				curr = m[part]
			default:
				return ""
			}
		}
		if s, ok := curr.(string); ok {
			return s
		}
		return ""
	}
	if ls.CurrentLanguage != nil {
		if val := getFrom(ls.CurrentLanguage); val != "" {
			return val
		}
	}
	if ls.FallbackLanguage != nil {
		if val := getFrom(ls.FallbackLanguage); val != "" {
			return val
		}
	}
	return key
}

// GetRandom returns a random string from a category (for e.g. random success message)
func (ls *LocalizationSystem) GetRandom(category string) string {
	// TODO: Implement random selection
	return category // stub
}

// GetExample returns example strings for a category
func (ls *LocalizationSystem) GetExample(category string) []string {
	// TODO: Implement example lookup
	return nil
}
