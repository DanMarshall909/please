using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Infrastructure.Services;
using Spectre.Console;
using System.Diagnostics;
using System.IO;

namespace Please.Console.Services;

/// <summary>
/// Professional console UI service using Spectre.Console for beautiful output
/// </summary>
public class ConsoleUIService : IConsoleUIService
{
    private readonly SyntaxHighlightingService _syntaxHighlightingService = new();
    public void DisplayScript(string script, string title)
    {
        // Display header
        AnsiConsole.WriteLine();
        var rule = new Rule($"[bold yellow]{title}[/]");
        rule.Justification = Justify.Center;
        rule.Style = Style.Parse("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Display script line by line with line numbers (like Go implementation)
        var lines = script.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var lineNumber = (i + 1).ToString().PadLeft(3);
            AnsiConsole.MarkupLine($"[dim]{lineNumber}│[/] {lines[i].EscapeMarkup()}");
        }
        
        AnsiConsole.WriteLine();
    }

    public async Task DisplayProgressAsync(string message, Func<Task> action)
    {
        await AnsiConsole.Status()
            .StartAsync(message, async ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                ctx.SpinnerStyle(Style.Parse("green"));
                await action();
            });
    }

    public async Task<T> DisplayProgressAsync<T>(string message, Func<Task<T>> action)
    {
        return await AnsiConsole.Status()
            .StartAsync(message, async ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                ctx.SpinnerStyle(Style.Parse("green"));
                return await action();
            });
    }

    public int DisplayInteractiveMenu(string[] options)
    {
        // Check if we're in an interactive environment
        if (Environment.GetEnvironmentVariable("CI") == "true" || 
            Environment.GetEnvironmentVariable("TERM") == "dumb" ||
            !Environment.UserInteractive)
        {
            // Non-interactive environment - default to first safe option or cancel
            AnsiConsole.MarkupLine($"[yellow]Non-interactive environment detected. Defaulting to cancel for safety.[/]");
            return options.Length - 1; // Last option is typically "Cancel"
        }

        var prompt = new SelectionPrompt<string>()
            .Title("[green]Select an action:[/]")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .AddChoices(options);

        var selected = AnsiConsole.Prompt(prompt);
        return Array.IndexOf(options, selected);
    }

    public void DisplayRiskWarning(string riskLevel, string[] warnings)
    {
        var color = riskLevel.ToUpper() switch
        {
            "CRITICAL" => "bold red on white",
            "HIGH" => "red",
            "MEDIUM" => "yellow",
            "LOW" => "green",
            _ => "white"
        };

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Red)
            .AddColumn(new TableColumn($"[bold {color}]⚠️  {riskLevel} RISK WARNING[/]").Centered());

        foreach (var warning in warnings)
        {
            table.AddRow($"[{color}]{warning}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void DisplayBanner(string version, string description)
    {
        var figlet = new FigletText("Please v6")
            .Centered()
            .Color(Color.Blue);

        AnsiConsole.Write(figlet);

        var rule = new Rule($"[yellow]{description} - Version {version}[/]")
        {
            Style = Style.Parse("dim")
        };

        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public void DisplayScriptWithSyntaxHighlighting(string script, string title, ScriptType scriptType)
    {
        var language = scriptType switch
        {
            ScriptType.PowerShell => "powershell",
            ScriptType.Bash => "bash",
            _ => "text"
        };

        // Display header
        AnsiConsole.WriteLine();
        var rule = new Rule($"[bold yellow]{title} ({language.ToUpper()})[/]");
        rule.Justification = Justify.Center;
        rule.Style = Style.Parse(scriptType == ScriptType.PowerShell ? "blue" : "green");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Get highlighted script
        var highlightedScript = _syntaxHighlightingService.HighlightScript(script, scriptType);
        
        // Display script line by line with line numbers
        var lines = highlightedScript.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var lineNumber = (i + 1).ToString().PadLeft(3);
            // Note: highlightedScript already contains markup, so we don't escape it
            AnsiConsole.MarkupLine($"[dim]{lineNumber}│[/] {lines[i]}");
        }
        
        AnsiConsole.WriteLine();
    }

    public void DisplayScriptResponse(ScriptResponse response)
    {
        // Display script with syntax highlighting
        DisplayScriptWithSyntaxHighlighting(response.Script, "Generated Script", response.ScriptType);

        // Display metadata table
        var metadataTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("Property")
            .AddColumn("Value");

        metadataTable.AddRow("Provider", $"[blue]{response.Provider}[/]");
        metadataTable.AddRow("Model", $"[green]{response.Model}[/]");
        metadataTable.AddRow("Risk Level", GetRiskLevelMarkup(response.RiskLevel));
        metadataTable.AddRow("Generated", $"[dim]{response.GeneratedAt:yyyy-MM-dd HH:mm:ss}[/]");

        AnsiConsole.Write(metadataTable);
        AnsiConsole.WriteLine();

        // Display warnings if any
        if (response.Warnings.Count > 0)
        {
            DisplayRiskWarning(response.RiskLevel.ToString(), response.Warnings.Select(w => w.Message).ToArray());
        }

        // Display safety notes if any
        if (response.SafetyNotes.Count > 0)
        {
            DisplaySafetyNotes(response.SafetyNotes.Select(n => n.Message));
        }
    }

    public void DisplaySafetyNotes(IEnumerable<string> safetyNotes)
    {
        var notesList = safetyNotes.ToList();
        if (!notesList.Any()) return;

        var panel = new Panel(string.Join("\n", notesList.Select(note => $"• {note}")))
        {
            Header = new PanelHeader("[bold blue]Safety Notes[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public async Task DisplayEnhancedProgressAsync(string title, string[] steps, Func<string, int, Task> stepAction)
    {
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var progressTask = ctx.AddTask($"[green]{title}[/]", maxValue: steps.Length);
                
                for (int i = 0; i < steps.Length; i++)
                {
                    progressTask.Description = $"[green]{title}[/] - [blue]{steps[i]}[/]";
                    
                    await stepAction(steps[i], i);
                    
                    progressTask.Increment(1);
                    await Task.Delay(100); // Small delay for visual effect
                }
            });
    }

    public void DisplayScriptPreview(ScriptResponse response)
    {
        var previewTable = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(GetRiskLevelColor(response.RiskLevel))
            .AddColumn(new TableColumn("Script Preview").Centered());

        var scriptPreview = response.Script.Length > 200 
            ? response.Script.Substring(0, 200) + "..." 
            : response.Script;
        
        var highlightedPreview = _syntaxHighlightingService.HighlightScript(scriptPreview, response.ScriptType);

        previewTable.AddRow($"[bold]Task:[/] {response.TaskDescription.EscapeMarkup()}");
        previewTable.AddRow($"[bold]Provider:[/] {response.Provider} ({response.Model})");
        previewTable.AddRow($"[bold]Risk:[/] {GetRiskLevelMarkup(response.RiskLevel)}");
        previewTable.AddRow($"[bold]Script:[/]\n{highlightedPreview}");

        if (response.Warnings.Count > 0)
        {
            previewTable.AddRow($"[bold red]Warnings:[/] {response.Warnings.Count} issue(s) detected");
        }

        AnsiConsole.Write(previewTable);
        AnsiConsole.WriteLine();
    }

    private string GetRiskLevelMarkup(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Low => "[green]LOW[/]",
            RiskLevel.Medium => "[yellow]MEDIUM[/]",
            RiskLevel.High => "[red]HIGH[/]",
            RiskLevel.Critical => "[bold red on white]CRITICAL[/]",
            _ => "[dim]UNKNOWN[/]"
        };
    }

    private Color GetRiskLevelColor(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Low => Color.Green,
            RiskLevel.Medium => Color.Yellow,
            RiskLevel.High => Color.Red,
            RiskLevel.Critical => Color.Maroon,
            _ => Color.Grey
        };
    }

    public async Task<string?> EditScriptExternallyAsync(string script, ScriptType scriptType, string taskDescription)
    {
        try
        {
            // Create temp file with appropriate extension
            var fileExtension = GetFileExtension(scriptType);
            var sanitizedDescription = SanitizeFileName(taskDescription);
            var tempFileName = $"please_script_{sanitizedDescription}_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";
            var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

            // Write script to temp file
            await File.WriteAllTextAsync(tempFilePath, script);

            AnsiConsole.MarkupLine($"[blue]📝 Opening script in external editor...[/]");
            AnsiConsole.MarkupLine($"[dim]File: {tempFilePath}[/]");
            AnsiConsole.WriteLine();

            // Open in default editor and wait for it to close
            var editor = GetPreferredEditor();
            await OpenInEditorAsync(tempFilePath, editor);

            AnsiConsole.MarkupLine($"[green]✅ Editor closed. Reading modified script...[/]");

            // Read the modified content
            if (File.Exists(tempFilePath))
            {
                var modifiedScript = await File.ReadAllTextAsync(tempFilePath);
                
                // Clean up temp file
                try { File.Delete(tempFilePath); }
                catch { /* Ignore cleanup errors */ }

                return string.IsNullOrWhiteSpace(modifiedScript) ? null : modifiedScript;
            }

            return null;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error opening external editor: {ex.Message}[/]");
            return null;
        }
    }

    public bool ConfirmScriptExecution(ScriptResponse response)
    {
        AnsiConsole.WriteLine();
        var rule = new Rule($"[bold yellow]📋 Script Review & Execution Confirmation[/]")
        {
            Style = Style.Parse("yellow")
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Display the script one more time for review
        DisplayScriptWithSyntaxHighlighting(response.Script, "Final Script", response.ScriptType);

        // Show risk assessment
        var riskColor = GetRiskLevelMarkup(response.RiskLevel);
        AnsiConsole.MarkupLine($"[bold]Risk Level:[/] {riskColor}");
        
        if (response.Warnings.Any())
        {
            AnsiConsole.MarkupLine($"[bold red]⚠️  {response.Warnings.Count} warning(s) detected[/]");
        }

        AnsiConsole.WriteLine();

        // Execution confirmation prompt
        var choices = new[] { "✅ Execute Script", "❌ Cancel" };
        
        if (Environment.GetEnvironmentVariable("CI") == "true" || 
            Environment.GetEnvironmentVariable("TERM") == "dumb")
        {
            // Non-interactive environment - default to cancel for safety
            AnsiConsole.MarkupLine($"[yellow]Non-interactive environment detected. Defaulting to cancel for safety.[/]");
            return false;
        }

        var prompt = new SelectionPrompt<string>()
            .Title("[green]Do you want to execute this script?[/]")
            .PageSize(10)
            .AddChoices(choices);

        var selected = AnsiConsole.Prompt(prompt);
        return selected.StartsWith("✅");
    }

    private string GetFileExtension(ScriptType scriptType)
    {
        return scriptType switch
        {
            ScriptType.PowerShell => ".ps1",
            ScriptType.Bash => ".sh",
            ScriptType.Command => ".bat",
            ScriptType.Python => ".py",
            _ => ".txt"
        };
    }

    private string SanitizeFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "script";

        // Remove invalid filename characters and limit length
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(input.Where(c => !invalidChars.Contains(c)).ToArray());
        sanitized = sanitized.Replace(" ", "_").ToLowerInvariant();
        
        return sanitized.Length > 20 ? sanitized.Substring(0, 20) : sanitized;
    }

    private string GetPreferredEditor()
    {
        // Check environment variables for preferred editor
        var editor = Environment.GetEnvironmentVariable("PLEASE_EDITOR") ??
                    Environment.GetEnvironmentVariable("EDITOR") ??
                    Environment.GetEnvironmentVariable("VISUAL");

        if (!string.IsNullOrEmpty(editor))
            return editor;

        // Platform-specific defaults
        if (OperatingSystem.IsWindows())
        {
            // Try VS Code first, then Notepad++ and finally Notepad
            var candidates = new[] { "code", "notepad++", "notepad" };
            foreach (var candidate in candidates)
            {
                if (IsCommandAvailable(candidate))
                    return candidate;
            }
            return "notepad";
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // Try common editors
            var candidates = new[] { "code", "nano", "vim", "vi" };
            foreach (var candidate in candidates)
            {
                if (IsCommandAvailable(candidate))
                    return candidate;
            }
            return "vi";
        }

        return "notepad"; // Fallback
    }

    private bool IsCommandAvailable(string command)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            process?.WaitForExit(1000); // 1 second timeout
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task OpenInEditorAsync(string filePath, string editor)
    {
        try
        {
            ProcessStartInfo startInfo;
            
            if (editor == "code")
            {
                // VS Code - wait for window to close
                startInfo = new ProcessStartInfo
                {
                    FileName = "code",
                    Arguments = $"--wait \"{filePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
            else if (OperatingSystem.IsWindows() && (editor == "notepad" || editor == "notepad++"))
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = editor,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                };
            }
            else
            {
                // Unix-like systems or other editors
                startInfo = new ProcessStartInfo
                {
                    FileName = editor,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false
                };
            }

            AnsiConsole.MarkupLine($"[dim]Command: {startInfo.FileName} {startInfo.Arguments}[/]");
            AnsiConsole.MarkupLine($"[yellow]💡 Please save and close the editor when you're done editing.[/]");
            AnsiConsole.WriteLine();

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                // For VS Code with --wait flag, this will block until editor closes
                // For other editors, we'll wait a reasonable amount of time
                if (editor == "code")
                {
                    await process.WaitForExitAsync();
                }
                else
                {
                    // For editors that don't support --wait, we need to poll or wait for user input
                    AnsiConsole.MarkupLine($"[yellow]📝 Editor opened. Press any key when you've finished editing and saved the file...[/]");
                    System.Console.ReadKey(true);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open editor '{editor}': {ex.Message}", ex);
        }
    }
}
