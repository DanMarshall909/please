using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application.Services;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Services;

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
        // Handle special commands first
        if (await HandleSpecialCommandsAsync())
            return;

        // Handle first-run installation if needed
        var installationService = _serviceProvider.GetRequiredService<InstallationService>();
        await installationService.HandleFirstRunAsync();

        // Display professional banner
        _consoleUI.DisplayBanner("6.0.0", "AI-Powered PowerShell Script Generator");

        if (!_arguments.HasInput)
        {
            ShowHelp();
            return;
        }

        string taskDescription = _arguments.TaskDescription;

        // Get required services
        var scriptService = _serviceProvider.GetRequiredService<IScriptService>();
        var scriptExecutor = _serviceProvider.GetRequiredService<IScriptExecutor>();

        // Create a script request using the task description with automatic provider selection
        var request = ScriptRequest.Create(taskDescription);

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

                // Check if auto-execute is requested
                if (_arguments.IsAutoExecuteCommand)
                {
                    // Auto-execute the script without user interaction
                    _consoleUI.DisplayScript("Auto-executing script...", "Information");
                    await ExecuteScriptDirectly(result.Value!, scriptExecutor);
                    return;
                }

                // Interactive menu for user action
                var menuOptions = new[]
                {
                    "🚀 Execute script now",
                    "✏️ Edit in external editor",
                    "📋 Copy to clipboard",
                    "💾 Save to file",
                    "❌ Cancel"
                };

                var selectedAction = _consoleUI.DisplayInteractiveMenu(menuOptions);
                var finalScript = result.Value!;

                switch (selectedAction)
                {
                    case 0: // Execute script
                        await ExecuteScriptWithConfirmation(finalScript, scriptExecutor);
                        break;
                    case 1: // Edit in external editor
                        await EditAndExecuteScript(finalScript, scriptExecutor);
                        break;
                    case 2: // Copy to clipboard
                        _consoleUI.DisplayScript("Feature not yet implemented - Copy to clipboard", "Information");
                        break;
                    case 3: // Save to file
                        _consoleUI.DisplayScript("Feature not yet implemented - Save to file", "Information");
                        break;
                    case 4: // Cancel
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

    private async Task ExecuteScriptDirectly(ScriptResponse scriptResponse, IScriptExecutor scriptExecutor)
    {
        await _consoleUI.DisplayProgressAsync(
            "⚡ Executing script...",
            async () =>
            {
                var executionResult = await scriptExecutor.ExecuteScriptAsync(scriptResponse.Script);

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
    }

    private async Task ExecuteScriptWithConfirmation(ScriptResponse scriptResponse, IScriptExecutor scriptExecutor)
    {
        if (_consoleUI.ConfirmScriptExecution(scriptResponse))
        {
            await _consoleUI.DisplayProgressAsync(
                "⚡ Executing script...",
                async () =>
                {
                    var executionResult = await scriptExecutor.ExecuteScriptAsync(scriptResponse.Script);

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
        }
        else
        {
            _consoleUI.DisplayScript("Execution cancelled by user", "Information");
        }
    }

    private async Task EditAndExecuteScript(ScriptResponse originalScript, IScriptExecutor scriptExecutor)
    {
        // Open external editor
        var modifiedScript = await _consoleUI.EditScriptExternallyAsync(
            originalScript.Script, 
            originalScript.ScriptType, 
            originalScript.TaskDescription
        );

        if (modifiedScript != null && modifiedScript != originalScript.Script)
        {
            // Re-validate the modified script
            var validationService = _serviceProvider.GetRequiredService<IScriptValidationService>();
            
            // Create new script response with modified content
            var modifiedResponse = ScriptResponse.Create(
                modifiedScript,
                originalScript.TaskDescription + " (modified)",
                originalScript.Provider,
                originalScript.Model,
                originalScript.ScriptType,
                RiskLevel.Low // Will be updated by validation
            );

            // Re-validate with the modified script
            var validatedResponse = await _consoleUI.DisplayProgressAsync(
                "🔍 Re-validating modified script...",
                async () =>
                {
                    await Task.Delay(100); // Small delay for UI feedback
                    return validationService.EnhanceWithValidation(modifiedResponse);
                }
            );

            // Display the validated modified script
            _consoleUI.DisplayScriptResponse(validatedResponse);

            // Ask for execution confirmation
            await ExecuteScriptWithConfirmation(validatedResponse, scriptExecutor);
        }
        else if (modifiedScript == null)
        {
            _consoleUI.DisplayScript("Edit cancelled or no changes made", "Information");
        }
        else
        {
            _consoleUI.DisplayScript("No changes detected in script", "Information");
            // Still offer to execute the original
            await ExecuteScriptWithConfirmation(originalScript, scriptExecutor);
        }
    }

    private async Task<bool> HandleSpecialCommandsAsync()
    {
        var installationService = _serviceProvider.GetRequiredService<InstallationService>();

        if (_arguments.IsHelpCommand)
        {
            ShowHelp();
            return true;
        }

        if (_arguments.IsVersionCommand)
        {
            ShowVersion();
            return true;
        }

        if (_arguments.IsStatusCommand)
        {
            installationService.ShowStatus();
            return true;
        }

        if (_arguments.IsInstallCommand)
        {
            await installationService.ForceInstallAsync();
            return true;
        }

        return false;
    }

    private void ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("🤖 Please v6 - AI-Powered Script Generator");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("  please <natural language description>");
        Console.WriteLine("  please [COMMAND]");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  please get current time");
        Console.WriteLine("  please list running services");
        Console.WriteLine("  please create backup script for my documents");
        Console.WriteLine("  please find files older than 7 days");
        Console.WriteLine("  please say hello --auto-execute");
        Console.WriteLine();
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  --install, -i       Install Please to your system");
        Console.WriteLine("  --status,  -s       Show installation status");
        Console.WriteLine("  --version, -v       Show version information");
        Console.WriteLine("  --help,    -h       Show this help message");
        Console.WriteLine("  --auto-execute, -x  Auto-execute generated script without confirmation");
        Console.WriteLine();
        Console.WriteLine("Please uses natural language to generate and execute scripts.");
        Console.WriteLine("Supported providers: OpenAI, Anthropic, Gemini, OpenRouter, Ollama");
        Console.WriteLine();
    }

    private void ShowVersion()
    {
        var platformService = _serviceProvider.GetRequiredService<IPlatformService>();
        
        Console.WriteLine();
        Console.WriteLine("Please v6.0.0");
        Console.WriteLine($"Platform: {platformService.GetPlatformName()}");
        Console.WriteLine($"Runtime: .NET 8.0");
        Console.WriteLine($"Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine();
        
        if (!platformService.IsInstalled())
        {
            Console.WriteLine("📁 Running as portable application");
            Console.WriteLine("💡 Run 'please --install' to install to your system");
        }
        else
        {
            Console.WriteLine("✅ Installed to system");
            Console.WriteLine($"Location: {platformService.GetInstallationDirectory()}");
        }
        Console.WriteLine();
    }
}
