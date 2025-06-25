namespace Please.Domain.ValueObjects;

public record struct ScriptId
{
    public ScriptId(Guid Value) => this.Value = Value;

    public ScriptId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ScriptId cannot be null or empty.", nameof(value));

        Value = Guid.Parse(value);
    }

    public static ScriptId New() => Guid.NewGuid();
    public static ScriptId From(string value) => Guid.Parse(value);
    public static implicit operator Guid(ScriptId id) => id.Value;
    public static implicit operator ScriptId(Guid value) => new(value);
    public Guid Value { get; set; }

    public readonly void Deconstruct(out Guid Value)
    {
        Value = this.Value;
    }

    public override readonly string ToString() => Value.ToString();
}
