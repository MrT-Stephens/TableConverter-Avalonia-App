using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Interfaces;

/// <summary>
///     Interface for handling output operations for file conversion.
/// </summary>
public interface IConverterHandlerOutput
{
    /// <summary>
    ///     Gets or sets the dynamic options for output conversion.
    /// </summary>
    public dynamic? Options { get; set; }

    /// <summary>
    ///     Converts the given headers and rows into a formatted string.
    /// </summary>
    /// <param name="headers">The headers for the table data.</param>
    /// <param name="rows">The rows containing the table data.</param>
    /// <returns>A result containing the converted table data as a string.</returns>
    public Result<string> Convert(string[] headers, string[][] rows);

    /// <summary>
    ///     Asynchronously converts the given headers and rows into a formatted string.
    /// </summary>
    /// <param name="headers">The headers for the table data.</param>
    /// <param name="rows">The rows containing the table data.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, containing a result with the converted table data as a
    ///     string.
    /// </returns>
    public Task<Result<string>> ConvertAsync(string[] headers, string[][] rows);

    /// <summary>
    ///     Saves the converted table data to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to save the converted data to.</param>
    /// <param name="buffer">The buffer containing the data to be saved.</param>
    /// <returns>A result indicating the success or failure of the operation.</returns>
    public Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer);

    /// <summary>
    ///     Asynchronously saves the converted table data to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to save the converted data to.</param>
    /// <param name="buffer">The buffer containing the data to be saved.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, containing a result indicating the success or failure of
    ///     the operation.
    /// </returns>
    public Task<Result> SaveFileAsync(Stream? stream, ReadOnlyMemory<byte> buffer);
}