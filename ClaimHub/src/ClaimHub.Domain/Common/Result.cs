namespace ClaimHub.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, string? error, string? code)
    {
        IsSuccess = isSuccess;
        Error = error;
        Code = code;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public string? Code { get; }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string error, string? code = null) => new(false, error, code);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error, string? code = null) => Result<T>.Failure(error, code);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, bool isSuccess, string? error, string? code)
        : base(isSuccess, error, code)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result has no value: " + Error);

    public static Result<T> Success(T value) => new(value, true, null, null);

    public static new Result<T> Failure(string error, string? code = null) => new(default, false, error, code);

    public Result<TNext> Map<TNext>(Func<T, TNext> mapper) =>
        IsSuccess ? Result<TNext>.Success(mapper(Value)) : Result<TNext>.Failure(Error!, Code);
}
