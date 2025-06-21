using TUnit;
using System.Threading.Tasks;
using Please.Domain.Common;

namespace Please.Domain.UnitTests.Common;

[TestFixture]
public class ResultTests
{
    [Test]
    public void a_success_result_indicates_success()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Test]
    public void a_failure_result_contains_the_error_message()
    {
        const string error = "something went wrong";
        var result = Result.Failure(error);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Test]
    public void a_generic_success_holds_the_value()
    {
        var result = Result<int>.Success(42);
        Assert.Equal(42, result.Value);
        Assert.True(result.IsSuccess);
    }

    [Test]
    public void mapping_transforms_the_value_when_successful()
    {
        var start = Result<int>.Success(2);
        var mapped = start.Map(x => x * 2);
        Assert.True(mapped.IsSuccess);
        Assert.Equal(4, mapped.Value);
    }

    [Test]
    public void mapping_preserves_the_error_when_failure()
    {
        var start = Result<int>.Failure("bad");
        var mapped = start.Map(x => x * 2);
        Assert.True(mapped.IsFailure);
        Assert.Equal("bad", mapped.Error);
    }

    [Test]
    public async Task mapping_async_transforms_the_value_when_successful()
    {
        var start = Result<int>.Success(3);
        var mapped = await start.MapAsync(x => Task.FromResult(x + 2));
        Assert.True(mapped.IsSuccess);
        Assert.Equal(5, mapped.Value);
    }
}
