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
}
