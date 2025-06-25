using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.TestUtilities;
using Please.TestUtilities.Builders;

namespace Please.Presentation.UnitTests
{
    public class TaskProcessorTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskProcessor> _logger;
        private readonly IScriptService _scriptService;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IUserConfirmation _userConfirmation;

        public TaskProcessorTests()
        {
            _scriptService = Substitute.For<IScriptService>();
            _scriptExecutor = Substitute.For<IScriptExecutor>();
            _userConfirmation = Substitute.For<IUserConfirmation>();
            _logger = Substitute.For<ILogger<TaskProcessor>>();

            var services = new ServiceCollection();
            services.AddSingleton(_scriptService);
            services.AddSingleton(_scriptExecutor);
            services.AddSingleton(_userConfirmation);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenUserConfirmsExecution_ExecutesScript()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = ScriptResponseBuilder.Create()
                .WithScript("echo 'Hello World'")
                .WithProvider(ProviderType.OpenAi)
                .WithModel("gpt-3.5-turbo")
                .Build();

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            _userConfirmation.AskForConfirmation(Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);

            _scriptExecutor.ExecuteScriptAsync(Arg.Any<string>())
                .Returns(Result<string>.Success("Script executed successfully"));

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _scriptExecutor.Received(1).ExecuteScriptAsync("echo 'Hello World'");
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenUserDeclinesExecution_DoesNotExecuteScript()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = ScriptResponseBuilder.Create()
                .WithScript("echo 'Hello World'")
                .WithProvider(ProviderType.OpenAi)
                .WithModel("gpt-3.5-turbo")
                .Build();

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            _userConfirmation.AskForConfirmation(Arg.Any<string>(), Arg.Any<string>())
                .Returns(false);

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments);

            // Act
            await taskProcessor.ProcessTaskAsync();

            // Assert
            await _scriptExecutor.DidNotReceive().ExecuteScriptAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenScriptExecutionFails_DoesNotThrow()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = ScriptResponseBuilder.Create()
                .WithScript("invalid command")
                .WithProvider(ProviderType.OpenAi)
                .WithModel("gpt-3.5-turbo")
                .Build();

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            _userConfirmation.AskForConfirmation(Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);

            _scriptExecutor.ExecuteScriptAsync(Arg.Any<string>())
                .Returns(Result<string>.Failure("Command not found"));

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments);

            // Act & Assert - should not throw
            await taskProcessor.ProcessTaskAsync();

            // Verify script execution was attempted
            await _scriptExecutor.Received(1).ExecuteScriptAsync("invalid command");
        }

        [Fact]
        public async Task ProcessTaskAsync_WhenScriptExecutionSucceeds_CompletesSuccessfully()
        {
            // Arrange
            var arguments = CommandLineArguments.Parse(new[] { "create a test file" });
            var scriptResponse = ScriptResponseBuilder.Create()
                .WithScript("echo 'Hello World'")
                .WithProvider(ProviderType.OpenAi)
                .WithModel("gpt-3.5-turbo")
                .Build();

            _scriptService.GenerateScriptAsync(Arg.Any<ScriptRequest>())
                .Returns(Result<ScriptResponse>.Success(scriptResponse));

            _userConfirmation.AskForConfirmation(Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);

            _scriptExecutor.ExecuteScriptAsync(Arg.Any<string>())
                .Returns(Result<string>.Success("Hello World"));

            var taskProcessor = new TaskProcessor(_serviceProvider, _logger, arguments);

            // Act & Assert - should not throw
            await taskProcessor.ProcessTaskAsync();

            // Verify script execution was attempted
            await _scriptExecutor.Received(1).ExecuteScriptAsync("echo 'Hello World'");
        }
    }
}
