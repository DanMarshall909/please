namespace Please.UI.Services;

public class ColorService
{
    private readonly Dictionary<string, string> _colors = new()
    {
        { "Red", "\u001b[31m" },
        { "Green", "\u001b[32m" },
        { "Blue", "\u001b[34m" },
        { "Yellow", "\u001b[33m" },
        { "Cyan", "\u001b[36m" },
        { "Purple", "\u001b[35m" }
    };

    private readonly string _resetColor = "\u001b[0m";

    public string GetResetColor()
    {
        return _resetColor;
    }

    public string GetColor(string colorName)
    {
        return _colors.TryGetValue(colorName, out var color) ? color : string.Empty;
    }
}
