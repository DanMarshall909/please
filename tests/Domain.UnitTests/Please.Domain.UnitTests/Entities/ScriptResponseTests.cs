using Shouldly;
using Xunit;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Domain.UnitTests.Entities;

public class ScriptResponseTests
{
    [Fact]
    public void requires_confirmation_is_true_when_risk_level_is_medium()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "rm -rf /",
            "Delete all files",
            ProviderType.OpenAI,
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
    public void requires_confirmation_is_true_when_response_has_warnings()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "echo 'safe command'",
            "Echo text",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash,
            RiskLevel.Low
        ).WithWarning(new ScriptResponse.Warning("This command does nothing useful"));

        // Act
        bool requiresConfirmation = response.RequiresConfirmation;

        // Assert
        requiresConfirmation.ShouldBeTrue();
    }

    [Fact]
    public void requires_confirmation_is_false_when_low_risk_and_no_warnings()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "ls -la",
            "List files",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash,
            RiskLevel.Low
        );

        // Act
        bool requiresConfirmation = response.RequiresConfirmation;

        // Assert
        requiresConfirmation.ShouldBeFalse();
    }

    [Fact]
    public void is_dangerous_is_true_when_risk_level_is_high()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "sudo rm -rf /",
            "Delete system files",
            ProviderType.OpenAI,
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
    public void with_warning_adds_warning_to_list()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "test script",
            "Test task",
            ProviderType.OpenAI,
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
    public void with_safety_note_adds_note_to_list()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "test script",
            "Test task",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash
        );

        // Act
        var updatedResponse = response.WithSafetyNote("Test safety note");

        // Assert
        updatedResponse.SafetyNotes.Contains("Test safety note").ShouldBeTrue();
        updatedResponse.SafetyNotes.Count.ShouldBe(1);
    }

    [Fact]
    public void created_at_is_settable_and_respected()
    {
        // Arrange
        var customDate = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var response = ScriptResponse.Create(
            "echo test",
            "Test created at",
            ProviderType.OpenAI,
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
