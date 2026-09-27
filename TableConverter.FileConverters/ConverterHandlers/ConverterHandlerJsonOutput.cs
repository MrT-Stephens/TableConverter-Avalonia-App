using System.Globalization;
using Newtonsoft.Json;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;
using TableConverter.Utilities.Models;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerJsonOutput : ConverterHandlerOutputAbstract<ConverterHandlerJsonOutputOptions>
{
    public override async Task<Result> ConvertToStreamAsync(
        Stream? stream,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(source);

        await using var writer = TableRowStream.CreateTextWriter(stream);

        try
        {
            var columns = await source.GetColumnsAsync(cancellationToken).ConfigureAwait(false);

            // The writer streams the document out as it is built, so the whole of it is never held as
            // one string. It is closed, not the caller's writer, which must stay open.
            using var jsonWriter = new JsonTextWriter(writer)
            {
                Formatting = Options!.MinifyJson ? Formatting.None : Formatting.Indented,
                CloseOutput = false
            };

            switch (Options!.SelectedJsonFormatType)
            {
                case ConverterHandlerJsonOutputOptions.JsonStyles.ArrayOfObjects:
                {
                    jsonWriter.WriteStartArray();

                    await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
                    {
                        jsonWriter.WriteStartObject();

                        for (var j = 0; j < columns.Count; j++)
                        {
                            jsonWriter.WritePropertyName(columns[j].Name.Replace(' ', '_'));
                            WriteValue(jsonWriter, columns[j], ConverterHandlerUtilities.GetCellValue(row, j));
                        }

                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteEndArray();
                    break;
                }
                case ConverterHandlerJsonOutputOptions.JsonStyles.TwoDimensionalArrays:
                {
                    jsonWriter.WriteStartArray();

                    jsonWriter.WriteStartArray();
                    foreach (var column in columns) jsonWriter.WriteValue(column.Name.Replace(' ', '_'));
                    jsonWriter.WriteEndArray();

                    await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
                    {
                        jsonWriter.WriteStartArray();

                        for (var j = 0; j < columns.Count; j++)
                        {
                            WriteValue(jsonWriter, columns[j], ConverterHandlerUtilities.GetCellValue(row, j));
                        }

                        jsonWriter.WriteEndArray();
                    }

                    jsonWriter.WriteEndArray();
                    break;
                }
                case ConverterHandlerJsonOutputOptions.JsonStyles.ColumnArrays:
                {
                    // Every row is needed for each column, so the rows are gathered once rather than
                    // being read back from the source once per column.
                    var rows = new List<string[]>();

                    await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
                        rows.Add(row);

                    jsonWriter.WriteStartArray();

                    for (var j = 0; j < columns.Count; j++)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName(columns[j].Name.Replace(' ', '_'));
                        jsonWriter.WriteStartArray();

                        foreach (var row in rows)
                            WriteValue(jsonWriter, columns[j], ConverterHandlerUtilities.GetCellValue(row, j));

                        jsonWriter.WriteEndArray();
                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteEndArray();
                    break;
                }
                case ConverterHandlerJsonOutputOptions.JsonStyles.KeyedArrays:
                {
                    var rows = new List<string[]>();

                    await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
                        rows.Add(row);

                    jsonWriter.WriteStartArray();

                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("0");
                    jsonWriter.WriteStartArray();
                    foreach (var column in columns) jsonWriter.WriteValue(column.Name.Replace(' ', '_'));
                    jsonWriter.WriteEndArray();
                    jsonWriter.WriteEndObject();

                    for (var i = 0; i < rows.Count; i++)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName((i + 1).ToString());
                        jsonWriter.WriteStartArray();

                        for (var j = 0; j < columns.Count; j++)
                        {
                            WriteValue(jsonWriter, columns[j], ConverterHandlerUtilities.GetCellValue(rows[i], j));
                        }

                        jsonWriter.WriteEndArray();
                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteEndArray();
                    break;
                }
                default:
                    return Result.Failure("Unsupported json format");
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    /// <summary>
    ///     Writes <paramref name="value" /> as the kind of value the column holds, so a number is written
    ///     as a JSON number and a boolean as a JSON boolean rather than both as a string.
    /// </summary>
    /// <remarks>
    ///     A date is written as its plain, sortable text, because JSON has no date of its own. A value
    ///     that does not read as its column's type, and a column whose type is not known, are written as
    ///     the text they already are, so nothing is lost.
    /// </remarks>
    private static void WriteValue(JsonWriter writer, TableColumn column, string? value)
    {
        if (column.TryReadValue(value, out var typed))
        {
            switch (typed)
            {
                case long integer:
                    writer.WriteValue(integer);
                    return;

                case decimal number:
                    writer.WriteValue(number);
                    return;

                case bool flag:
                    writer.WriteValue(flag);
                    return;

                case DateOnly date:
                    writer.WriteValue(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    return;

                case DateTime moment:
                    writer.WriteValue(moment);
                    return;
            }
        }

        writer.WriteValue(value ?? string.Empty);
    }
}