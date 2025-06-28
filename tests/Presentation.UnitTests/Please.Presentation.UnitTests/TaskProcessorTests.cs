using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.TestUtilities;

namespace Please.Presentation.UnitTests
{
    public class TaskProcessorTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskProcessor> _logger;
        private readonly IScriptService _scriptService;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IConsoleUIService _consoleUI;
        private readonly IClipboardService _clipboardService;
        private readonly IFileService _fileService;

        public TaskProcessorTests()
        {
            _scriptService = Substitute.For<IScriptService>();
            _scriptExecutor = Substitute.For<IScriptExecutor>();
            _consoleUI = Substitute.For<IConsoleUIService>();
            _clipboardService = Substitute.For<IClipboardService>();
            _fileService = Substitute.For<IFileService>();
            _logger = Substitute.For<ILogger<TaskProcessor>>();

            _serviceProvider = TestSystem.Create(services =>
            {
                // Override test doubles with our specific mocks
                services.AddSingleton(_scriptService);
                services.AddSingleton(_scriptExecutor);
                services.AddSingleton(_consoleUI);
                services.AddSingleton(_clipboardService);
                services.AddSingleton(_fileService);
            });
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
        public async Task Executes_script_when_user_selects_execute_option()
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

            // Mock confirm execution to return true
            _consoleUI.ConfirmScriptExecution(Arg.Any<ScriptResponse>())
                .Returns(true);

            // Mock execution progress display
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task>>())
                .Returns(callInfo => callInfo.Arg<Func<Task>>()());

            _scriptExecutor.ExecuteScriptAsync(Arg.Any<string>())
                .Returns(Result<string>.Success("Script executed successfully"));

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _scriptExecutor.Received(1).ExecuteScriptAsync("echo 'Hello World'");
            _consoleUI.Received(1).DisplayBanner("6.0.0", "AI-Powered PowerShell Script Generator");
            _consoleUI.Received(1).DisplayScript("echo 'Hello World'", Arg.Any<string>());
        }

        [Fact]
        public async Task Does_not_execute_script_when_user_cancels()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = CreateScriptResponse("echo 'Hello World'");

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            // Mock progress display to return the script service result
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<Result<ScriptResponse>>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());

            // Mock interactive menu to return "Cancel" (index 4)
            _consoleUI.DisplayInteractiveMenu(Arg.Any<string[]>())
                .Returns(4);

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _scriptExecutor.DidNotReceive().ExecuteScriptAsync(Arg.Any<string>());
            _consoleUI.Received(1).DisplayScript("Operation cancelled by user", "Information");
        }

        [Fact]
        public async Task Shows_error_when_script_generation_fails()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Failure("API connection failed"));

            // Mock progress display to return the script service result
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<Result<ScriptResponse>>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            _consoleUI.Received(1).DisplayRiskWarning("HIGH", Arg.Is<string[]>(arr =>
                arr.Contains("Script generation failed") && arr.Contains("Error: API connection failed")));
        }

        [Fact]
        public async Task Shows_help_when_no_arguments_provided()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new string[0]);
            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            _consoleUI.Received(1).DisplayBanner("6.0.0", "AI-Powered PowerShell Script Generator");
            // The implementation shows help via Console.WriteLine, not DisplayRiskWarning
            // Since ShowHelp() writes to Console directly, we can't easily verify it
            // But we can verify the banner was shown and script generation wasn't attempted
            await _scriptService.DidNotReceive().GenerateScriptAsync(Arg.Any<ScriptRequest>());
        }

        [Fact]
        public async Task Copies_script_to_clipboard_when_user_selects_copy_option()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = CreateScriptResponse("echo 'Hello World'");

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            // Mock progress display to return the script service result
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<Result<ScriptResponse>>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());

            // Mock interactive menu to return "Copy to clipboard" (index 2)
            _consoleUI.DisplayInteractiveMenu(Arg.Any<string[]>())
                .Returns(2);

            // Mock clipboard service to be supported and successful
            _clipboardService.IsSupported().Returns(true);
            _clipboardService.SetTextAsync(Arg.Any<string>()).Returns(true);

            // Mock clipboard progress display
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<bool>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<bool>>>()());

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _clipboardService.Received(1).SetTextAsync("echo 'Hello World'");
            _clipboardService.Received(1).IsSupported();
            _consoleUI.Received(1).DisplayScript("✅ Script successfully copied to clipboard!", "Success");
        }

        [Fact]
        public async Task Shows_warning_when_clipboard_not_supported()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = CreateScriptResponse("echo 'Hello World'");

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            // Mock progress display to return the script service result
            _consoleUI.DisplayProgressAsync(Arg.Any<string>(), Arg.Any<Func<Task<Result<ScriptResponse>>>>())
                .Returns(callInfo => callInfo.Arg<Func<Task<Result<ScriptResponse>>>>()());

            // Mock interactive menu to return "Copy to clipboard" (index 2)
            _consoleUI.DisplayInteractiveMenu(Arg.Any<string[]>())
                .Returns(2);

            // Mock clipboard service to be not supported
            _clipboardService.IsSupported().Returns(false);

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _clipboardService.DidNotReceive().SetTextAsync(Arg.Any<string>());
            _clipboardService.Received(1).IsSupported();
            _consoleUI.Received(1).DisplayRiskWarning("MEDIUM", Arg.Is<string[]>(arr =>
                arr.Any(s => s.Contains("Clipboard operations are not supported"))));
        }

        [Fact]
        public async Task Shows_history_when_history_command_provided()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "--history" });
            var scriptRepository = Substitute.For<IScriptRepository>();
            
            // Mock repository to indicate no history exists
            scriptRepository.HasHistoryAsync().Returns(Task.FromResult(Result<bool>.Success(false)));
            
            // Create a new service provider that includes our mocked repository
            var serviceProvider = TestSystem.Create(services =>
            {
                // Override test doubles with our specific mocks
                services.AddSingleton(_scriptService);
                services.AddSingleton(_scriptExecutor);
                services.AddSingleton(_consoleUI);
                services.AddSingleton(_clipboardService);
                services.AddSingleton(_fileService);
                services.AddSingleton<IScriptRepository>(scriptRepository);
            });
            
            var taskProcessor = new TaskProcessor(serviceProvider, _logger, arguments, _consoleUI, _clipboardService, _fileService);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            _consoleUI.Received(1).DisplayBanner("6.0.0", "Script History");
            _consoleUI.Received(1).DisplayScript("📝 No scripts found in history.", "Information");
            _consoleUI.Received(1).DisplayScript("💡 Generate some scripts first, then use 'please --history' to view them.", "Tip");
            
            // Verify repository was called
            await scriptRepository.Received(1).HasHistoryAsync();
        }
    }
}
