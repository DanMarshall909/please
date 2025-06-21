using TUnit;
using Please.Domain.Exceptions;

namespace Please.Domain.UnitTests.Exceptions;

[TestFixture]
public class DomainExceptionTests
{
    [Test]
    public void unsupported_provider_message_contains_the_provider()
    {
        var ex = new UnsupportedProviderException("foo");
        Assert.Equal("Unsupported provider: foo", ex.Message);
    }

    [Test]
    public void unsupported_model_message_references_provider_and_model()
    {
        var ex = new UnsupportedModelException("p", "m");
        Assert.Equal("Model 'm' is not supported by provider 'p'", ex.Message);
    }

    [Test]
    public void script_generation_exception_preserves_the_message()
    {
        var ex = new ScriptGenerationException("msg");
        Assert.Equal("msg", ex.Message);
    }

    [Test]
    public void script_validation_exception_preserves_the_message()
    {
        var ex = new ScriptValidationException("oops");
        Assert.Equal("oops", ex.Message);
    }
}
