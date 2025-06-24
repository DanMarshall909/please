using Shouldly;
using Xunit;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Domain.UnitTests.Entities;

public class ScriptResponseTests
{
    [Fact]
    public void medium_risk_scripts_require_user_confirmation()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "rm -rf /",
            "Delete all files",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash,
            RiskLevel.Medium
        );

        // Act
        bool requiresConfirmation = response.RequiresConfirmation;

        // Assert
        requiresConfirmation.ShouldBeTrue();
    }

    [Fact]
    public void scripts_with_warnings_require_user_confirmation()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "echo 'safe command'",
            "Echo text",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash
        ).WithWarning(new ScriptResponse.Warning("This command does nothing useful"));

        // Act
        bool requiresConfirmation = response.RequiresConfirmation;

        // Assert
        requiresConfirmation.ShouldBeTrue();
    }

    [Fact]
    public void safe_scripts_without_warnings_do_not_require_confirmation()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "ls -la",
            "List files",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash
        );

        // Act
        bool requiresConfirmation = response.RequiresConfirmation;

        // Assert
        requiresConfirmation.ShouldBeFalse();
    }

    [Fact]
    public void high_risk_scripts_are_marked_as_dangerous()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "sudo rm -rf /",
            "Delete system files",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash,
            RiskLevel.High
        );

        // Act
        bool isDangerous = response.IsDangerous;

        // Assert
        isDangerous.ShouldBeTrue();
    }

    [Fact]
    public void script_response_can_collect_multiple_warnings()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "test script",
            "Test task",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash
        );

        // Act
        var updatedResponse = response.WithWarning(new ScriptResponse.Warning("Test warning"));

        // Assert
        updatedResponse.Warnings.Any(w => w.Message == "Test warning").ShouldBeTrue();
        updatedResponse.Warnings.Count.ShouldBe(1);
    }

    [Fact]
    public void script_response_can_collect_safety_recommendations()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "test script",
            "Test task",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash
        );

        // Act
        var updatedResponse = response.WithSafetyNote(new ScriptResponse.SafetyNote("Test safety note"));

        // Assert
        updatedResponse.SafetyNotes.Contains(new ScriptResponse.SafetyNote("Test safety note")).ShouldBeTrue();
        updatedResponse.SafetyNotes.Count.ShouldBe(1);
    }

    [Fact]
    public void script_response_preserves_custom_creation_timestamp()
    {
        // Arrange
        var customDate = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var response = ScriptResponse.Create(
            "echo test",
            "Test created at",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.Bash,
            RiskLevel.Low,
            customDate
        );

        // Act
        var createdAt = response.CreatedAt;

        // Assert
        createdAt.ShouldBe(customDate);
    }
}
