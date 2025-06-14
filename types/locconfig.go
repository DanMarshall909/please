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

// ScriptDisplay holds script display messages
type ScriptDisplay struct {
	TaskLabel      string `json:"task_label"`
	ModelLabel     string `json:"model_label"`
	PlatformLabel  string `json:"platform_label"`
	ScriptHeader   string `json:"script_header"`
	SuccessMessage string `json:"success_message"`
}

// Menu holds menu item labels
type Menu struct {
	GenerateScript string `json:"generate_script"`
	RunLast        string `json:"run_last"`
	Help           string `json:"help"`
	Exit           string `json:"exit"`
}

// Messages groups all localized messages
type Messages struct {
	Banner        Banner        `json:"banner"`
	Errors        Errors        `json:"errors"`
	Prompts       Prompts       `json:"prompts"`
	ScriptDisplay ScriptDisplay `json:"script_display"`
	Menu          Menu          `json:"menu"`
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
