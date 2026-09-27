using System.Globalization;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;
using TableConverter.Utilities.Models;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerSQLOutput : ConverterHandlerOutputAbstract<ConverterHandlerSQLOutputOptions>
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

        var columns = await source.GetColumnsAsync(cancellationToken).ConfigureAwait(false);

        var quoteType = Options!.QuoteTypes[Options!.SelectedQuoteType];
        var closingQuote = quoteType == "[" ? "]" : quoteType;

        var headersText = string.Join(", ", columns.Select(column =>
            $"{quoteType}{column.Name.Replace(' ', '_')}{closingQuote}"));

        if (Options!.InsertMultiRowsAtOnce)
        {
            // The closing ';' and newline belong after the last row, which is only known once the rows
            // run out, so they are written after the loop rather than with the last row.
            var wroteAny = false;

            await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                var rowText = BuildRowText(row, columns);

                if (!wroteAny)
                {
                    writer.Write("INSERT INTO " +
                                 $"{quoteType}" +
                                 $"{Options!.TableName.Replace(' ', '_')}" +
                                 $"{closingQuote} " +
                                 $"({headersText}) VALUES{Environment.NewLine} ({rowText})");

                    wroteAny = true;
                }
                else
                {
                    writer.Write($",{Environment.NewLine} ({rowText})");
                }
            }

            if (wroteAny)
            {
                writer.Write($";{Environment.NewLine}");
            }
        }
        else
        {
            await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                var rowText = BuildRowText(row, columns);

                writer.Write($"INSERT INTO " +
                             $"{quoteType}" +
                             $"{Options!.TableName.Replace(' ', '_')}" +
                             $"{closingQuote} " +
                             $"({headersText}) VALUES ({rowText});" + Environment.NewLine);
            }
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    /// <summary>
    ///     Builds the parenthesised list of values for one row.
    /// </summary>
    /// <remarks>
    ///     A number is written as the bare number rather than as quoted text, so the value lands in the
    ///     target column as the number it is. Everything else keeps the quoting it has always had, which
    ///     is what every dialect accepts whatever a column is declared as.
    /// </remarks>
    private static string BuildRowText(string[] row, IReadOnlyList<TableColumn> columns)
    {
        // Iterate the columns so a ragged row is padded rather than producing fewer values than columns.
        return string.Join(", ", columns.Select((column, index) => BuildCellText(column, row, index)));
    }

    private static string BuildCellText(TableColumn column, string[] row, int columnIndex)
    {
        var value = ConverterHandlerUtilities.GetCellValue(row, columnIndex);

        if (column.TryReadValue(value, out var typed))
        {
            switch (typed)
            {
                case long integer:
                    return integer.ToString(CultureInfo.InvariantCulture);

                case decimal number:
                    return number.ToString(CultureInfo.InvariantCulture);
            }
        }

        return $"\'{value.Replace("\'", "\'\'")}\'";
    }
}