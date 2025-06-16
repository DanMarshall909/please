namespace Please.Domain.Common;

public abstract record StronglyTypedId<T>(T Value)
    where T : IComparable<T>, IEquatable<T>
{
    public override string ToString() => Value?.ToString() ?? string.Empty;

    public static implicit operator T(StronglyTypedId<T> id) => id.Value;
}
