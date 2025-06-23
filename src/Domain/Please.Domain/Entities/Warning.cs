namespace Please.Domain.Entities;

public partial record ScriptResponse
{
    /// <summary>
    /// Represents a warning as a value object
    /// </summary>
    public record Warning(string Message) : IMessage
    {
        public static implicit operator Warning(string message) => new(message);
    }
}
