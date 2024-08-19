namespace TableConverter.FileConverters.Utilities
{
    public enum TextAlignment
    {
        Left = 0, Center = 1, Right = 2,
    }

    public static class ConverterHandlerUtilities
    {
        public static string AlignText(string text, TextAlignment text_alignment, int amount, char padding_character)
        {
            switch (text_alignment)
            {
                case TextAlignment.Left:
                    text = text.PadRight(amount, padding_character);
                    break;
                case TextAlignment.Center:
                    int leftPadding = (amount - text.Length) / 2;
                    int rightPadding = amount - text.Length - leftPadding;
                    text = text.PadLeft(text.Length + leftPadding, padding_character);
                    text = text.PadRight(text.Length + rightPadding, padding_character);
                    break;
                case TextAlignment.Right:
                    text = text.PadLeft(amount, padding_character);
                    break;
                default:
                    text = text.PadRight(amount, padding_character);
                    break;
            }

            return text;
        }
    }
}
