using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.DataModels;

/// <summary>
///     An abstract base class for handling output conversion, with generic support for options of type
///     <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type of the options that extend <see cref="ConverterHandlerBaseOptions" />.</typeparam>
public abstract class ConverterHandlerOutputAbstract<T> : IConverterHandlerOutput
    where T : ConverterHandlerBaseOptions, new()
{
    /// <summary>
    ///     Gets or sets the options used for output conversion.
    /// </summary>
    public T? Options { get; set; } = typeof(T) == typeof(ConverterHandlerBaseOptions) ? null : new T();

    /// <summary>
    ///     Gets or sets the dynamic options for output conversion. Implements <see cref="IConverterHandlerOutput.Options" />.
    /// </summary>
    dynamic? IConverterHandlerOutput.Options
    {
        get => Options;
        set => Options = value;
    }

    /// <summary>
    ///     Converts the provided headers and rows into a formatted string.
    /// </summary>
    /// <param name="headers">The headers of the table data.</param>
    /// <param name="rows">The rows containing the table data.</param>
    /// <returns>A result containing the converted table data as a string.</returns>
    public abstract Result<string> Convert(string[] headers, string[][] rows);

    /// <summary>
    ///     Asynchronously converts the provided headers and rows into a formatted string.
    /// </summary>
    /// <param name="headers">The headers of the table data.</param>
    /// <param name="rows">The rows containing the table data.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, containing the result with the converted table data as a
    ///     string.
    /// </returns>
    public async Task<Result<string>> ConvertAsync(string[] headers, string[][] rows)
    {
        return await Task.Run(() => Convert(headers, rows));
    }

    /// <summary>
    ///     Saves the converted data to a stream.
    /// </summary>
    /// <param name="stream">The stream to which the data will be written.</param>
    /// <param name="buffer">The buffer containing the data to be saved.</param>
    /// <returns>A result indicating the success or failure of the save operation.</returns>
    public virtual Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
    {
        // Ensure the stream is not null before attempting to write
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        try
        {
            // Write the data to the stream
            stream.Write(buffer.Span);

            // Close the stream after writing
            stream.Close();
        }
        catch (Exception ex)
        {
            // Return a failure result if an exception occurs during writing
            return Result.Failure(ex.Message);
        }

        // Return a success result if the file was saved without errors
        return Result.Success();
    }

    /// <summary>
    ///     Asynchronously saves the converted data to a stream.
    /// </summary>
    /// <param name="output">The stream to which the data will be written.</param>
    /// <param name="buffer">The buffer containing the data to be saved.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the save operation.</returns>
    public async Task<Result> SaveFileAsync(Stream? output, ReadOnlyMemory<byte> buffer)
    {
        return await Task.Run(() => SaveFile(output, buffer));
    }
}