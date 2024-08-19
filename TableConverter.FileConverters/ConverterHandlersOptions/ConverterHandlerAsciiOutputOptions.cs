using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public record TableCharacterConfig(
        char HeaderTopLeft,
        char HeaderTopRight,
        char HeaderBottomLeft,
        char HeaderBottomRight,
        char BottomLeft,
        char BottomRight,
        char Horizontal,
        char Vertical,
        char MiddleIntersection,
        char TopIntersection,
        char BottomIntersection,
        char LeftIntersection,
        char RightIntersection
    );

    public class ConverterHandlerAsciiOutputOptions : ConverterHandlerBaseOptions
    {
        public readonly Dictionary<string, TableCharacterConfig> TableTypes = new()
        {
            {
                "Single",
                new TableCharacterConfig(
                                       '┌', '┐', '└', '┘', '└', '┘', '─', '│', '┼', '┬', '┴', '├', '┤'
                                   )
            },
            {
                "Double",
                new TableCharacterConfig(
                                       '╔', '╗', '╚', '╝', '╚', '╝', '═', '║', '╬', '╦', '╩', '╠', '╣'
                                   )
            },
            {
                "Bold",
                new TableCharacterConfig(
                                       '┏', '┓', '┗', '┛', '┗', '┛', '━', '┃', '╋', '┳', '┻', '┣', '┫'
                                   )
            },
            {
                "Round",
                new TableCharacterConfig(
                                       '╭', '╮', '╰', '╯', '╰', '╯', '─', '│', '┼', '┬', '┴', '├', '┤'
                                   )
            },
            {
                "Bold Round",
                new TableCharacterConfig(
                                       '╭', '╮', '╰', '╯', '╰', '╯', '━', '┃', '╋', '┳', '┻', '┣', '┫'
                                   )
            },
            {
                "Classic",
                new TableCharacterConfig(
                                       '+', '+', '+', '+', '+', '+', '-', '|', '+', '+', '+', '+', '+'
                                   )
            },
        };

        public string SelectedTableType { get; set; } = "Single";

        public readonly Dictionary<string, TextAlignment> TextAlignment = new()
        {
            { "Left", (TextAlignment)0 },
            { "Center", (TextAlignment)1 },
            { "Right", (TextAlignment)2 }
        };

        public string SelectedTextAlignment { get; set; } = "Left";

        public readonly Dictionary<string, string> CommentTypes = new()
        {
            { "None", "" },
            { "Double-Slash (//)", "//" },
            { "Hash (#)", "#" },
            { "Semi-Colon (;)", ";" },
            { "Double-Dash (--)","--" },
            { "Percent (%)", "%" },
            { "Asterisk (*)", "*" }
        };

        public string SelectedCommentType { get; set; } = "None";

        public bool ForceRowSeparators { get; set; } = false;
    }
}
