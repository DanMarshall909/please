using NUnit.Framework;
using Please.Domain.Entities;

namespace Please.UnitTests;

[TestFixture]
public class StronglyTypedIdTests
{
    [Test]
    public void Test_script_id_generates_unique_guid()
    {
        var id1 = ScriptId.New();
        var id2 = ScriptId.New();
        Assert.That(id1, Is.Not.EqualTo(id2));
    }

    [Test]
    public void Test_script_id_converts_to_underlying_value()
    {
        var guid = Guid.NewGuid();
        ScriptId id = new(guid);
        Guid value = id;
        Assert.That(value, Is.EqualTo(guid));
    }
}
