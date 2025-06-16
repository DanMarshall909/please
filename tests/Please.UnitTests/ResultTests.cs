using Please.Domain.Common;

namespace Please.UnitTests;

public class ResultTests
{
    [Fact]
    public void Test_result_success_contains_value()
    {
        var result = Result<string>.Success("ok");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
    }

    [Fact]
    public void Test_result_failure_contains_error_message()
    {
        var result = Result<int>.Failure("fail");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("fail");
    }

    [Fact]
    public void Test_map_transforms_success_value()
    {
        var initial = Result<int>.Success(1);

        var mapped = initial.Map(v => v + 1);

        mapped.Value.Should().Be(2);
    }

    [Fact]
    public void Test_map_returns_failure_when_result_is_failure()
    {
        var initial = Result<int>.Failure("nope");

        var mapped = initial.Map(v => v + 1);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("nope");
    }

    [Fact]
    public async Task Test_map_async_transforms_success_value()
    {
        var initial = Result<int>.Success(1);

        var mapped = await initial.MapAsync(async v =>
        {
            await Task.Delay(1);
            return v + 2;
        });

        mapped.Value.Should().Be(3);
    }
}
