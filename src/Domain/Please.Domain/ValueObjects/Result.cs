namespace Please.Domain.Common;

// The generic record for representing success/failure results.
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; init; } = string.Empty;
    public T? Value { get; init; }

    // Factory method to create a Success result.
    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    // Factory method to create a Failure result.
    public static Result<T> Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };

    // Maps this result to a new type, TNext, applying the given transformation.
    public Result<TNext> Map<TNext>(Func<T, TNext> map) =>
        IsSuccess
            ? Result<TNext>.Success(map(Value!))
            : Result<TNext>.Failure(Error);

    // Asynchronously maps this result to a new type, TNext.
    public async Task<Result<TNext>> MapAsync<TNext>(Func<T, Task<TNext>> map) =>
        IsSuccess
            ? Result<TNext>.Success(await map(Value!))
            : Result<TNext>.Failure(Error);

    // Implicit conversion for direct assignment.
    public static implicit operator Result<T>(T value) => Success(value);
}

// Static utility class for the creation of generic Result<T> instances.
public static class Result
{
    // Public factory methods for convenience.
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}
