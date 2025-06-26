using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application.Services;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

public class TaskProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskProcessor> _logger;
    private readonly CommandLineArguments _arguments;
    private readonly IConsoleUIService _consoleUI;

    public TaskProcessor(IServiceProvider serviceProvider, ILogger<TaskProcessor> logger,
        CommandLineArguments arguments, IConsoleUIService consoleUI)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _arguments = arguments;
        _consoleUI = consoleUI;
    }

    public async Task ProcessTaskAsync()
    {
        // Display professional banner
        _consoleUI.DisplayBanner("6.0.0", "AI-Powered PowerShell Script Generator");

        if (!_arguments.HasInput)
        {
            _consoleUI.DisplayRiskWarning("HIGH", new[] { "No task description provided", "Please pass a task description as a program argument" });
            return;
        }

        string taskDescription = _arguments.TaskDescription;

        // Get required services
        var scriptService = _serviceProvider.GetRequiredService<IScriptService>();
        var scriptExecutor = _serviceProvider.GetRequiredService<IScriptExecutor>();

        // Create a script request using the task description
        var request = ScriptRequest.Create(
            taskDescription,
            ProviderType.OpenAi,
            "gpt-3.5-turbo"
        );

        try
        {
            // Generate script with professional progress indicator
            var result = await _consoleUI.DisplayProgressAsync(
                $"🤖 Generating PowerShell script for: {taskDescription}",
                async () => await scriptService.GenerateScriptAsync(request)
            );

            if (result.IsSuccess)
            {
                // Display the generated script beautifully
                _consoleUI.DisplayScript(result.Value!.Script, $"Generated Script - {result.Value!.Provider} ({result.Value!.Model})");

                // Show risk warnings if applicable
                if (result.Value!.RiskLevel != RiskLevel.Low)
                {
                    var riskWarnings = new List<string> { $"This script has {result.Value!.RiskLevel} risk level" };
                    if (result.Value!.RiskLevel == RiskLevel.High || result.Value!.RiskLevel == RiskLevel.Critical)
                    {
                        riskWarnings.Add("May modify system files or settings");
                        riskWarnings.Add("Review carefully before execution");
                    }
                    _consoleUI.DisplayRiskWarning(result.Value!.RiskLevel.ToString().ToUpper(), riskWarnings.ToArray());
                }

                // Interactive menu for user action
                var menuOptions = new[]
                {
                    "🚀 Execute script now",
                    "📋 Copy to clipboard",
                    "💾 Save to file",
                    "❌ Cancel"
                };

                var selectedAction = _consoleUI.DisplayInteractiveMenu(menuOptions);

                switch (selectedAction)
                {
                    case 0: // Execute script
                        await _consoleUI.DisplayProgressAsync(
                            "⚡ Executing PowerShell script...",
                            async () =>
                            {
                                var executionResult = await scriptExecutor.ExecuteScriptAsync(result.Value!.Script);

                                if (executionResult.IsSuccess)
                                {
                                    if (!string.IsNullOrWhiteSpace(executionResult.Value))
                                    {
                                        _consoleUI.DisplayScript(executionResult.Value!, "Script Output");
                                    }
                                    else
                                    {
                                        _consoleUI.DisplayScript("Script completed successfully with no output.", "Execution Result");
                                    }
                                }
                                else
                                {
                                    _consoleUI.DisplayRiskWarning("HIGH", new[] { "Script execution failed", $"Error: {executionResult.Error}" });
                                }
                            }
                        );
                        break;
                    case 1: // Copy to clipboard
                        _consoleUI.DisplayScript("Feature not yet implemented - Copy to clipboard", "Information");
                        break;
                    case 2: // Save to file
                        _consoleUI.DisplayScript("Feature not yet implemented - Save to file", "Information");
                        break;
                    case 3: // Cancel
                        _consoleUI.DisplayScript("Operation cancelled by user", "Information");
                        break;
                }
            }
            else
            {
                _consoleUI.DisplayRiskWarning("HIGH", new[] { "Script generation failed", $"Error: {result.Error}" });
            }
        }
        catch (Exception ex)
        {
            _consoleUI.DisplayRiskWarning("HIGH", new[] { "Unexpected error occurred", $"Error: {ex.Message}" });
            _logger.LogError(ex, "Script generation failed with exception");
        }
    }
}
