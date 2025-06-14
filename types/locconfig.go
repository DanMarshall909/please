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

// Installation holds installation messages
type Installation struct {
	Success string `json:"success"`
	TryIt   string `json:"try_it"`
	Magic   string `json:"magic"`
}

// Footer holds footer messages
type Footer struct {
	Tips  string `json:"tips"`
	Happy string `json:"happy"`
}

// Messages groups all localized messages
type Messages struct {
	Banner       Banner       `json:"banner"`
	Errors       Errors       `json:"errors"`
	Prompts      Prompts      `json:"prompts"`
	Installation Installation `json:"installation"`
	Footer       Footer       `json:"footer"`
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
