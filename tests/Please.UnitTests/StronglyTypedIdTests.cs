using Please.Domain.Entities;

namespace Please.UnitTests;

public class StronglyTypedIdTests
{
    [Fact]
    public void Test_script_id_generates_unique_guid()
    {
        var id1 = ScriptId.New();
        var id2 = ScriptId.New();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Test_script_id_converts_to_underlying_value()
    {
        var guid = Guid.NewGuid();
        ScriptId id = new(guid);

        Guid value = id;
        value.Should().Be(guid);
    }

    [Fact]
    public void Test_ToString_returns_guid_string()
    {
        var id = ScriptId.New();

        id.ToString().Should().Be(id.Value.ToString());
    }
}
