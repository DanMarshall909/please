using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Presentation.UnitTests
{
    public class TaskProcessorTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskProcessor> _logger;
        private readonly IScriptService _scriptService;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IConsoleUIService _consoleUI;

        public TaskProcessorTests()
        {
            _scriptService = Substitute.For<IScriptService>();
            _scriptExecutor = Substitute.For<IScriptExecutor>();
            _consoleUI = Substitute.For<IConsoleUIService>();
            _logger = Substitute.For<ILogger<TaskProcessor>>();

            var services = new ServiceCollection();
            services.AddSingleton(_scriptService);
            services.AddSingleton(_scriptExecutor);
            _serviceProvider = services.BuildServiceProvider();
        }

        private static ScriptResponse CreateScriptResponse(string script = "echo 'Hello World'")
        {
            return ScriptResponse.Create(
                script,
                "test task description",
                ProviderType.OpenAi,
                "gpt-3.5-turbo",
                ScriptType.PowerShell,
                RiskLevel.Low);
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenUserSelectsExecuteScript_ExecutesScript()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = CreateScriptResponse("echo 'Hello World'");

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            // Mock progress display to return the script service result
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<Result<ScriptResponse>>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());

            // Mock interactive menu to return "Execute script now" (index 0)
            _consoleUI.DisplayInteractiveMenu(Arg.Any<string[]>())
                .Returns(0);

            // Mock execution progress display
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task>>())
                .Returns(callInfo => callInfo.Arg<Func<Task>>()());

            _scriptExecutor.ExecuteScriptAsync(Arg.Any<string>())
                .Returns(Result<string>.Success("Script executed successfully"));

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _scriptExecutor.Received(1).ExecuteScriptAsync("echo 'Hello World'");
            _consoleUI.Received(1).DisplayBanner("6.0.0", "AI-Powered PowerShell Script Generator");
            _consoleUI.Received(1).DisplayScript("echo 'Hello World'", Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenUserSelectsCancel_DoesNotExecuteScript()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = CreateScriptResponse("echo 'Hello World'");

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            // Mock progress display to return the script service result
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<Result<ScriptResponse>>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());

            // Mock interactive menu to return "Cancel" (index 3)
            _consoleUI.DisplayInteractiveMenu(Arg.Any<string[]>())
                .Returns(3);

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _scriptExecutor.DidNotReceive().ExecuteScriptAsync(Arg.Any<string>());
            _consoleUI.Received(1).DisplayScript("Operation cancelled by user", "Information");
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenScriptGenerationFails_ShowsError()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Failure("API connection failed"));

            // Mock progress display to return the script service result
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<Result<ScriptResponse>>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            _consoleUI.Received(1).DisplayRiskWarning("HIGH", Arg.Is<string[]>(arr =>
                arr.Contains("Script generation failed") && arr.Contains("Error: API connection failed")));
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenNoArgumentsProvided_ShowsWarning()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new string[0]);
            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            _consoleUI.Received(1).DisplayBanner("6.0.0", "AI-Powered PowerShell Script Generator");
            _consoleUI.Received(1).DisplayRiskWarning("HIGH", Arg.Is<string[]>(arr =>
                arr.Contains("No task description provided")));
        }
    }
}
