using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Please.Application;
using Please.Infrastructure;
using Please.Console.Services;
using Please.Domain.Interfaces;

var arguments = CommandLineArguments.Parse(args);

// Set up cancellation token for Ctrl+C handling
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; // Prevent immediate exit
    cts.Cancel();
    Console.WriteLine("\n⚠️ Operation cancelled by user.");
};

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication(); // Register application services
        services.AddInfrastructure(); // Register infrastructure services
        services.AddSingleton(arguments); // Register parsed command-line arguments
        services.AddTransient<TaskProcessor>(); // Register the TaskProcessor
        services.AddTransient<IConsoleUIService, ConsoleUIService>(); // Register professional UI service
    })
    .Build();

try
{
    // Use the TaskProcessor to handle the task
    var taskProcessor = host.Services.GetRequiredService<TaskProcessor>();
    await taskProcessor.ProcessTaskAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("✅ Script generation cancelled gracefully.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
    Environment.Exit(1);
}
