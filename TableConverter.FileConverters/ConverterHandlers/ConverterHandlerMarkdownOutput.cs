using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerMarkdownOutput : ConverterHandlerOutputAbstract<ConverterHandlerMarkdownOutputOptions>
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

        var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

        var boldColumnNames = Options!.BoldColumnNames;
        var boldFirstColumn = Options!.BoldFirstColumn;

        // Build bolded copies: the caller's table data must never be mutated.
        var displayHeaders = new string[headers.Count];

        for (var i = 0; i < headers.Count; i++)
            displayHeaders[i] = boldColumnNames || (i == 0 && boldFirstColumn)
                ? $"**{headers[i]}**"
                : headers[i];

        // A column is as wide as its widest cell, so every row has to be seen before the header can be
        // drawn. The rows are gathered bolded, then each line is written as it is drawn.
        var displayRows = new List<string[]>();

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            var displayRow = new string[row.Length];

            for (var i = 0; i < row.Length; i++)
                displayRow[i] = i == 0 && boldFirstColumn
                    ? $"**{row[i]}**"
                    : row[i];

            displayRows.Add(displayRow);
        }

        // Calculates the max text character widths of every column, including the bold markers.
        var maxColumnWidths = new long[displayHeaders.Length];

        for (var i = 0; i < displayHeaders.Length; i++)
            maxColumnWidths[i] = displayHeaders[i].Length + 2;

        foreach (var row in displayRows)
            for (var i = 0; i < displayHeaders.Length && i < row.Length; i++)
                maxColumnWidths[i] = Math.Max(maxColumnWidths[i], row[i].Length + 2);

        // Draw the table.
        switch (Options!.SelectedTableType)
        {
            case ConverterHandlerMarkdownOutputOptions.TableStyles.Normal:
            {
                writer.Write("|" + DrawDataRow(displayHeaders, maxColumnWidths,
                    Options!.SelectedTextAlignment, '|') + "|" + Environment.NewLine);
                writer.Write("|" + DrawSeparator(maxColumnWidths, '|', '-') + "|" + Environment.NewLine);

                foreach (var row in displayRows)
                    writer.Write("|" + DrawDataRow(row, maxColumnWidths,
                        Options!.SelectedTextAlignment, '|') + "|" + Environment.NewLine);

                break;
            }
            case ConverterHandlerMarkdownOutputOptions.TableStyles.Simple:
            {
                writer.Write(DrawDataRow(displayHeaders, maxColumnWidths,
                    Options!.SelectedTextAlignment, '|') + Environment.NewLine);
                writer.Write(DrawSeparator(maxColumnWidths, '|', '-') + Environment.NewLine);

                foreach (var row in displayRows)
                    writer.Write(DrawDataRow(row, maxColumnWidths,
                        Options!.SelectedTextAlignment, '|') + Environment.NewLine);

                break;
            }
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static string DrawSeparator(long[] columnWidths, char intersectionChar, char fillChar)
    {
        var separator = new StringBuilder();

        for (long i = 0; i < columnWidths.LongLength; i++)
        {
            separator.Append(new string(fillChar, (int)columnWidths[i]));

            separator.Append(i == columnWidths.LongLength - 1 ? "" : intersectionChar);
        }

        return separator.ToString();
    }

    private static string DrawDataRow(IReadOnlyList<string> row, long[] columnWidths, TextAlignment textAlignment,
        char intersectionChar)
    {
        var dataRow = new StringBuilder();

        // Iterate the column widths so ragged rows (fewer or more cells than headers) cannot throw.
        for (var i = 0; i < columnWidths.LongLength; i++)
        {
            var cell = i < row.Count ? row[i] : string.Empty;

            dataRow.Append(ConverterHandlerUtilities.AlignText(cell, textAlignment, (int)columnWidths[i], ' '));

            dataRow.Append(i == columnWidths.LongLength - 1 ? "" : intersectionChar);
        }

        return dataRow.ToString();
    }
}