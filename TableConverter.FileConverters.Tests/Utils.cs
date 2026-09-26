using System.Text;
using TableConverter.FileConverters.Interfaces;
using TableConverter.FileConverters.Tests.TestBase;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Tests;

/// <summary>
///     Helper class for test data and methods.
/// </summary>
public static class Utils
{
    public static readonly TableSnapshot TestTable = new(
        ["FIRST_NAME", "LAST_NAME", "GENDER", "COUNTRY_CODE"],
        [
            ["Luxeena", "Binoy", "F", "GB"],
            ["Lisa", "Allen", "F", "GB"],
            ["Richard", "Wood", "M", "GB"],
            ["Luke", "Murphy", "M", "GB"],
            ["Adrian", "Heacock", "M", "GB"],
            ["Elvinas", "Palubinskas", "M", "GB"],
            ["Sian", "Turner", "F", "GB"],
            ["Potar", "Potts", "M", "GB"],
            ["Janis", "Chrisp", "F", "GB"],
            ["Sarah", "Proffitt", "F", "GB"],
            ["Calissa", "Noonan", "F", "GB"],
            ["Andrew", "Connors", "M", "GB"],
            ["Siann", "Tynan", "F", "GB"],
            ["Olivia", "Parry", "F", "GB"]
        ]);

    /// <summary>
    ///     Runs an output handler over a table through its single entry point and returns the text it
    ///     wrote, so a test can assert on the output without managing a stream itself.
    /// </summary>
    /// <param name="handler">The handler to run.</param>
    /// <param name="headers">The headers of the table to convert.</param>
    /// <param name="rows">The rows of the table to convert.</param>
    public static async Task<Result<string>> ConvertToTextAsync(
        IConverterHandlerOutput handler,
        string[] headers,
        string[][] rows)
    {
        using var stream = new MemoryStream();

        var result = await handler.ConvertToStreamAsync(stream, new TableSnapshot(headers, rows));

        return result.IsSuccess
            ? Result<string>.Success(Encoding.UTF8.GetString(stream.ToArray()))
            : Result<string>.Failure(result.Error ?? string.Empty);
    }

    /// <summary>
    ///     Runs an output handler over a table through its single entry point, writing into
    ///     <paramref name="stream" />, for a format a test has to read back from the bytes rather than
    ///     from text.
    /// </summary>
    /// <param name="handler">The handler to run.</param>
    /// <param name="headers">The headers of the table to convert.</param>
    /// <param name="rows">The rows of the table to convert.</param>
    /// <param name="stream">The stream the converted table is written to.</param>
    public static Task<Result> ConvertToStreamAsync(
        IConverterHandlerOutput handler,
        string[] headers,
        string[][] rows,
        Stream stream)
    {
        return handler.ConvertToStreamAsync(stream, new TableSnapshot(headers, rows));
    }
}