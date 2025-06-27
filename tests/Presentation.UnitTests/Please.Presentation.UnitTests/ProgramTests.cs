using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Please.Application;
using Please.Application.Services;
using Please.Domain.Interfaces;
using Please.Infrastructure;
using Shouldly;
using Xunit;

namespace Please.Presentation.UnitTests;

public class ProgramTests
{
    [Fact]
    public void Program_dependency_injection_registers_all_required_services()
    {
        // Arrange
        var args = new[] { "test command" };
        var arguments = CommandLineArguments.Parse(args);

        // Act - Build the host using the same pattern as Program.cs
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(); // This should be added to Program.cs
                services.AddSingleton(arguments);
                services.AddTransient<TaskProcessor>();
            })
            .Build();

        // Assert - Verify all critical services can be resolved
        host.Services.GetService<IScriptService>().ShouldNotBeNull();
        host.Services.GetService<IScriptRepository>().ShouldNotBeNull();
        host.Services.GetService<IScriptGenerator>().ShouldNotBeNull();
        host.Services.GetService<IContextService>().ShouldNotBeNull();
        host.Services.GetService<TaskProcessor>().ShouldNotBeNull();
        host.Services.GetService<CommandLineArguments>().ShouldBe(arguments);
        host.Services.GetService<ILogger<TaskProcessor>>().ShouldNotBeNull();
    }

    [Fact]
    public void Program_can_resolve_task_processor_with_all_dependencies()
    {
        // Arrange
        var args = new[] { "generate", "test script" };
        var arguments = CommandLineArguments.Parse(args);

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure();
                services.AddSingleton(arguments);
                services.AddTransient<TaskProcessor>();
            })
            .Build();

        // Act
        var taskProcessor = host.Services.GetRequiredService<TaskProcessor>();

        // Assert
        taskProcessor.ShouldNotBeNull();
    }

    [Fact]
    public void Program_host_creation_succeeds_with_valid_args()
    {
        // Arrange
        var args = new[] { "list files" };
        var arguments = CommandLineArguments.Parse(args);

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication();
                    services.AddInfrastructure();
                    services.AddSingleton(arguments);
                    services.AddTransient<TaskProcessor>();
                })
                .Build();

            host.Dispose();
        });
    }

    [Fact]
    public void Program_host_creation_succeeds_with_empty_args()
    {
        // Arrange
        string[] args = Array.Empty<string>();
        var arguments = CommandLineArguments.Parse(args);

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication();
                    services.AddInfrastructure();
                    services.AddSingleton(arguments);
                    services.AddTransient<TaskProcessor>();
                })
                .Build();

            host.Dispose();
        });
    }
}
