using Shouldly;
using Please.Domain.Exceptions;
using Xunit;

namespace Please.Domain.UnitTests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void unsupported_provider_message_contains_the_provider()
    {
        var ex = new UnsupportedProviderException("foo");
        ex.Message.ShouldBe("Unsupported provider: foo");
    }

    [Fact]
    public void unsupported_model_message_references_provider_and_model()
    {
        var ex = new UnsupportedModelException("p", "m");
        ex.Message.ShouldBe("Model 'm' is not supported by provider 'p'");
    }

    [Fact]
    public void script_generation_exception_preserves_the_message()
    {
        var ex = new ScriptGenerationException("msg");
        ex.Message.ShouldBe("msg");
    }

    [Fact]
    public void script_validation_exception_preserves_the_message()
    {
        var ex = new ScriptValidationException("oops");
        ex.Message.ShouldBe("oops");
    }
}
