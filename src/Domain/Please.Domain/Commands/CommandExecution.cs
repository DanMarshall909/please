using System;

namespace Please.Domain.Commands;

/// <summary>
/// Represents a completed command execution for learning
/// </summary>
public sealed record CommandExecution(string CommandText, DateTime ExecutedAt)
{
    public static CommandExecution Create(string commandText) =>
        new(commandText, DateTime.UtcNow);
}


