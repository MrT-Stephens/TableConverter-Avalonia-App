using Newtonsoft.Json;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

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
            var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

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

                        for (var j = 0; j < headers.Count; j++)
                        {
                            jsonWriter.WritePropertyName(headers[j].Replace(' ', '_'));
                            jsonWriter.WriteValue(ConverterHandlerUtilities.GetCellValue(row, j));
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
                    foreach (var header in headers) jsonWriter.WriteValue(header.Replace(' ', '_'));
                    jsonWriter.WriteEndArray();

                    await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
                    {
                        jsonWriter.WriteStartArray();
                        foreach (var cell in row) jsonWriter.WriteValue(cell);
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

                    for (var j = 0; j < headers.Count; j++)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName(headers[j].Replace(' ', '_'));
                        jsonWriter.WriteStartArray();

                        foreach (var row in rows)
                            jsonWriter.WriteValue(ConverterHandlerUtilities.GetCellValue(row, j));

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
                    foreach (var header in headers) jsonWriter.WriteValue(header.Replace(' ', '_'));
                    jsonWriter.WriteEndArray();
                    jsonWriter.WriteEndObject();

                    for (var i = 0; i < rows.Count; i++)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName((i + 1).ToString());
                        jsonWriter.WriteStartArray();
                        foreach (var cell in rows[i]) jsonWriter.WriteValue(cell);
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
}