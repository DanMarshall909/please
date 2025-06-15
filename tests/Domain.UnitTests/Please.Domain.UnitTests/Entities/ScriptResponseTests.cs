using NUnit.Framework;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Domain.UnitTests.Entities;

[TestFixture]
public class ScriptResponseTests
{
    [Test]
    public void RequiresConfirmation_WhenRiskLevelMedium_ShouldReturnTrue()
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
        Assert.That(requiresConfirmation, Is.True);
    }

    [Test]
    public void RequiresConfirmation_WhenHasWarnings_ShouldReturnTrue()
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
        Assert.That(requiresConfirmation, Is.True);
    }

    [Test]
    public void RequiresConfirmation_WhenLowRiskNoWarnings_ShouldReturnFalse()
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
        Assert.That(requiresConfirmation, Is.False);
    }

    [Test]
    public void IsDangerous_WhenRiskLevelHigh_ShouldReturnTrue()
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
        Assert.That(isDangerous, Is.True);
    }

    [Test]
    public void WithWarning_ShouldAddWarningToList()
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
        Assert.That(updatedResponse.Warnings, Contains.Item("Test warning"));
        Assert.That(updatedResponse.Warnings.Count, Is.EqualTo(1));
    }

    [Test]
    public void WithSafetyNote_ShouldAddNoteToList()
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
        Assert.That(updatedResponse.SafetyNotes, Contains.Item("Test safety note"));
        Assert.That(updatedResponse.SafetyNotes.Count, Is.EqualTo(1));
    }
}
