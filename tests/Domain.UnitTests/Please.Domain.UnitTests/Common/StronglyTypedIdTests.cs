using Please.Domain.Entities;

namespace Please.Domain.UnitTests.Common;

public class StronglyTypedIdTests
{
    [Fact]
    public void a_strongly_typed_id_converts_to_the_underlying_value()
    {
        var id = ScriptId.From("00000000-0000-0000-0000-000000000001");
        Guid value = id;
        value.ShouldBe(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        id.ToString().ShouldBe("00000000-0000-0000-0000-000000000001");
    }

    [Fact]
    public void script_id_new_creates_a_unique_identifier()
    {
        var id1 = ScriptId.New();
        var id2 = ScriptId.New();
        id1.Value.ShouldNotBe(Guid.Empty);
        id1.Value.ShouldNotBe(id2.Value);
    }

    [Fact]
    public void provider_id_static_values_are_as_expected()
    {
        ProviderId.OpenAi.Value.ShouldBe("openai");
        ProviderId.Anthropic.Value.ShouldBe("anthropic");
        ProviderId.Ollama.Value.ShouldBe("ollama");
    }
}
