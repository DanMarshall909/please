using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application.Services;
using Please.Console;
using Please.Domain.Entities;
using Please.Domain.Enums;

var provider = PleaseHost.CreateServiceProvider();

// Entry point resolves services directly through DI
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
logger.LogInformation("Please v6 C# - Infrastructure layer complete!");

// Demonstrate end-to-end functionality using direct services
var scriptService = provider.GetRequiredService<IScriptService>();

// Create a sample script request
var request = ScriptRequest.Create(
    "list files in current directory",
    ProviderType.OpenAi,
    "gpt-3.5-turbo"
);

try
{
    var result = await scriptService.GenerateScriptAsync(request);

    if (result.IsSuccess)
    {
        logger.LogInformation("✅ Script generated successfully!");
        logger.LogInformation("Script: {Script}", result.Value!.Script);
        logger.LogInformation("Provider: {Provider}", result.Value!.Provider);
        logger.LogInformation("Model: {Model}", result.Value!.Model);
        logger.LogInformation("Risk Level: {RiskLevel}", result.Value!.RiskLevel);
    }
    else
    {
        logger.LogError("❌ Script generation failed: {Error}", result.Error);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Script generation failed: {Error}", ex.Message);
}

logger.LogInformation("🎯 Please v6 C# demonstration complete!");
