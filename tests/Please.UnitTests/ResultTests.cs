using NUnit.Framework;
using Please.Domain.Common;

namespace Please.UnitTests;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Test_result_success_contains_value()
    {
        var result = Result<string>.Success("ok");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("ok"));
    }

    [Test]
    public void Test_result_failure_contains_error_message()
    {
        var result = Result<int>.Failure("fail");
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("fail"));
    }
}
