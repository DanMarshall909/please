using NUnit.Framework;
using Please.Domain.Exceptions;

namespace Please.Domain.UnitTests.Exceptions;

[TestFixture]
public class DomainExceptionTests
{
    [Test]
    public void unsupported_provider_message_contains_the_provider()
    {
        var ex = new UnsupportedProviderException("foo");
        Assert.That(ex.Message, Is.EqualTo("Unsupported provider: foo"));
    }

    [Test]
    public void unsupported_model_message_references_provider_and_model()
    {
        var ex = new UnsupportedModelException("p", "m");
        Assert.That(ex.Message, Is.EqualTo("Model 'm' is not supported by provider 'p'"));
    }

    [Test]
    public void script_generation_exception_preserves_the_message()
    {
        var ex = new ScriptGenerationException("msg");
        Assert.That(ex.Message, Is.EqualTo("msg"));
    }

    [Test]
    public void script_validation_exception_preserves_the_message()
    {
        var ex = new ScriptValidationException("oops");
        Assert.That(ex.Message, Is.EqualTo("oops"));
    }
}
