using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.UnitTests;

public class ScriptResponseTests
{
    [Fact]
    public void Test_script_response_with_valid_data_creates_successfully()
    {
        var result = ScriptResponse.Create(
            "echo hi",
            "Say hi",
            ProviderType.OpenAI,
            "gpt-4");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Script.Should().Be("echo hi");
    }

    [Fact]
    public void Test_invalid_input_returns_failure_result()
    {
        var result = ScriptResponse.Create(
            "",
            "",
            ProviderType.OpenAI,
            "");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Test_with_warning_adds_warning_to_list()
    {
        var response = ScriptResponse.Create(
            "cmd",
            "task",
            ProviderType.OpenAI,
            "gpt-4"
        ).Value!;

        response.WithWarning("be careful");

        response.Warnings.Should().ContainSingle("be careful");
    }

    [Fact]
    public void Test_with_safety_note_adds_note_to_list()
    {
        var response = ScriptResponse.Create(
            "cmd",
            "task",
            ProviderType.OpenAI,
            "gpt-4"
        ).Value!;

        response.WithSafetyNote("use a sandbox");

        response.SafetyNotes.Should().ContainSingle("use a sandbox");
    }

    [Fact]
    public void Test_requires_confirmation_is_true_when_risk_is_high()
    {
        var result = ScriptResponse.Create(
            "cmd",
            "task",
            ProviderType.OpenAI,
            "gpt-4",
            scriptType: ScriptType.Bash,
            riskLevel: RiskLevel.High);

        result.Value!.RequiresConfirmation.Should().BeTrue();
        result.Value.IsDangerous.Should().BeTrue();
    }

    [Fact]
    public void Test_requires_confirmation_is_true_when_warning_added()
    {
        var response = ScriptResponse.Create(
            "cmd",
            "task",
            ProviderType.OpenAI,
            "gpt-4"
        ).Value!;

        response.WithWarning("be careful");

        response.RequiresConfirmation.Should().BeTrue();
    }
}
