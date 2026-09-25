using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerMarkdownOutput : ConverterHandlerOutputAbstract<ConverterHandlerMarkdownOutputOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        var boldColumnNames = Options!.BoldColumnNames;
        var boldFirstColumn = Options!.BoldFirstColumn;

        // Build bolded copies: the caller's table data must never be mutated.
        var displayHeaders = new string[headers.Length];

        for (var i = 0; i < headers.Length; i++)
            displayHeaders[i] = boldColumnNames || (i == 0 && boldFirstColumn)
                ? $"**{headers[i]}**"
                : headers[i];

        var displayRows = new string[rows.Length][];

        for (var r = 0; r < rows.Length; r++)
        {
            var row = rows[r];
            var displayRow = new string[row.Length];

            for (var i = 0; i < row.Length; i++)
                displayRow[i] = i == 0 && boldFirstColumn
                    ? $"**{row[i]}**"
                    : row[i];

            displayRows[r] = displayRow;
        }

        // Calculates the max text character widths of every column, including the bold markers.
        var maxColumnWidths = new long[displayHeaders.Length];

        for (var i = 0; i < displayHeaders.Length; i++)
            maxColumnWidths[i] = displayHeaders[i].Length + 2;

        foreach (var row in displayRows)
            for (var i = 0; i < displayHeaders.Length && i < row.Length; i++)
                maxColumnWidths[i] = Math.Max(maxColumnWidths[i], row[i].Length + 2);

        // Draw the table.
        var asciiOutput = new StringBuilder();

        switch (Options!.SelectedTableType)
        {
            case ConverterHandlerMarkdownOutputOptions.TableStyles.Normal:
            {
                asciiOutput.AppendLine("|" + DrawDataRow(displayHeaders, maxColumnWidths,
                    Options!.SelectedTextAlignment, '|') + "|");
                asciiOutput.AppendLine("|" + DrawSeparator(maxColumnWidths, '|', '-') + "|");

                foreach (var row in displayRows)
                    asciiOutput.AppendLine("|" + DrawDataRow(row, maxColumnWidths,
                        Options!.SelectedTextAlignment, '|') + "|");

                break;
            }
            case ConverterHandlerMarkdownOutputOptions.TableStyles.Simple:
            {
                asciiOutput.AppendLine(DrawDataRow(displayHeaders, maxColumnWidths,
                    Options!.SelectedTextAlignment, '|'));
                asciiOutput.AppendLine(DrawSeparator(maxColumnWidths, '|', '-'));

                foreach (var row in displayRows)
                    asciiOutput.AppendLine(DrawDataRow(row, maxColumnWidths,
                        Options!.SelectedTextAlignment, '|'));

                break;
            }
        }

        return Result<string>.Success(asciiOutput.ToString());
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

    private static string DrawDataRow(string[] row, long[] columnWidths, TextAlignment textAlignment,
        char intersectionChar)
    {
        var dataRow = new StringBuilder();

        // Iterate the column widths so ragged rows (fewer or more cells than headers) cannot throw.
        for (long i = 0; i < columnWidths.LongLength; i++)
        {
            var cell = i < row.LongLength ? row[i] : string.Empty;

            dataRow.Append(ConverterHandlerUtilities.AlignText(cell, textAlignment, (int)columnWidths[i], ' '));

            dataRow.Append(i == columnWidths.LongLength - 1 ? "" : intersectionChar);
        }

        return dataRow.ToString();
    }
}