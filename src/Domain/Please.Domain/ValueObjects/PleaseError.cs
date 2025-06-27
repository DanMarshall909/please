namespace Please.Domain.Common;

public record PleaseError(string Message)
{
    public static implicit operator PleaseError(string message) => new(message);
    public static implicit operator string(PleaseError error) => error.Message;
}
