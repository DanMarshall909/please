namespace Please.Domain.Common;

/// <summary>
/// Represents a void result with success/failure handling but no return value.
/// </summary>
public sealed class VoidResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public PleaseError Error { get; private set; }

    private VoidResult(bool success, PleaseError error)
    {
        IsSuccess = success;
        Error = error;
    }

    // Factory method to create a successful result

    public static VoidResult Success => new(true, string.Empty);

    // Factory method to create a failed result
    public static VoidResult Failure(PleaseError error) => new(false, error);

    /// A pre-completed task that represents a successful <see cref="VoidResult"/> instance.
    /// </summary>
    /// <remarks>
    /// This task is used as a convenient way to return a successful result without any additional overhead.
    /// It is equivalent to <c>Task.FromResult(VoidResult.Success)</c>.
    /// </remarks>
    public static readonly Task<VoidResult> SuccessfulTask = Task.FromResult(Success);
}
