using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerMarkdownOutput : ConverterHandlerOutputAbstract<ConverterHandlerMarkdownOutputOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        var asciiOutput = new StringBuilder();

        // Calculates the max text character widths of every column.
        var maxColumnWidths = new long[headers.Length];

        for (var i = 0; i < headers.Length; i++)
            if ((i == 0 && Options!.BoldFirstColumn) || Options!.BoldColumnNames)
                maxColumnWidths[i] = headers[i].Length + 6;
            else
                maxColumnWidths[i] = headers[i].Length + 2;

        foreach (var row in rows)
            for (var i = 0; i < headers.Length; i++)
                if (i == 0 && Options!.BoldFirstColumn)
                    maxColumnWidths[i] = Math.Max(maxColumnWidths[i], row[i].Length + 6);
                else
                    maxColumnWidths[i] = Math.Max(maxColumnWidths[i], row[i].Length + 2);

        // Bold the data if the user has selected to bold the first column.
        if (Options!.BoldFirstColumn || Options!.BoldColumnNames) headers[0] = "**" + headers[0] + "**";

        if (Options!.BoldColumnNames)
            for (long i = 1; i < headers.LongLength; i++)
                headers[i] = "**" + headers[i] + "**";

        if (Options!.BoldFirstColumn)
            foreach (var row in rows)
                row[0] = "**" + row[0] + "**";

        // Draw the table.
        switch (Options!.SelectedTableType)
        {
            case "Markdown Table (Normal)":
            {
                asciiOutput.AppendLine("|" + DrawDataRow(headers, maxColumnWidths,
                    Options!.TextAlignment[Options!.SelectedTextAlignment], '|') + "|");
                asciiOutput.AppendLine("|" + DrawSeparator(maxColumnWidths, '|', '-') + "|");

                foreach (var row in rows)
                    asciiOutput.AppendLine("|" + DrawDataRow(row, maxColumnWidths,
                        Options!.TextAlignment[Options!.SelectedTextAlignment], '|') + "|");

                break;
            }
            case "Markdown Table (Simple)":
            {
                asciiOutput.AppendLine(DrawDataRow(headers, maxColumnWidths,
                    Options!.TextAlignment[Options!.SelectedTextAlignment], '|'));
                asciiOutput.AppendLine(DrawSeparator(maxColumnWidths, '|', '-'));

                foreach (var row in rows)
                    asciiOutput.AppendLine(DrawDataRow(row, maxColumnWidths,
                        Options!.TextAlignment[Options!.SelectedTextAlignment], '|'));

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

        for (long i = 0; i < row.LongLength; i++)
        {
            dataRow.Append(ConverterHandlerUtilities.AlignText(row[i], textAlignment, (int)columnWidths[i], ' '));

            dataRow.Append(i == row.LongLength - 1 ? "" : intersectionChar);
        }

        return dataRow.ToString();
    }
}