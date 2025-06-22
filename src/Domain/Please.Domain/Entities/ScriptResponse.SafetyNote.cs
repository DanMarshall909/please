namespace Please.Domain.Entities;

public partial record ScriptResponse
{
	/// <summary>
	/// Represents a safety note as a value object
	/// </summary>
	public record SafetyNote(string Message) : IMessage
	{
		public static implicit operator SafetyNote(string message) => new(message);
		public static implicit operator string(SafetyNote note) => note.Message;
	}
}

