using Microsoft.Extensions.Logging;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Services;

/// <summary>
/// Provides user confirmation through console input.
/// </summary>
public class ConsoleUserConfirmation : IUserConfirmation
{
    private readonly ILogger<ConsoleUserConfirmation> _logger;

    public ConsoleUserConfirmation(ILogger<ConsoleUserConfirmation> logger)
    {
        _logger = logger;
    }

    public bool AskForConfirmation(string message, string scriptContent)
    {
        try
        {
            _logger.LogInformation("Requesting user confirmation for script execution");

            Console.WriteLine();
            Console.WriteLine("=== SCRIPT CONFIRMATION ===");
            Console.WriteLine(message);
            Console.WriteLine();
            Console.WriteLine("Script to execute:");
            Console.WriteLine("==================");
            Console.WriteLine(scriptContent);
            Console.WriteLine("==================");
            Console.WriteLine();
            Console.Write("Do you want to execute this script? (y/N): ");

            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            var approved = response == "y" || response == "yes";

            _logger.LogInformation("User confirmation result: {Approved}", approved);
            return approved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while requesting user confirmation");
            return false; // Default to not executing on error
        }
    }
}
