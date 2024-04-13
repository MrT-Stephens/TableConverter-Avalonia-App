using Avalonia.Layout;

namespace TableConverter.Services
{
    internal static class ConverterHandlerMiscItems
    {
        public static string AlignText(string text, HorizontalAlignment text_alignment, int amount, char padding_character)
        {
            switch (text_alignment)
            {
                case HorizontalAlignment.Left:
                    text = text.PadRight(amount, padding_character);
                    break;
                case HorizontalAlignment.Center:
                    int leftPadding = (amount - text.Length) / 2;
                    int rightPadding = (amount - text.Length) - leftPadding;
                    text = text.PadLeft(text.Length + leftPadding, padding_character);
                    text = text.PadRight(text.Length + rightPadding, padding_character);
                    break;
                case HorizontalAlignment.Right:
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
