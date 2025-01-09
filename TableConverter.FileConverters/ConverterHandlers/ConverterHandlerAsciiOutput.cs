using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerAsciiOutput : ConverterHandlerOutputAbstract<ConverterHandlerAsciiOutputOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        var asciiOutput = new StringBuilder();

        var tableCharacterConfig = Options!.TableTypes[Options!.SelectedTableType];

        // Calculates the max text character widths of every column.
        var maxColumnWidths = new long[headers.LongLength];

        for (long i = 0; i < headers.LongLength; i++) maxColumnWidths[i] = headers[i].Length + 2;

        foreach (var row in rows)
            for (long i = 0; i < row.LongLength; i++)
                maxColumnWidths[i] = Math.Max(maxColumnWidths[i], row[i].LongCount() + 2);

        var comment = Options!.SelectedCommentType == Options!.CommentTypes.First().Key
            ? ""
            : $"{Options!.CommentTypes[Options!.SelectedCommentType]}    ";

        // Draws the table header.
        asciiOutput.AppendLine(comment +
                               DrawSeparator(maxColumnWidths,
                                   tableCharacterConfig.HeaderTopLeft,
                                   tableCharacterConfig.HeaderTopRight,
                                   tableCharacterConfig.TopIntersection,
                                   tableCharacterConfig.Horizontal));

        asciiOutput.AppendLine(comment +
                               DrawDataRow(headers, maxColumnWidths,
                                   Options!.TextAlignment[Options!.SelectedTextAlignment],
                                   tableCharacterConfig.Vertical,
                                   tableCharacterConfig.Vertical,
                                   tableCharacterConfig.Vertical));

        asciiOutput.AppendLine(comment +
                               DrawSeparator(maxColumnWidths,
                                   tableCharacterConfig.LeftIntersection,
                                   tableCharacterConfig.RightIntersection,
                                   tableCharacterConfig.MiddleIntersection,
                                   tableCharacterConfig.Horizontal));

        // Draws the table rows.
        for (long i = 0; i < rows.LongLength; i++)
        {
            asciiOutput.AppendLine(comment +
                                   DrawDataRow(rows[i], maxColumnWidths,
                                       Options!.TextAlignment[Options!.SelectedTextAlignment],
                                       tableCharacterConfig.Vertical,
                                       tableCharacterConfig.Vertical,
                                       tableCharacterConfig.Vertical));

            if (i < rows.LongLength - 1 && Options!.ForceRowSeparators)
                asciiOutput.AppendLine(comment +
                                       DrawSeparator(maxColumnWidths,
                                           tableCharacterConfig.LeftIntersection,
                                           tableCharacterConfig.RightIntersection,
                                           tableCharacterConfig.MiddleIntersection,
                                           tableCharacterConfig.Horizontal));
            else if (i == rows.LongLength - 1)
                asciiOutput.AppendLine(comment +
                                       DrawSeparator(maxColumnWidths,
                                           tableCharacterConfig.BottomLeft,
                                           tableCharacterConfig.BottomRight,
                                           tableCharacterConfig.BottomIntersection,
                                           tableCharacterConfig.Horizontal));
        }

        return Result<string>.Success(asciiOutput.ToString());
    }

    private static string DrawSeparator(long[] columnWidths, char leftChar, char rightChar, char intersectionChar,
        char fillChar)
    {
        var separator = new StringBuilder();

        separator.Append(leftChar);

        for (long i = 0; i < columnWidths.LongLength; i++)
        {
            separator.Append(new string(fillChar, (int)columnWidths[i]));

            separator.Append(i == columnWidths.LongLength - 1 ? rightChar : intersectionChar);
        }

        return separator.ToString();
    }

    private static string DrawDataRow(string[] row, long[] columnWidths, TextAlignment textAlignment, char leftChar,
        char rightChar, char intersectionChar)
    {
        var dataRow = new StringBuilder();

        dataRow.Append(leftChar);

        for (long i = 0; i < row.LongLength; i++)
        {
            dataRow.Append(ConverterHandlerUtilities.AlignText(row[i], textAlignment, (int)columnWidths[i], ' '));

            dataRow.Append(i == row.LongLength - 1 ? rightChar : intersectionChar);
        }

        return dataRow.ToString();
    }
}