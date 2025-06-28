using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application.Services;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using SearchOptions = Please.Domain.Interfaces.SearchOptions;

public class TaskProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskProcessor> _logger;
    private readonly CommandLineArguments _arguments;
    private readonly IConsoleUIService _consoleUI;
    private readonly IClipboardService _clipboardService;
    private readonly IFileService _fileService;

    public TaskProcessor(IServiceProvider serviceProvider, ILogger<TaskProcessor> logger,
        CommandLineArguments arguments, IConsoleUIService consoleUI, IClipboardService clipboardService, IFileService fileService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _arguments = arguments;
        _consoleUI = consoleUI;
        _clipboardService = clipboardService;
        _fileService = fileService;
    }

    public async Task ProcessTaskAsync(CancellationToken cancellationToken = default)
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
                async () => await scriptService.GenerateScriptAsync(request, cancellationToken)
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
                        await CopyScriptToClipboard(finalScript);
                        break;
                    case 3: // Save to file
                        await SaveScriptToFile(finalScript);
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

    private async Task CopyScriptToClipboard(ScriptResponse scriptResponse)
    {
        if (!_clipboardService.IsSupported())
        {
            _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
            { 
                "Clipboard operations are not supported on this platform",
                "Consider installing clipboard utilities (xclip, xsel, or wl-clipboard on Linux)"
            });
            return;
        }

        try
        {
            var success = await _consoleUI.DisplayProgressAsync(
                "📋 Copying script to clipboard...",
                async () => await _clipboardService.SetTextAsync(scriptResponse.Script)
            );

            if (success)
            {
                _consoleUI.DisplayScript("✅ Script successfully copied to clipboard!", "Success");
                
                // Show helpful information about the copied content
                var lines = scriptResponse.Script.Split('\n').Length;
                var chars = scriptResponse.Script.Length;
                _consoleUI.DisplayScript(
                    $"📊 Copied {lines} lines ({chars} characters) from {scriptResponse.Provider} ({scriptResponse.Model})", 
                    "Clipboard Info"
                );
            }
            else
            {
                _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
                { 
                    "Failed to copy script to clipboard",
                    "You can manually select and copy the script text above"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while copying to clipboard");
            _consoleUI.DisplayRiskWarning("HIGH", new[] 
            { 
                "Unexpected error occurred while copying to clipboard",
                $"Error: {ex.Message}"
            });
        }
    }

    private async Task SaveScriptToFile(ScriptResponse scriptResponse)
    {
        try
        {
            var result = await _consoleUI.DisplayProgressAsync(
                "💾 Saving script to file...",
                async () => await _fileService.SaveScriptToFileAsync(scriptResponse)
            );

            if (result.IsSuccess)
            {
                _consoleUI.DisplayScript("✅ Script successfully saved to file!", "Success");
                
                // Show helpful information about the saved file
                var lines = scriptResponse.Script.Split('\n').Length;
                var chars = scriptResponse.Script.Length;
                var fileExtension = _fileService.GetFileExtension(scriptResponse.ScriptType);
                var fileName = Path.GetFileName(result.Value!);
                
                _consoleUI.DisplayScript(
                    $"📁 Saved as: {fileName}\n" +
                    $"📂 Location: {result.Value!}\n" +
                    $"📊 Content: {lines} lines ({chars} characters)\n" +
                    $"🔧 Provider: {scriptResponse.Provider} ({scriptResponse.Model})\n" +
                    $"🏷️ Type: {scriptResponse.ScriptType} ({fileExtension})", 
                    "File Info"
                );

                // Show safety reminder for executable files
                if (scriptResponse.ScriptType == ScriptType.PowerShell || 
                    scriptResponse.ScriptType == ScriptType.Bash || 
                    scriptResponse.ScriptType == ScriptType.Command)
                {
                    _consoleUI.DisplayRiskWarning("LOW", new[] 
                    { 
                        "Remember to review the script before executing",
                        $"File contains executable {scriptResponse.ScriptType} code",
                        "Always verify AI-generated scripts before running"
                    });
                }
            }
            else
            {
                _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
                { 
                    "Failed to save script to file",
                    $"Error: {result.Error}",
                    "You can manually copy and save the script text"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving script to file");
            _consoleUI.DisplayRiskWarning("HIGH", new[] 
            { 
                "Unexpected error occurred while saving script",
                $"Error: {ex.Message}",
                "Please try again or save the script manually"
            });
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

        if (_arguments.IsHistoryCommand)
        {
            await ShowHistoryAsync();
            return true;
        }

        if (_arguments.IsSearchCommand)
        {
            await ShowSearchAsync();
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
        Console.WriteLine();
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  --install, -i    Install Please to your system");
        Console.WriteLine("  --status,  -s    Show installation status");
        Console.WriteLine("  --version, -v    Show version information");
        Console.WriteLine("  --history, -r    Browse previously generated scripts");
        Console.WriteLine("  --search,  -f    Search scripts by query");
        Console.WriteLine("  --help,    -h    Show this help message");
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

    private async Task ShowHistoryAsync()
    {
        try
        {
            var scriptRepository = _serviceProvider.GetRequiredService<IScriptRepository>();
            
            // Check if there's any history
            var hasHistoryResult = await scriptRepository.HasHistoryAsync();
            if (!hasHistoryResult.IsSuccess)
            {
                _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
                { 
                    "Failed to check script history",
                    $"Error: {hasHistoryResult.Error}"
                });
                return;
            }

            if (!hasHistoryResult.Value)
            {
                _consoleUI.DisplayBanner("6.0.0", "Script History");
                _consoleUI.DisplayScript("📝 No scripts found in history.", "Information");
                _consoleUI.DisplayScript("💡 Generate some scripts first, then use 'please --history' to view them.", "Tip");
                return;
            }

            // Get the script history
            var historyResult = await _consoleUI.DisplayProgressAsync(
                "📜 Loading script history...",
                async () => await scriptRepository.GetScriptHistoryAsync(20) // Get last 20 scripts
            );

            if (!historyResult.IsSuccess)
            {
                _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
                { 
                    "Failed to load script history",
                    $"Error: {historyResult.Error}"
                });
                return;
            }

            var scripts = historyResult.Value!.ToList();
            if (!scripts.Any())
            {
                _consoleUI.DisplayScript("📝 No scripts found in history.", "Information");
                return;
            }

            // Display the history browser
            await DisplayHistoryBrowser(scripts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while showing script history");
            _consoleUI.DisplayRiskWarning("HIGH", new[] 
            { 
                "Unexpected error occurred while loading history",
                $"Error: {ex.Message}"
            });
        }
    }

    private async Task DisplayHistoryBrowser(List<ScriptResponse> scripts)
    {
        _consoleUI.DisplayBanner("6.0.0", $"Script History ({scripts.Count} scripts)");

        while (true)
        {
            // Display script list
            var options = new List<string>();
            
            for (int i = 0; i < Math.Min(scripts.Count, 10); i++) // Show first 10
            {
                var script = scripts[i];
                var preview = script.Script.Length > 60 
                    ? script.Script.Substring(0, 60) + "..." 
                    : script.Script;
                
                // Replace newlines with spaces for display
                preview = preview.Replace('\n', ' ').Replace('\r', ' ');
                
                var timeAgo = GetTimeAgo(script.GeneratedAt);
                options.Add($"📜 {script.TaskDescription} ({script.Provider}) - {timeAgo}");
            }

            if (scripts.Count > 10)
            {
                options.Add($"📄 Show more scripts ({scripts.Count - 10} more)");
            }

            options.Add("🔍 Search scripts");
            options.Add("🗑️ Clear history");
            options.Add("❌ Exit history browser");

            _consoleUI.DisplayScript($"Found {scripts.Count} scripts in history:", "History");
            var selection = _consoleUI.DisplayInteractiveMenu(options.ToArray());

            if (selection < Math.Min(scripts.Count, 10))
            {
                // User selected a script
                await DisplayScriptDetails(scripts[selection]);
            }
            else if (selection == Math.Min(scripts.Count, 10) && scripts.Count > 10)
            {
                // Show more scripts
                await ShowAllScripts(scripts);
            }
            else if (selection == options.Count - 3) // Search
            {
                await ShowSearchAsync();
                return;
            }
            else if (selection == options.Count - 2) // Clear history
            {
                await ClearHistory();
                return;
            }
            else // Exit
            {
                break;
            }
        }
    }


    private Task ShowAllScripts(List<ScriptResponse> scripts)
    {
        _consoleUI.DisplayScript($"All {scripts.Count} scripts:", "Complete History");
        
        foreach (var script in scripts)
        {
            var timeAgo = GetTimeAgo(script.GeneratedAt);
            var riskBadge = script.RiskLevel != RiskLevel.Low ? $" [{script.RiskLevel}]" : "";
            _consoleUI.DisplayScript(
                $"📜 {script.TaskDescription} ({script.Provider}/{script.Model}){riskBadge} - {timeAgo}",
                "Script Entry"
            );
        }
        
        _consoleUI.DisplayScript("Use 'please --history' again to interact with scripts.", "Tip");
        return Task.CompletedTask;
    }

    private async Task ShowSearchAsync()
    {
        try
        {
            var scriptSearchService = _serviceProvider.GetRequiredService<IScriptSearchService>();
            
            // Check if there's any history to search
            var scriptRepository = _serviceProvider.GetRequiredService<IScriptRepository>();
            var hasHistoryResult = await scriptRepository.HasHistoryAsync();
            if (!hasHistoryResult.IsSuccess)
            {
                _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
                { 
                    "Failed to check script history",
                    $"Error: {hasHistoryResult.Error}"
                });
                return;
            }

            if (!hasHistoryResult.Value)
            {
                _consoleUI.DisplayBanner("6.0.0", "Script Search");
                _consoleUI.DisplayScript("📝 No scripts found in history to search.", "Information");
                _consoleUI.DisplayScript("💡 Generate some scripts first, then use search to find them.", "Tip");
                return;
            }

            _consoleUI.DisplayBanner("6.0.0", "Script Search");

            // Handle search query from command line
            if (!string.IsNullOrWhiteSpace(_arguments.SearchQuery))
            {
                await PerformSearch(scriptSearchService, _arguments.SearchQuery);
                return;
            }

            // Interactive search mode
            await InteractiveSearchMode(scriptSearchService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ShowSearchAsync");
            _consoleUI.DisplayRiskWarning("HIGH", new[] 
            { 
                "Unexpected error occurred during search",
                $"Error: {ex.Message}"
            });
        }
    }

    private async Task InteractiveSearchMode(IScriptSearchService searchService)
    {
        bool continueSearching = true;

        while (continueSearching)
        {
            Console.WriteLine();
            _consoleUI.DisplayScript("🔍 Search Options", "Menu");
            
            var searchOptions = new[]
            {
                "📝 Search by text query",
                "📅 Show recent scripts (last 7 days)",
                "🔒 Show safe scripts only",
                "⚙️ Advanced search with filters",
                "📊 Browse all scripts",
                "❌ Exit search"
            };

            var choice = _consoleUI.DisplayInteractiveMenu(searchOptions);

            switch (choice)
            {
                case 0: // Text search
                    await TextSearch(searchService);
                    break;
                case 1: // Recent scripts
                    await ShowRecentScripts(searchService);
                    break;
                case 2: // Safe scripts only
                    await ShowSafeScripts(searchService);
                    break;
                case 3: // Advanced search
                    await AdvancedSearch(searchService);
                    break;
                case 4: // Browse all
                    await BrowseAllScripts(searchService);
                    break;
                case 5: // Exit
                    continueSearching = false;
                    break;
            }
        }
    }

    private async Task TextSearch(IScriptSearchService searchService)
    {
        Console.WriteLine();
        Console.Write("🔍 Enter search query: ");
        var query = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(query))
        {
            _consoleUI.DisplayScript("❌ Search cancelled - no query provided.", "Information");
            return;
        }

        await PerformSearch(searchService, query);
    }

    private async Task PerformSearch(IScriptSearchService searchService, string query)
    {
        var result = await _consoleUI.DisplayProgressAsync(
            $"🔍 Searching for: '{query}'...",
            async () => await searchService.SearchAsync(query, SearchOptions.Default)
        );

        if (!result.IsSuccess)
        {
            _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
            { 
                "Search failed",
                $"Error: {result.Error}"
            });
            return;
        }

        var scripts = result.Value!.ToList();
        
        if (!scripts.Any())
        {
            _consoleUI.DisplayScript($"❌ No scripts found matching: '{query}'", "Search Results");
            _consoleUI.DisplayScript("💡 Try a different search term or browse all scripts.", "Tip");
            return;
        }

        await DisplaySearchResults(scripts, $"Search Results for: '{query}'");
    }

    private async Task ShowRecentScripts(IScriptSearchService searchService)
    {
        var result = await _consoleUI.DisplayProgressAsync(
            "📅 Loading recent scripts...",
            async () => await searchService.GetFilteredScriptsAsync(SearchOptions.Recent)
        );

        if (!result.IsSuccess)
        {
            _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
            { 
                "Failed to load recent scripts",
                $"Error: {result.Error}"
            });
            return;
        }

        var scripts = result.Value!.ToList();
        await DisplaySearchResults(scripts, "Recent Scripts (Last 7 Days)");
    }

    private async Task ShowSafeScripts(IScriptSearchService searchService)
    {
        var result = await _consoleUI.DisplayProgressAsync(
            "🔒 Loading safe scripts...",
            async () => await searchService.GetFilteredScriptsAsync(SearchOptions.SafeOnly)
        );

        if (!result.IsSuccess)
        {
            _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
            { 
                "Failed to load safe scripts",
                $"Error: {result.Error}"
            });
            return;
        }

        var scripts = result.Value!.ToList();
        await DisplaySearchResults(scripts, "Safe Scripts (Low-Medium Risk Only)");
    }

    private async Task AdvancedSearch(IScriptSearchService searchService)
    {
        Console.WriteLine();
        _consoleUI.DisplayScript("⚙️ Advanced Search", "Options");
        
        var searchOptions = new SearchOptions();

        // Get search query
        Console.Write("🔍 Search query (optional): ");
        var query = Console.ReadLine()?.Trim();

        // Provider filter
        Console.WriteLine();
        Console.WriteLine("Select AI Provider (optional):");
        var providerOptions = new[] { "Any Provider", "OpenAI", "Anthropic", "Gemini", "OpenRouter", "Ollama" };
        var providerChoice = _consoleUI.DisplayInteractiveMenu(providerOptions);
        if (providerChoice > 0)
        {
            searchOptions.Provider = (ProviderType)(providerChoice - 1);
        }

        // Risk level filter
        Console.WriteLine();
        Console.WriteLine("Maximum Risk Level:");
        var riskOptions = new[] { "Any Risk Level", "Low Only", "Medium or Lower", "High or Lower" };
        var riskChoice = _consoleUI.DisplayInteractiveMenu(riskOptions);
        if (riskChoice > 0)
        {
            searchOptions.MaxRiskLevel = (RiskLevel)(riskChoice - 1);
        }

        // Execute search
        var result = await _consoleUI.DisplayProgressAsync(
            "🔍 Performing advanced search...",
            async () => string.IsNullOrWhiteSpace(query) 
                ? await searchService.GetFilteredScriptsAsync(searchOptions)
                : await searchService.SearchAsync(query, searchOptions)
        );

        if (!result.IsSuccess)
        {
            _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
            { 
                "Advanced search failed",
                $"Error: {result.Error}"
            });
            return;
        }

        var scripts = result.Value!.ToList();
        var title = string.IsNullOrWhiteSpace(query) ? "Advanced Filter Results" : $"Advanced Search Results for: '{query}'";
        await DisplaySearchResults(scripts, title);
    }

    private async Task BrowseAllScripts(IScriptSearchService searchService)
    {
        var result = await _consoleUI.DisplayProgressAsync(
            "📊 Loading all scripts...",
            async () => await searchService.GetFilteredScriptsAsync(SearchOptions.Default)
        );

        if (!result.IsSuccess)
        {
            _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
            { 
                "Failed to load scripts",
                $"Error: {result.Error}"
            });
            return;
        }

        var scripts = result.Value!.ToList();
        await DisplaySearchResults(scripts, "All Scripts");
    }

    private async Task DisplaySearchResults(List<ScriptResponse> scripts, string title)
    {
        if (!scripts.Any())
        {
            _consoleUI.DisplayScript($"❌ No scripts found", title);
            return;
        }

        _consoleUI.DisplayScript($"📊 Found {scripts.Count} script(s)", title);
        Console.WriteLine();

        // Display scripts in a table format
        for (int i = 0; i < scripts.Count; i++)
        {
            var script = scripts[i];
            var timeAgo = GetTimeAgo(script.CreatedAt);
            var riskIcon = GetRiskIcon(script.RiskLevel);
            var truncatedTask = script.TaskDescription.Length > 60 
                ? script.TaskDescription.Substring(0, 57) + "..." 
                : script.TaskDescription;

            Console.WriteLine($"  {i + 1,2}. {riskIcon} {truncatedTask}");
            Console.WriteLine($"      🤖 {script.Provider} | ⏰ {timeAgo} | 🎯 {script.ScriptType}");
            Console.WriteLine();
        }

        // Allow user to select a script to view/execute
        Console.WriteLine("Select a script to view details (1-{0}) or press Enter to return:", scripts.Count);
        Console.Write("Choice: ");
        var input = Console.ReadLine();

        if (int.TryParse(input, out int selection) && selection >= 1 && selection <= scripts.Count)
        {
            var selectedScript = scripts[selection - 1];
            await DisplayScriptDetails(selectedScript);
        }
    }

    private async Task DisplayScriptDetails(ScriptResponse script)
    {
        Console.WriteLine();
        _consoleUI.DisplayScript($"📋 {script.TaskDescription}", $"Script Details - {script.Provider} ({script.Model})");
        
        // Show metadata
        Console.WriteLine($"📅 Created: {script.CreatedAt:yyyy-MM-dd HH:mm:ss} ({GetTimeAgo(script.CreatedAt)})");
        Console.WriteLine($"🤖 Provider: {script.Provider} ({script.Model})");
        Console.WriteLine($"🎯 Type: {script.ScriptType}");
        Console.WriteLine($"⚠️ Risk: {script.RiskLevel} {GetRiskIcon(script.RiskLevel)}");
        Console.WriteLine();

        // Display the script
        _consoleUI.DisplayScript(script.Script, "Script Content");

        // Show warnings if any
        if (script.Warnings.Any())
        {
            Console.WriteLine();
            Console.WriteLine("⚠️ Warnings:");
            foreach (var warning in script.Warnings)
            {
                Console.WriteLine($"  • {warning.Message}");
            }
        }

        // Interactive options for the selected script
        Console.WriteLine();
        var options = new[]
        {
            "🚀 Execute script",
            "📋 Copy to clipboard", 
            "💾 Save to file",
            "🔙 Back to search results"
        };

        var choice = _consoleUI.DisplayInteractiveMenu(options);
        
        switch (choice)
        {
            case 0: // Execute
                var scriptExecutor = _serviceProvider.GetRequiredService<IScriptExecutor>();
                await ExecuteScriptWithConfirmation(script, scriptExecutor);
                break;
            case 1: // Copy
                await CopyScriptToClipboard(script);
                break;
            case 2: // Save
                await SaveScriptToFile(script);
                break;
            case 3: // Back
                return;
        }
    }

    private static string GetRiskIcon(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Low => "✅",
            RiskLevel.Medium => "⚠️",
            RiskLevel.High => "🔥",
            RiskLevel.Critical => "⛔",
            _ => "❓"
        };
    }

    private async Task ClearHistory()
    {
        var confirmOptions = new[] { "❌ No, keep history", "🗑️ Yes, clear all history" };
        var confirm = _consoleUI.DisplayInteractiveMenu(confirmOptions);
        
        if (confirm == 1)
        {
            var scriptRepository = _serviceProvider.GetRequiredService<IScriptRepository>();
            var result = await _consoleUI.DisplayProgressAsync(
                "🗑️ Clearing script history...",
                async () => await scriptRepository.ClearHistoryAsync()
            );
            
            if (result.IsSuccess)
            {
                _consoleUI.DisplayScript("✅ Script history cleared successfully.", "Success");
            }
            else
            {
                _consoleUI.DisplayRiskWarning("MEDIUM", new[] 
                { 
                    "Failed to clear script history",
                    $"Error: {result.Error}"
                });
            }
        }
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;
        
        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)}w ago";
        
        return dateTime.ToString("MMM dd");
    }
}
