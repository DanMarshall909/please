using System.Management.Automation.Language;
using System.Text;
using Spectre.Console;
using Please.Domain.Enums;

namespace Please.Infrastructure.Services;

/// <summary>
/// Service for providing syntax highlighting for different script types
/// </summary>
public class SyntaxHighlightingService
{
    public string HighlightScript(string script, ScriptType scriptType)
    {
        return scriptType switch
        {
            ScriptType.PowerShell => HighlightPowerShell(script),
            ScriptType.Bash => HighlightBash(script),
            _ => script.EscapeMarkup()
        };
    }

    private string HighlightPowerShell(string script)
    {
        try
        {
            // Parse the PowerShell script using AST
            var ast = Parser.ParseInput(script, out Token[] tokens, out ParseError[] errors);
            
            if (tokens == null || tokens.Length == 0)
            {
                // Fallback to regex-based highlighting if AST parsing fails
                return ApplyRegexHighlighting(script);
            }

            var result = new StringBuilder();
            var lastEnd = 0;

            foreach (var token in tokens.OrderBy(t => t.Extent.StartOffset))
            {
                // Add any text between tokens
                if (token.Extent.StartOffset > lastEnd)
                {
                    var between = script.Substring(lastEnd, token.Extent.StartOffset - lastEnd);
                    result.Append(between.EscapeMarkup());
                }

                // Get the token text
                var tokenText = script.Substring(token.Extent.StartOffset, 
                    token.Extent.EndOffset - token.Extent.StartOffset);

                // Apply syntax highlighting based on token type
                var highlightedText = GetHighlightedToken(tokenText, token);
                result.Append(highlightedText);

                lastEnd = token.Extent.EndOffset;
            }

            // Add any remaining text
            if (lastEnd < script.Length)
            {
                result.Append(script.Substring(lastEnd).EscapeMarkup());
            }

            return result.ToString();
        }
        catch
        {
            // If parsing fails, fallback to regex highlighting
            return ApplyRegexHighlighting(script);
        }
    }

    private string ApplyRegexHighlighting(string script)
    {
        var result = script;

        // Escape markup first to avoid conflicts
        result = result.EscapeMarkup();

        // PowerShell cmdlets (Get-, Set-, New-, etc.)
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"\b([A-Z][a-z]+-[A-Za-z]+)\b", "[bold cyan]$1[/]");

        // Variables ($var)
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"(\$[A-Za-z_][A-Za-z0-9_]*)", "[bold yellow]$1[/]");

        // Keywords
        var keywords = @"\b(if|else|elseif|switch|function|filter|workflow|class|for|foreach|while|do|try|catch|finally|throw|return|break|continue|exit)\b";
        result = System.Text.RegularExpressions.Regex.Replace(
            result, keywords, "[bold magenta]$1[/]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Strings (single and double quotes)
        result = System.Text.RegularExpressions.Regex.Replace(
            result, "'([^']*?)'", "[green]'$1'[/]");
        result = System.Text.RegularExpressions.Regex.Replace(
            result, "\"([^\"]*?)\"", "[green]\"$1\"[/]");

        // Comments
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"(#.*)", "[dim green]$1[/]");

        // Numbers
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"\b(\d+)\b", "[blue]$1[/]");

        // Parameters (-Parameter)
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"(-[A-Za-z]+)", "[cyan]$1[/]");

        return result;
    }

    private string GetHighlightedToken(string tokenText, Token token)
    {
        var escapedText = tokenText.EscapeMarkup();

        // Check if this is a command by looking at token type and content
        if (IsCommandToken(token, tokenText))
        {
            return $"[bold cyan]{escapedText}[/]";
        }

        return token.Kind switch
        {
            // Keywords
            TokenKind.If or TokenKind.Else or TokenKind.ElseIf or TokenKind.Switch or 
            TokenKind.Function or TokenKind.Filter or TokenKind.Workflow or TokenKind.Class or
            TokenKind.For or TokenKind.While or TokenKind.Do or
            TokenKind.Try or TokenKind.Catch or TokenKind.Finally or TokenKind.Throw or
            TokenKind.Return or TokenKind.Break or TokenKind.Continue or TokenKind.Exit => 
                $"[bold magenta]{escapedText}[/]",

            // Basic operators
            TokenKind.Plus or TokenKind.Minus or TokenKind.Multiply or TokenKind.Divide or
            TokenKind.Equals or TokenKind.And or TokenKind.Or or TokenKind.Not or 
            TokenKind.Xor or TokenKind.Band or TokenKind.Bor or TokenKind.Bnot or TokenKind.Bxor => 
                $"[yellow]{escapedText}[/]",

            // Strings
            TokenKind.StringLiteral or TokenKind.StringExpandable => $"[green]{escapedText}[/]",

            // Numbers
            TokenKind.Number => $"[blue]{escapedText}[/]",

            // Variables
            TokenKind.Variable => $"[bold yellow]{escapedText}[/]",

            // Comments
            TokenKind.Comment => $"[dim green]{escapedText}[/]",

            // Parameters
            TokenKind.Parameter => $"[cyan]{escapedText}[/]",

            // Types
            TokenKind.LBracket when IsTypeToken(tokenText) => $"[bold blue]{escapedText}[/]",

            // Default - no highlighting
            _ => escapedText
        };
    }

    private bool IsCommandToken(Token token, string tokenText)
    {
        // Check if this looks like a PowerShell command/cmdlet
        if (token.Kind == TokenKind.Generic && 
            (tokenText.Contains('-') || char.IsUpper(tokenText.FirstOrDefault())))
        {
            return true;
        }
        
        // Check for common PowerShell cmdlet patterns
        var commonCmdletPatterns = new[]
        {
            "Get-", "Set-", "New-", "Remove-", "Add-", "Clear-", "Copy-", "Move-",
            "Start-", "Stop-", "Restart-", "Test-", "Invoke-", "Import-", "Export-",
            "Write-", "Read-", "Select-", "Where-", "ForEach-", "Sort-", "Group-"
        };

        return commonCmdletPatterns.Any(pattern => tokenText.StartsWith(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsTypeToken(string tokenText)
    {
        // Common PowerShell types
        var commonTypes = new[]
        {
            "[string]", "[int]", "[bool]", "[array]", "[hashtable]", "[pscustomobject]",
            "[datetime]", "[timespan]", "[guid]", "[regex]", "[xml]", "[object]",
            "[scriptblock]", "[psobject]", "[system.", "[microsoft."
        };

        return commonTypes.Any(type => tokenText.StartsWith(type, StringComparison.OrdinalIgnoreCase));
    }

    private string HighlightBash(string script)
    {
        // Simple regex-based highlighting for Bash since we don't have a parser
        var lines = script.Split('\n');
        var result = new StringBuilder();

        foreach (var line in lines)
        {
            var highlightedLine = line;

            // Comments
            highlightedLine = System.Text.RegularExpressions.Regex.Replace(
                highlightedLine, @"(#.*)", "[dim green]$1[/]");

            // Variables
            highlightedLine = System.Text.RegularExpressions.Regex.Replace(
                highlightedLine, @"(\$\w+)", "[bold yellow]$1[/]");

            // Strings (simple)
            highlightedLine = System.Text.RegularExpressions.Regex.Replace(
                highlightedLine, "\"([^\"]*?)\"", "[green]\"$1\"[/]");
            highlightedLine = System.Text.RegularExpressions.Regex.Replace(
                highlightedLine, "'([^']*?)'", "[green]'$1'[/]");

            // Common commands
            highlightedLine = System.Text.RegularExpressions.Regex.Replace(
                highlightedLine, @"\b(echo|ls|cd|pwd|cat|grep|find|awk|sed|sort|uniq|head|tail|wc|chmod|chown|mkdir|rmdir|rm|cp|mv|ln|wget|curl|tar|gzip|gunzip)\b",
                "[bold cyan]$1[/]");

            // Keywords
            highlightedLine = System.Text.RegularExpressions.Regex.Replace(
                highlightedLine, @"\b(if|then|else|elif|fi|for|while|do|done|case|esac|function|return|exit|break|continue)\b",
                "[bold magenta]$1[/]");

            result.AppendLine(highlightedLine.EscapeMarkup());
        }

        return result.ToString().TrimEnd();
    }
}