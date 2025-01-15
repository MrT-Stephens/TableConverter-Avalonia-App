using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Interfaces;

/// <summary>
///     Interface for handling input operations for file conversion.
/// </summary>
public interface IConverterHandlerInput
{
    /// <summary>
    ///     Gets or sets the dynamic options for input conversion.
    /// </summary>
    public dynamic? Options { get; set; }

    /// <summary>
    ///     Reads and converts a file from a provided stream.
    /// </summary>
    /// <param name="stream">The stream containing file data.</param>
    /// <returns>A result containing the file contents as a string.</returns>
    public Result<string> ReadFile(Stream? stream);

    /// <summary>
    ///     Asynchronously reads and converts a file from a provided stream.
    /// </summary>
    /// <param name="stream">The stream containing file data.</param>
    /// <returns>A task that represents the asynchronous operation, containing a result with the file contents as a string.</returns>
    public Task<Result<string>> ReadFileAsync(Stream? stream);

    /// <summary>
    ///     Reads and converts raw text data to a <see cref="TableData" /> object.
    /// </summary>
    /// <param name="text">The text to be converted.</param>
    /// <returns>A result containing the parsed table data.</returns>
    public Result<TableData> ReadText(string text);

    /// <summary>
    ///     Asynchronously reads and converts raw text data to a <see cref="TableData" /> object.
    /// </summary>
    /// <param name="text">The text to be converted.</param>
    /// <returns>A task that represents the asynchronous operation, containing a result with the parsed table data.</returns>
    public Task<Result<TableData>> ReadTextAsync(string text);
}