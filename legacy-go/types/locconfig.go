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

// ScriptDisplay holds script display messages
type ScriptDisplay struct {
	TaskLabel     string `json:"task_label"`
	ModelLabel    string `json:"model_label"`
	PlatformLabel string `json:"platform_label"`
	ScriptHeader  string `json:"script_header"`
	SuccessMessage string `json:"success_message"`
}

// Menu holds menu item messages
type Menu struct {
	GenerateScript  string `json:"generate_script"`
	RunLast        string `json:"run_last"`
	Help           string `json:"help"`
	Exit           string `json:"exit"`
	MainPrompt     string `json:"main_prompt"`
	ShowHelp       string `json:"show_help"`
	LoadLast       string `json:"load_last"`
	BrowseHistory  string `json:"browse_history"`
	ShowConfig     string `json:"show_config"`
}

// Menus holds menu messages (alias for Menu for backward compatibility)
type Menus struct {
	ShowHelp       string `json:"show_help"`
	GenerateScript string `json:"generate_script"`
	LoadLast       string `json:"load_last"`
	BrowseHistory  string `json:"browse_history"`
	ShowConfig     string `json:"show_config"`
	Exit           string `json:"exit"`
	MainPrompt     string `json:"main_prompt"`
}

// Success holds success messages
type Success struct {
	Exit      string `json:"exit"`
	ExitQuick string `json:"exit_quick"`
}

// Messages groups all localized messages
type Messages struct {
	Banner        Banner        `json:"banner"`
	Errors        Errors        `json:"errors"`
	Prompts       Prompts       `json:"prompts"`
	Installation  Installation  `json:"installation"`
	Footer        Footer        `json:"footer"`
	ScriptDisplay ScriptDisplay `json:"script_display"`
	Menu          Menu          `json:"menu"`
	Menus         Menus         `json:"menus"`
	Success       Success       `json:"success"`
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
