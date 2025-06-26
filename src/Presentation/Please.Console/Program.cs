using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Please.Application;
using Please.Infrastructure;
using Please.Console.Services;
using Please.Domain.Interfaces;

var arguments = CommandLineArguments.Parse(args);

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

// Use the TaskProcessor to handle the task
var taskProcessor = host.Services.GetRequiredService<TaskProcessor>();
await taskProcessor.ProcessTaskAsync();
