package types

// Banner holds banner title and subtitle
type Banner struct {
    Title    string `json:"title"`
    Subtitle string `json:"subtitle"`
}

// Errors holds error messages
type Errors struct {
	ProviderConnection string `json:"provider_connection"`
	InvalidInput       string `json:"invalid_input"`
}

// Prompts holds prompt messages
type Prompts struct {
	SelectProvider string `json:"select_provider"`
	EnterRequest   string `json:"enter_request"`
}

// Messages groups all localized messages
type Messages struct {
	Banner  Banner  `json:"banner"`
	Errors  Errors  `json:"errors"`
	Prompts Prompts `json:"prompts"`
}

// Theme defines color mappings
type Theme struct {
	Colors map[string]string `json:"colors"`
}

// LocalizationConfig represents the full localization config file
type LocalizationConfig struct {
	Language string   `json:"language"`
	Theme    string   `json:"theme"`
	Messages Messages `json:"messages"`
	Themes   Theme    `json:"themes"`
}
