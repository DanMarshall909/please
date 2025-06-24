using Shouldly;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.TestUtilities.Builders;
using Xunit;

namespace Please.Domain.UnitTests.Entities;

public class ScriptRequestTests
{
    [Fact]
    public void new_script_request_contains_task_description_and_timestamp()
    {
        // Arrange
        var taskDescription = "Deploy application to production";

        // Act
        var request = ScriptRequestBuilder.Create()
            .WithTask(taskDescription)
            .Build();

        // Assert
        request.TaskDescription.ShouldBe(taskDescription);
        (Math.Abs((request.RequestTime - DateTime.UtcNow).TotalSeconds) <= 1).ShouldBeTrue();
        request.AdditionalParameters.ShouldNotBeNull();
        request.AdditionalParameters.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(ProviderType.OpenAi, "gpt-4")]
    [InlineData(ProviderType.Anthropic, "claude-3-sonnet")]
    [InlineData(ProviderType.Ollama, "llama2")]
    public void script_request_with_specific_ai_provider_stores_provider_details(ProviderType provider, string model)
    {
        // Arrange
        var taskDescription = "Create backup script";

        // Act
        var request = ScriptRequestBuilder.Create()
            .WithTask(taskDescription)
            .WithProvider(provider)
            .WithModel(model)
            .Build();

        // Assert
        request.TaskDescription.ShouldBe(taskDescription);
        request.Provider.ShouldBe(provider);
        request.Model.ShouldBe(model);
    }

    [Theory]
    [InlineData("/home/user/projects")]
    [InlineData("C:\\Users\\Developer\\Projects")]
    [InlineData("/var/www/html")]
    public void script_request_with_working_directory_preserves_location(string workingDir)
    {
        // Arrange & Act
        var request = ScriptRequestBuilder.Create()
            .WithTask("Test task")
            .WithWorkingDirectory(workingDir)
            .Build();

        // Assert
        request.WorkingDirectory.ShouldBe(workingDir);
    }

    [Theory]
    [InlineData(ScriptType.Bash)]
    [InlineData(ScriptType.PowerShell)]
    [InlineData(ScriptType.Python)]
    public void script_request_with_script_type_preserves_preference(ScriptType scriptType)
    {
        // Arrange & Act
        var request = ScriptRequestBuilder.Create()
            .WithTask("Generate script")
            .WithScriptType(scriptType)
            .Build();

        // Assert
        request.ScriptType.ShouldBe(scriptType);
    }

    [Fact]
    public void script_request_with_force_execution_flag_preserves_setting()
    {
        // Arrange & Act
        var request = ScriptRequestBuilder.Create()
            .WithTask("Dangerous operation")
            .WithForceExecution()
            .Build();

        // Assert
        request.ForceExecution.ShouldBeTrue();
    }

    [Fact]
    public void script_request_can_store_additional_parameters()
    {
        // Arrange & Act
        var request = ScriptRequestBuilder.Create()
            .WithTask("Custom task")
            .WithParameter("timeout", "30")
            .WithParameter("retry-count", "3")
            .Build();

        // Assert
        request.AdditionalParameters.Count.ShouldBe(2);
        request.AdditionalParameters["timeout"].ShouldBe("30");
        request.AdditionalParameters["retry-count"].ShouldBe("3");
    }
}
