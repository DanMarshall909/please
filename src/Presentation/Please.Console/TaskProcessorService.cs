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

    public TaskProcessor(IServiceProvider serviceProvider, ILogger<TaskProcessor> logger,
        CommandLineArguments arguments)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _arguments = arguments;
    }

    public async Task ProcessTaskAsync()
    {
        if (!_arguments.HasInput)
        {
            _logger.LogError("❌ No task description provided. Please pass a task description as a program argument.");
            return;
        }

        string taskDescription = _arguments.TaskDescription;
        _logger.LogInformation("Processing task: {TaskDescription}", taskDescription);

        // Get required services
        var scriptService = _serviceProvider.GetRequiredService<IScriptService>();
        var userConfirmation = _serviceProvider.GetRequiredService<IUserConfirmation>();
        var scriptExecutor = _serviceProvider.GetRequiredService<IScriptExecutor>();

        // Create a script request using the task description
        var request = ScriptRequest.Create(
            taskDescription,
            ProviderType.OpenAi,
            "gpt-3.5-turbo"
        );

        try
        {
            var result = await scriptService.GenerateScriptAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Script generated successfully!");
                _logger.LogInformation("Script: {Script}", result.Value!.Script);
                _logger.LogInformation("Provider: {Provider}", result.Value!.Provider);
                _logger.LogInformation("Model: {Model}", result.Value!.Model);
                _logger.LogInformation("Risk Level: {RiskLevel}", result.Value!.RiskLevel);

                // Ask user for confirmation before executing
                var confirmationMessage = $"Do you want to execute this script?\nRisk Level: {result.Value!.RiskLevel}";
                var userApproves = userConfirmation.AskForConfirmation(confirmationMessage, result.Value!.Script);

                if (userApproves)
                {
                    _logger.LogInformation("User approved script execution. Executing...");

                    var executionResult = await scriptExecutor.ExecuteScriptAsync(result.Value!.Script);

                    if (executionResult.IsSuccess)
                    {
                        _logger.LogInformation("✅ Script executed successfully!");
                        if (!string.IsNullOrWhiteSpace(executionResult.Value))
                        {
                            Console.WriteLine("\n=== SCRIPT OUTPUT ===");
                            Console.WriteLine(executionResult.Value);
                            Console.WriteLine("=== END OUTPUT ===\n");
                        }
                        else
                        {
                            Console.WriteLine("Script completed with no output.");
                        }
                    }
                    else
                    {
                        _logger.LogError("❌ Script execution failed: {Error}", executionResult.Error);
                    }
                }
                else
                {
                    _logger.LogInformation("User declined script execution. Script not executed.");
                }
            }
            else
            {
                _logger.LogError("❌ Script generation failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Script generation failed: {Error}", ex.Message);
        }

        _logger.LogInformation("🎯 Task processing complete!");
    }
}
