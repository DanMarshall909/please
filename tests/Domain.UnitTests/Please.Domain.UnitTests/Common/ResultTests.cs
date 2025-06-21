using NUnit.Framework;
using System.Threading.Tasks;
using Please.Domain.Common;

namespace Please.Domain.UnitTests.Common;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Test_success_result_indicates_success()
    {
        var result = Result.Success();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.IsFailure, Is.False);
    }

    [Test]
    public void Test_failure_result_contains_error_message()
    {
        const string error = "something went wrong";
        var result = Result.Failure(error);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(error));
    }

    [Test]
    public void Test_generic_success_holds_value()
    {
        var result = Result<int>.Success(42);
        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Test_map_transforms_value_when_success()
    {
        var start = Result<int>.Success(2);
        var mapped = start.Map(x => x * 2);
        Assert.That(mapped.IsSuccess, Is.True);
        Assert.That(mapped.Value, Is.EqualTo(4));
    }

    [Test]
    public void Test_map_preserves_error_when_failure()
    {
        var start = Result<int>.Failure("bad");
        var mapped = start.Map(x => x * 2);
        Assert.That(mapped.IsFailure, Is.True);
        Assert.That(mapped.Error, Is.EqualTo("bad"));
    }

    [Test]
    public async Task Test_map_async_transforms_value_when_success()
    {
        var start = Result<int>.Success(3);
        var mapped = await start.MapAsync(x => Task.FromResult(x + 2));
        Assert.That(mapped.IsSuccess, Is.True);
        Assert.That(mapped.Value, Is.EqualTo(5));
    }
}
