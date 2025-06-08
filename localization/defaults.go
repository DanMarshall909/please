package localization

import "please/types"

// DefaultEnglishSilly returns the built-in silly English language pack
func DefaultEnglishSilly() *types.LanguagePack {
	return &types.LanguagePack{
		Metadata: types.LanguageMetadata{
			Name:        "English (Silly)",
			Code:        "en-us-silly",
			Version:     "1.0.0",
			Author:      "Please Team",
			Description: "Default silly tone for English speakers",
		},
		Messages: map[string]interface{}{
			"success": map[string]string{
				"script_generated": "✨ Ta-da! Your script is ready and looking fabulous!",
				"script_saved":     "🎉 Script saved successfully! *chef's kiss*",
				"exit":             "✨ Ta-da! Thanks for using Please! Happy scripting! 🎉",
				"exit_quick":       "✨ Quick exit! Thanks for using Please! 🎉",
			},
			"errors": map[string]string{
				"general":         "Oops! Something went sideways 🙃 (but don't worry, I still love you)",
				"provider_failed": "🤔 Hmm, that didn't work as planned. Let's try again, shall we?",
				"file_not_found":  "💔 Aww shucks! Can't find that file anywhere",
				"invalid_choice":  "❌ Invalid choice. Please try again.",
			},
			"menus": map[string]string{
				"main_prompt":     "🎯 What would you like to do?",
				"show_help":       "Show help & usage",
				"generate_script": "Generate new script",
				"load_last":       "Load last script",
				"browse_history":  "Browse history",
				"show_config":     "Show configuration",
				"exit":            "Exit",
			},
		},
		Examples: map[string][]string{
			"file_operations": {
				"backup my important documents",
				"organize photos by date taken",
				"find duplicate files in downloads",
			},
		},
		Placeholders: map[string]string{
			"task_description": "What would you like me to help you with?",
		},
	}
}
