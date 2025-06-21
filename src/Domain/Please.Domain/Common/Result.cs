using System;
using System.Threading.Tasks;
namespace Please.Domain.Common;

public abstract record Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; init; } = string.Empty;

    public static Result Success() => new SuccessResult();
    public static Result Failure(string error) => new FailureResult(error);

    protected Result() { }
}

public sealed record SuccessResult : Result
{
    public SuccessResult()
    {
        IsSuccess = true;
    }
}

public sealed record FailureResult : Result
{
    public FailureResult(string error)
    {
        IsSuccess = false;
        Error = error;
    }
}

public sealed record Result<T> : Result
{
    public T? Value { get; init; }

    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static Result<T> Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };

    public Result<TNext> Map<TNext>(Func<T, TNext> map)
    {
        return IsSuccess
            ? Result<TNext>.Success(map(Value!))
            : Result<TNext>.Failure(Error);
    }

    public async Task<Result<TNext>> MapAsync<TNext>(Func<T, Task<TNext>> map)
    {
        return IsSuccess
            ? Result<TNext>.Success(await map(Value!))
            : Result<TNext>.Failure(Error);
    }
}
