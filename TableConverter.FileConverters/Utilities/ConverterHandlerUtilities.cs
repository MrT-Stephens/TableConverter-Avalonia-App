namespace TableConverter.FileConverters.Utilities;

public enum TextAlignment
{
    Left = 0,
    Center = 1,
    Right = 2
}

public static class ConverterHandlerUtilities
{
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