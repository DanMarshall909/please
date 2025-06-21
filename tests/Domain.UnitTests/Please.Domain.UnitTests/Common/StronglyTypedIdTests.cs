using System;
using TUnit;
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
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), value);
        Assert.Equal("00000000-0000-0000-0000-000000000001", id.ToString());
    }

    [Test]
    public void script_id_new_creates_a_unique_identifier()
    {
        var id1 = ScriptId.New();
        var id2 = ScriptId.New();
        Assert.NotEqual(Guid.Empty, id1.Value);
        Assert.NotEqual(id1.Value, id2.Value);
    }

    [Test]
    public void provider_id_static_values_are_as_expected()
    {
        Assert.Equal("openai", ProviderId.OpenAI.Value);
        Assert.Equal("anthropic", ProviderId.Anthropic.Value);
        Assert.Equal("ollama", ProviderId.Ollama.Value);
    }
}
