namespace TableConverter.FileConverters.DataModels;

/// <summary>
///     Represents the result of an operation, including whether it was successful and an optional error message.
/// </summary>
public class Result
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result" /> class.
    /// </summary>
    /// <param name="error">An optional error message. If null or empty, the result is considered a success.</param>
    public Result(string? error = null)
    {
        Error = error;
    }

    /// <summary>
    ///     Gets the error message if the operation failed, otherwise null.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful (i.e., no error).
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    ///     Creates a successful result with no error.
    /// </summary>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public static Result Success()
    {
        return new Result();
    }

    /// <summary>
    ///     Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A <see cref="Result" /> indicating failure.</returns>
    public static Result Failure(string error)
    {
        return new Result(error);
    }
}

/// <summary>
///     Represents the result of an operation that returns a value of type <typeparamref name="T" />, including whether it
///     was successful and an optional error message.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result{T}" /> class.
    /// </summary>
    /// <param name="value">The value returned by the operation.</param>
    /// <param name="error">An optional error message. If null or empty, the result is considered a success.</param>
    public Result(T value, string? error = null) : base(error)
    {
        Value = value;
    }

    /// <summary>
    ///     Gets the value returned by the operation. This will be the default value if the operation failed.
    /// </summary>
    public T Value { get; }

    /// <summary>
    ///     Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value returned by the operation.</param>
    /// <returns>A <see cref="Result{T}" /> indicating success.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    /// <summary>
    ///     Creates a failed result with the specified error message and a default value.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A <see cref="Result{T}" /> indicating failure with the default value for type <typeparamref name="T" />.</returns>
    public new static Result<T> Failure(string error)
    {
        return new Result<T>(default!, error);
    }
}