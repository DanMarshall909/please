using NUnit.Framework;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.UnitTests;

[TestFixture]
public class ScriptResponseTests
{
    [Test]
    public void Test_script_response_with_valid_data_creates_successfully()
    {
        var result = ScriptResponse.Create(
            "echo hi",
            "Say hi",
            ProviderType.OpenAI,
            "gpt-4");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!.Script, Is.EqualTo("echo hi"));
    }

    [Test]
    public void Test_invalid_input_returns_failure_result()
    {
        var result = ScriptResponse.Create(
            "",
            "",
            ProviderType.OpenAI,
            "");

        Assert.That(result.IsFailure, Is.True);
    }
}
