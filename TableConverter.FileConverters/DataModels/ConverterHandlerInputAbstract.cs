using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.DataModels;

/// <summary>
///     An abstract base class for handling input conversion, with generic support for options of type
///     <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type of the options that extend <see cref="ConverterHandlerBaseOptions" />.</typeparam>
public abstract class ConverterHandlerInputAbstract<T> : IConverterHandlerInput
    where T : ConverterHandlerBaseOptions, new()
{
    /// <summary>
    ///     Gets or sets the options used for input conversion.
    /// </summary>
    public T? Options { get; set; } = typeof(T) == typeof(ConverterHandlerBaseOptions) ? null : new T();

    /// <summary>
    ///     Gets or sets the dynamic options for input conversion. Implements <see cref="IConverterHandlerInput.Options" />.
    /// </summary>
    dynamic? IConverterHandlerInput.Options
    {
        get => Options;
        set => Options = value;
    }

    /// <summary>
    ///     Reads the content of a file stream and returns it as a string.
    ///     Retries up to 3 times if a timeout error occurs.
    /// </summary>
    /// <param name="stream">The file stream to read from.</param>
    /// <returns>A result containing the file content as a string, or a failure message if an error occurs.</returns>
    public virtual Result<string> ReadFile(Stream? stream)
    {
        // Ensure the stream is not null before attempting to read
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        const int maxRetries = 3; // Maximum number of retry attempts
        var attempts = 0;

        // Create a StreamReader to read the file stream
        using var reader = new StreamReader(stream);

        // Retry the file reading process up to maxRetries in case of timeouts
        while (attempts < maxRetries)
            try
            {
                var text = reader.ReadToEnd();

                // If the file content is empty or whitespace, return a failure result
                if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                    return Result<string>.Failure("File is empty");

                // Return a successful result with the file content
                return Result<string>.Success(text);
            }
            catch (IOException exception) when (exception.Message.Contains("timed out"))
            {
                // If a timeout occurs, increment attempts and wait for a short time before retrying
                attempts++;
                Thread.Sleep(2000); // Wait before retrying
            }
            catch (Exception exception)
            {
                // Return a failure result with the exception message if any other error occurs
                return Result<string>.Failure(exception.Message);
            }

        // Return a failure result if reading the file fails after maxRetries
        return Result<string>.Failure("Failed to read file");
    }

    /// <summary>
    ///     Asynchronously reads the content of a file stream and returns it as a string.
    /// </summary>
    /// <param name="stream">The file stream to read from.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result with the file content as a string.</returns>
    public async Task<Result<string>> ReadFileAsync(Stream? stream)
    {
        return await Task.Run(() => ReadFile(stream));
    }

    /// <summary>
    ///     Reads the provided text and converts it into a <see cref="TableData" /> instance.
    ///     This method must be implemented by a derived class.
    /// </summary>
    /// <param name="text">The text to be converted into a table data.</param>
    /// <returns>A result containing the parsed <see cref="TableData" /> instance.</returns>
    public abstract Result<TableData> ReadText(string text);

    /// <summary>
    ///     Asynchronously reads the provided text and converts it into a <see cref="TableData" /> instance.
    /// </summary>
    /// <param name="text">The text to be converted into a table data.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, containing the result with the parsed
    ///     <see cref="TableData" /> instance.
    /// </returns>
    public async Task<Result<TableData>> ReadTextAsync(string text)
    {
        return await Task.Run(() => ReadText(text));
    }
}