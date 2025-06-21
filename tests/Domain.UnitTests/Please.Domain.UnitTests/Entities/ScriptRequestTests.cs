using TUnit;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Domain.UnitTests.Entities;

[TestFixture]
public class ScriptRequestTests
{
    [Test]
    public void script_request_created_with_task_description_sets_required_properties()
    {
        // Arrange
        var taskDescription = "Deploy application to production";

        // Act
        var request = ScriptRequest.Create(taskDescription);

        // Assert
        Assert.Equal(taskDescription, request.TaskDescription);
        Assert.True(Math.Abs((request.RequestTime - DateTime.UtcNow).TotalSeconds) <= 1);
        Assert.True(request.AdditionalParameters != null);
        Assert.True(request.AdditionalParameters.Count == 0);
    }

    [Test]
    public void script_request_created_with_provider_and_model_sets_all_properties()
    {
        // Arrange
        var taskDescription = "Create backup script";
        var provider = ProviderType.OpenAI;
        var model = "gpt-4";

        // Act
        var request = ScriptRequest.Create(taskDescription, provider, model);

        // Assert
        Assert.Equal(taskDescription, request.TaskDescription);
        Assert.Equal(provider, request.Provider);
        Assert.Equal(model, request.Model);
    }

    [Test]
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
        Assert.Equal(workingDir, request.WorkingDirectory);
    }
}
