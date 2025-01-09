namespace TableConverter.FileConverters.DataModels;

public class Result(string? error = null)
{
    public string? Error { get; } = error;

    public bool IsSuccess => string.IsNullOrEmpty(Error);

    public static Result Success()
    {
        return new Result();
    }

    public static Result Failure(string error)
    {
        return new Result(error);
    }
}

public class Result<T>(T value, string? error = null) : Result(error)
{
    public T Value { get; } = value;

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public new static Result<T> Failure(string error)
    {
        return new Result<T>(default!, error);
    }
}