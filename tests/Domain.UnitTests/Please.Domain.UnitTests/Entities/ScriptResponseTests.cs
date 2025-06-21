using TUnit;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Domain.UnitTests.Entities;

[TestFixture]
public class ScriptResponseTests
{
    [Test]
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
        var requiresConfirmation = response.RequiresConfirmation;

        // Assert
        Assert.True(requiresConfirmation);
    }

    [Test]
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
        ).WithWarning("This command does nothing useful");

        // Act
        var requiresConfirmation = response.RequiresConfirmation;

        // Assert
        Assert.True(requiresConfirmation);
    }

    [Test]
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
        var requiresConfirmation = response.RequiresConfirmation;

        // Assert
        Assert.False(requiresConfirmation);
    }

    [Test]
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
        var isDangerous = response.IsDangerous;

        // Assert
        Assert.True(isDangerous);
    }

    [Test]
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
        var updatedResponse = response.WithWarning("Test warning");

        // Assert
        Assert.True(updatedResponse.Warnings.Contains("Test warning"));
        Assert.Equal(1, updatedResponse.Warnings.Count);
    }

    [Test]
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
        Assert.True(updatedResponse.SafetyNotes.Contains("Test safety note"));
        Assert.Equal(1, updatedResponse.SafetyNotes.Count);
    }
}
