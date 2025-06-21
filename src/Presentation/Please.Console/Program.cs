using Microsoft.Extensions.DependencyInjection;
using Please.Application;

var services = new ServiceCollection();
services.AddApplication();

var provider = services.BuildServiceProvider();

// Entry point would resolve command handlers here
Console.WriteLine("Dependency injection configured.");
