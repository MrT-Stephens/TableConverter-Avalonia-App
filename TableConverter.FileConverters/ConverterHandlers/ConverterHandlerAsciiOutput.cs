using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerAsciiOutput : ConverterHandlerOutputAbstract<ConverterHandlerAsciiOutputOptions>
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

        var tableCharacterConfig = Options!.TableTypes[Options!.SelectedTableType];

        var comment = Options!.SelectedCommentType == Options!.CommentTypes.First().Key
            ? ""
            : $"{Options!.CommentTypes[Options!.SelectedCommentType]}    ";

        // A column is as wide as its widest cell, so every row has to be seen before the header can be
        // drawn. The rows are gathered here, then written out as they are drawn, so the whole table is
        // never held as one string.
        var rows = new List<string[]>();

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            rows.Add(row);

        // Calculates the max text character widths of every column.
        var maxColumnWidths = new long[headers.Count];

        for (var i = 0; i < headers.Count; i++) maxColumnWidths[i] = headers[i].Length + 2;

        foreach (var row in rows)
            for (var i = 0; i < row.Length; i++)
                maxColumnWidths[i] = Math.Max(maxColumnWidths[i], row[i].LongCount() + 2);

        // Draws the table header.
        writer.Write(comment +
                     DrawSeparator(maxColumnWidths,
                         tableCharacterConfig.HeaderTopLeft,
                         tableCharacterConfig.HeaderTopRight,
                         tableCharacterConfig.TopIntersection,
                         tableCharacterConfig.Horizontal) + Environment.NewLine);

        writer.Write(comment +
                     DrawDataRow(headers, maxColumnWidths,
                         Options!.SelectedTextAlignment,
                         tableCharacterConfig.Vertical,
                         tableCharacterConfig.Vertical,
                         tableCharacterConfig.Vertical) + Environment.NewLine);

        writer.Write(comment +
                     DrawSeparator(maxColumnWidths,
                         tableCharacterConfig.LeftIntersection,
                         tableCharacterConfig.RightIntersection,
                         tableCharacterConfig.MiddleIntersection,
                         tableCharacterConfig.Horizontal) + Environment.NewLine);

        // Draws the table rows.
        for (var i = 0; i < rows.Count; i++)
        {
            writer.Write(comment +
                         DrawDataRow(rows[i], maxColumnWidths,
                             Options!.SelectedTextAlignment,
                             tableCharacterConfig.Vertical,
                             tableCharacterConfig.Vertical,
                             tableCharacterConfig.Vertical) + Environment.NewLine);

            if (i < rows.Count - 1 && Options!.ForceRowSeparators)
                writer.Write(comment +
                             DrawSeparator(maxColumnWidths,
                                 tableCharacterConfig.LeftIntersection,
                                 tableCharacterConfig.RightIntersection,
                                 tableCharacterConfig.MiddleIntersection,
                                 tableCharacterConfig.Horizontal) + Environment.NewLine);
            else if (i == rows.Count - 1)
                writer.Write(comment +
                             DrawSeparator(maxColumnWidths,
                                 tableCharacterConfig.BottomLeft,
                                 tableCharacterConfig.BottomRight,
                                 tableCharacterConfig.BottomIntersection,
                                 tableCharacterConfig.Horizontal) + Environment.NewLine);
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
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

    private static string DrawDataRow(IReadOnlyList<string> row, long[] columnWidths, TextAlignment textAlignment,
        char leftChar, char rightChar, char intersectionChar)
    {
        var dataRow = new StringBuilder();

        dataRow.Append(leftChar);

        for (var i = 0; i < row.Count; i++)
        {
            dataRow.Append(ConverterHandlerUtilities.AlignText(row[i], textAlignment, (int)columnWidths[i], ' '));

            dataRow.Append(i == row.Count - 1 ? rightChar : intersectionChar);
        }

        return dataRow.ToString();
    }
}