using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Spectre.Console;

namespace Please.Console.Services;

/// <summary>
/// Professional console UI service using Spectre.Console for beautiful output
/// </summary>
public class ConsoleUIService : IConsoleUIService
{
    public void DisplayScript(string script, string title)
    {
        var panel = new Panel(new Markup($"[cyan]{script.EscapeMarkup()}[/]"))
        {
            Header = new PanelHeader($"[bold yellow]{title}[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue)
        };

        AnsiConsole.Write(panel);
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

        var panel = new Panel(new Markup($"[cyan]{script.EscapeMarkup()}[/]"))
        {
            Header = new PanelHeader($"[bold yellow]{title} ({language.ToUpper()})[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(scriptType == ScriptType.PowerShell ? Color.Blue : Color.Green)
        };

        AnsiConsole.Write(panel);
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

        previewTable.AddRow($"[bold]Task:[/] {response.TaskDescription}");
        previewTable.AddRow($"[bold]Provider:[/] {response.Provider} ({response.Model})");
        previewTable.AddRow($"[bold]Risk:[/] {GetRiskLevelMarkup(response.RiskLevel)}");
        previewTable.AddRow($"[bold]Script:[/]\n[dim]{scriptPreview.EscapeMarkup()}[/]");

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
}
