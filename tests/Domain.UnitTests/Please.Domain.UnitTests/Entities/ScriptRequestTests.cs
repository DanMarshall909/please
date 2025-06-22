using Shouldly;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Xunit;

namespace Please.Domain.UnitTests.Entities;

public class ScriptRequestTests
{
    [Fact]
    public void script_request_created_with_task_description_sets_required_properties()
    {
        // Arrange
        var taskDescription = "Deploy application to production";

        // Act
        var request = ScriptRequest.Create(taskDescription);

        // Assert
        request.TaskDescription.ShouldBe(taskDescription);
        (Math.Abs((request.RequestTime - DateTime.UtcNow).TotalSeconds) <= 1).ShouldBeTrue();
        request.AdditionalParameters.ShouldNotBeNull();
        request.AdditionalParameters.Count.ShouldBe(0);
    }

    [Fact]
    public void script_request_created_with_provider_and_model_sets_all_properties()
    {
        // Arrange
        var taskDescription = "Create backup script";
        var provider = ProviderType.OpenAI;
        var model = "gpt-4";

        // Act
        var request = ScriptRequest.Create(taskDescription, provider, model);

        // Assert
        request.TaskDescription.ShouldBe(taskDescription);
        request.Provider.ShouldBe(provider);
        request.Model.ShouldBe(model);
    }

    [Fact]
    public void script_request_with_working_directory_preserves_value()
    {
        // Arrange
        var workingDir = "/home/user/projects";

        // Act
        var request = new ScriptRequest
        {
            TaskDescription = "Test task",
            WorkingDirectory = workingDir
        };

        // Assert
        request.WorkingDirectory.ShouldBe(workingDir);
    }
}
