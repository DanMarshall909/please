using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.UnitTests;

public class ScriptRequestTests
{
    [Fact]
    public void Test_create_returns_success_with_trimmed_description()
    {
        var result = ScriptRequest.Create("  list files  ", ProviderType.OpenAI, "gpt-4");

        result.IsSuccess.Should().BeTrue();
        result.Value!.TaskDescription.Should().Be("list files");
    }

    [Fact]
    public void Test_create_with_empty_description_returns_failure()
    {
        var result = ScriptRequest.Create("   ");

        result.IsFailure.Should().BeTrue();
    }
}
