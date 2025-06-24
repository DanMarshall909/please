package ui

// MenuItem represents a single menu option
// Action returns true if the menu should exit after this action
// (for main menu, script menu, etc)
type MenuItem struct {
	Label  string
	Icon   string
	Color  string
	Action func() bool
}
