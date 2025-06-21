using System;
using NUnit.Framework;
using Please.Domain.Common;
using Please.Domain.Entities;

namespace Please.Domain.UnitTests.Common;

[TestFixture]
public class StronglyTypedIdTests
{
    [Test]
    public void a_strongly_typed_id_converts_to_the_underlying_value()
    {
        var id = ScriptId.From("00000000-0000-0000-0000-000000000001");
        Guid value = id;
        Assert.That(value, Is.EqualTo(Guid.Parse("00000000-0000-0000-0000-000000000001")));
        Assert.That(id.ToString(), Is.EqualTo("00000000-0000-0000-0000-000000000001"));
    }

    [Test]
    public void script_id_new_creates_a_unique_identifier()
    {
        var id1 = ScriptId.New();
        var id2 = ScriptId.New();
        Assert.That(id1.Value, Is.Not.EqualTo(Guid.Empty));
        Assert.That(id2.Value, Is.Not.EqualTo(id1.Value));
    }

    [Test]
    public void provider_id_static_values_are_as_expected()
    {
        Assert.That(ProviderId.OpenAI.Value, Is.EqualTo("openai"));
        Assert.That(ProviderId.Anthropic.Value, Is.EqualTo("anthropic"));
        Assert.That(ProviderId.Ollama.Value, Is.EqualTo("ollama"));
    }
}
