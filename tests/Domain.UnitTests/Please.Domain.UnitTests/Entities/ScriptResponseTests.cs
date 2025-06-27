using Please.Domain.Enums;
using Please.TestUtilities.Builders;

namespace Please.Domain.UnitTests.Entities;

public class ScriptResponseTests
{
    [Theory]
    [InlineData(RiskLevel.Medium, true)]
    [InlineData(RiskLevel.High, true)]
    [InlineData(RiskLevel.Critical, true)]
    [InlineData(RiskLevel.Low, false)]
    public void scripts_with_elevated_risk_require_user_confirmation(RiskLevel riskLevel, bool expectedConfirmation)
    {
        // Arrange
        var response = ScriptResponseBuilder.Create()
            .WithRiskLevel(riskLevel)
            .Build();

        // Act & Assert
        response.RequiresConfirmation.ShouldBe(expectedConfirmation);
    }

    [Fact]
    public void scripts_with_warnings_require_user_confirmation()
    {
        // Arrange
        var response = ScriptResponseBuilder.Create()
            .WithWarning("This command does nothing useful")
            .Build();

        // Act & Assert
        response.RequiresConfirmation.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RiskLevel.High, true)]
    [InlineData(RiskLevel.Critical, true)]
    [InlineData(RiskLevel.Medium, false)]
    [InlineData(RiskLevel.Low, false)]
    public void high_risk_scripts_are_marked_as_dangerous(RiskLevel riskLevel, bool expectedDangerous)
    {
        // Arrange
        var response = ScriptResponseBuilder.Create()
            .WithRiskLevel(riskLevel)
            .Build();

        // Act & Assert
        response.IsDangerous.ShouldBe(expectedDangerous);
    }

    [Fact]
    public void script_response_can_collect_multiple_warnings()
    {
        // Arrange
        var response = ScriptResponseBuilder.Create()
            .WithWarning("First warning")
            .WithWarning("Second warning")
            .Build();

        // Act & Assert
        response.Warnings.Count.ShouldBe(2);
        response.Warnings.Any(w => w.Message == "First warning").ShouldBeTrue();
        response.Warnings.Any(w => w.Message == "Second warning").ShouldBeTrue();
    }

    [Fact]
    public void script_response_can_collect_safety_recommendations()
    {
        // Arrange
        var response = ScriptResponseBuilder.Create()
            .WithSafetyNote("First safety note")
            .WithSafetyNote("Second safety note")
            .Build();

        // Act & Assert
        response.SafetyNotes.Count.ShouldBe(2);
        response.SafetyNotes.Any(n => n.Message == "First safety note").ShouldBeTrue();
        response.SafetyNotes.Any(n => n.Message == "Second safety note").ShouldBeTrue();
    }

    [Fact]
    public void script_response_preserves_custom_creation_timestamp()
    {
        // Arrange
        var customDate = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var response = ScriptResponseBuilder.Create()
            .WithCreatedAt(customDate)
            .Build();

        // Act & Assert
        response.CreatedAt.ShouldBe(customDate);
    }

    [Theory]
    [InlineData(ProviderType.OpenAi, "gpt-4")]
    [InlineData(ProviderType.Anthropic, "claude-3")]
    [InlineData(ProviderType.Ollama, "llama2")]
    public void script_response_stores_provider_and_model_details(ProviderType provider, string model)
    {
        // Arrange & Act
        var response = ScriptResponseBuilder.Create()
            .WithProvider(provider)
            .WithModel(model)
            .Build();

        // Assert
        response.Provider.ShouldBe(provider);
        response.Model.ShouldBe(model);
    }
}
