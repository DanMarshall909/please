namespace Please.Domain.Commands;

/// <summary>
/// Represents the intent extracted from a user command
/// </summary>
public sealed record CommandIntent(string CommandText);
