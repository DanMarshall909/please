namespace Please.Domain.Common;

/// <summary>
/// Represents a void result with success/failure handling but no return value.
/// </summary>
public sealed class VoidResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; private set; }

    private VoidResult(bool success, string error)
    {
        IsSuccess = success;
        Error = error;
    }

    // Factory method to create a successful result
    public static VoidResult Success() => new(true, string.Empty);

    // Factory method to create a failed result
    public static VoidResult Failure(string error) => new(false, error);
}
