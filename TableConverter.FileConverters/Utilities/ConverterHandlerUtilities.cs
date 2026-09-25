namespace TableConverter.FileConverters.Utilities;

public enum TextAlignment
{
    Left = 0,
    Center = 1,
    Right = 2
}

public static class ConverterHandlerUtilities
{
    /// <summary>
    ///     Gets the value at <paramref name="columnIndex" /> in <paramref name="row" />.
    /// </summary>
    /// <remarks>
    ///     Tables loaded from files can be ragged (a row may have fewer cells than there are headers),
    ///     so callers that iterate the headers must use this instead of indexing the row directly.
    /// </remarks>
    /// <returns>The cell value, or an empty string when the row does not contain that column.</returns>
    public static string GetCellValue(string[]? row, long columnIndex)
    {
        if (row is null || columnIndex < 0 || columnIndex >= row.LongLength)
        {
            return string.Empty;
        }

        return row[columnIndex];
    }

    public static string AlignText(string text, TextAlignment textAlignment, int amount, char paddingCharacter)
    {
        switch (textAlignment)
        {
            case TextAlignment.Left:
                text = text.PadRight(amount, paddingCharacter);
                break;
            case TextAlignment.Center:
                var leftPadding = (amount - text.Length) / 2;
                var rightPadding = amount - text.Length - leftPadding;
                text = text.PadLeft(text.Length + leftPadding, paddingCharacter);
                text = text.PadRight(text.Length + rightPadding, paddingCharacter);
                break;
            case TextAlignment.Right:
                text = text.PadLeft(amount, paddingCharacter);
                break;
            default:
                text = text.PadRight(amount, paddingCharacter);
                break;
        }

        return text;
    }
}