using Please.Domain.Common;

namespace Please.Domain.Entities;

public sealed record ScriptId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ScriptId New() => new(Guid.NewGuid());
    public static ScriptId From(string value) => new(Guid.Parse(value));
    public static ScriptId Empty => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}
