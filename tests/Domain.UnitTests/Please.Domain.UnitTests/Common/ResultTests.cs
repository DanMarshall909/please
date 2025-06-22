using Xunit;
using Please.Domain.Common;
using Shouldly;

namespace Please.Domain.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void a_success_result_indicates_success()
    {
        var result = Result.Success();
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
    }

    [Fact]
    public void a_failure_result_contains_the_error_message()
    {
        const string error = "something went wrong";
        var result = Result.Failure(error);
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void a_generic_success_holds_the_value()
    {
        var result = Result<int>.Success(42);
        result.Value.ShouldBe(42);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void mapping_transforms_the_value_when_successful()
    {
        var start = Result<int>.Success(2);
        var mapped = start.Map(x => x * 2);
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(4);
    }

    [Fact]
    public void mapping_preserves_the_error_when_failure()
    {
        var start = Result<int>.Failure("bad");
        var mapped = start.Map(x => x * 2);
        mapped.IsFailure.ShouldBeTrue();
        mapped.Error.ShouldBe("bad");
    }

    [Fact]
    public async Task mapping_async_transforms_the_value_when_successful()
    {
        var start = Result<int>.Success(3);
        var mapped = await start.MapAsync(x => Task.FromResult(x + 2));
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(5);
    }
}
