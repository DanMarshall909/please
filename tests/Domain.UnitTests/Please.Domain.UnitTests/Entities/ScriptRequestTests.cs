using NUnit.Framework;
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
        Assert.That(request.TaskDescription, Is.EqualTo(taskDescription));
        Assert.That(request.RequestTime, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
        Assert.That(request.AdditionalParameters, Is.Not.Null);
        Assert.That(request.AdditionalParameters, Is.Empty);
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
        Assert.That(request.TaskDescription, Is.EqualTo(taskDescription));
        Assert.That(request.Provider, Is.EqualTo(provider));
        Assert.That(request.Model, Is.EqualTo(model));
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
        Assert.That(request.WorkingDirectory, Is.EqualTo(workingDir));
    }
}
