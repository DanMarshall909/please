using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Please.Console;

var provider = PleaseHost.CreateServiceProvider();

// Entry point would resolve command handlers here
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
logger.LogInformation("Dependency injection configured.");
