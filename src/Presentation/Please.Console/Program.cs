using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Application;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddApplication();

var provider = services.BuildServiceProvider();

// Entry point would resolve command handlers here
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
logger.LogInformation("Dependency injection configured.");
