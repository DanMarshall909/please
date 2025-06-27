using Please.Domain.Enums;
using Please.Infrastructure.Services;
using Shouldly;

namespace Please.Infrastructure.UnitTests.Services;

public class SyntaxHighlightingServiceTests
{
    private readonly SyntaxHighlightingService _syntaxHighlightingService;

    public SyntaxHighlightingServiceTests()
    {
        _syntaxHighlightingService = new SyntaxHighlightingService();
    }

    [Fact]
    public void Highlight_powershell_script_applies_syntax_coloring()
    {
        // Arrange
        var script = "Get-Process | Where-Object { $_.Name -eq 'notepad' }";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldNotBe(script); // Should be different due to markup
        result.ShouldContain("["); // Should contain markup tags
    }

    [Fact]
    public void Highlight_bash_script_applies_syntax_coloring()
    {
        // Arrange
        var script = "#!/bin/bash\necho \"Hello World\"\nls -la";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.Bash);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldNotBe(script); // Should be different due to markup
        result.ShouldContain("["); // Should contain markup tags
    }

    [Fact]
    public void Highlight_powershell_keywords_get_colored()
    {
        // Arrange
        var script = "if ($true) { Write-Host 'test' }";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell);

        // Assert
        result.ShouldContain("[bold magenta]"); // Keywords should be magenta
    }

    [Fact]
    public void Highlight_powershell_variables_get_colored()
    {
        // Arrange
        var script = "$myVariable = 'test'";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell);

        // Assert
        result.ShouldContain("[bold yellow]"); // Variables should be yellow
    }

    [Fact]
    public void Highlight_powershell_strings_get_colored()
    {
        // Arrange
        var script = "Write-Host 'Hello World'";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell);

        // Assert
        result.ShouldContain("[green]"); // Strings should be green
    }

    [Fact]
    public void Highlight_powershell_commands_get_colored()
    {
        // Arrange
        var script = "Get-Process";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell);

        // Assert
        result.ShouldContain("[bold cyan]"); // Commands should be cyan
    }

    [Fact]
    public void Highlight_bash_comments_get_colored()
    {
        // Arrange
        var script = "# This is a comment\necho 'hello'";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.Bash);

        // Assert
        result.ShouldContain("[dim green]"); // Comments should be dim green
    }

    [Fact]
    public void Highlight_bash_variables_get_colored()
    {
        // Arrange
        var script = "echo $HOME";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.Bash);

        // Assert
        result.ShouldContain("[bold yellow]"); // Variables should be yellow
    }

    [Fact]
    public void Highlight_unknown_script_type_returns_escaped_text()
    {
        // Arrange
        var script = "some random text";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.Command);

        // Assert
        result.ShouldBe(script); // Should return original text for unknown types
    }

    [Fact]
    public void Highlight_empty_script_returns_empty_string()
    {
        // Arrange
        var script = "";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell);

        // Assert
        result.ShouldBe("");
    }

    [Fact]
    public void Highlight_malformed_powershell_script_does_not_throw()
    {
        // Arrange
        var script = "Get-Process | Where-Object { $_.Name -eq 'test' } | {{{ invalid syntax";

        // Act & Assert
        Should.NotThrow(() => _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell));
    }

    [Fact]
    public void Highlight_complex_powershell_script_preserves_structure()
    {
        // Arrange
        var script = @"
# PowerShell script with multiple elements
$servers = @('server1', 'server2')
foreach ($server in $servers) {
    if (Test-Connection -ComputerName $server -Count 1 -Quiet) {
        Write-Host ""$server is online"" -ForegroundColor Green
    } else {
        Write-Warning ""$server is offline""
    }
}";

        // Act
        var result = _syntaxHighlightingService.HighlightScript(script, ScriptType.PowerShell);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain("[bold magenta]"); // Keywords
        result.ShouldContain("[bold yellow]"); // Variables
        result.ShouldContain("[green]"); // Strings
        result.ShouldContain("[bold cyan]"); // Commands
    }
}