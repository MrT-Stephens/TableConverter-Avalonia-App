using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlersOptions;

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
    public enum TableStyles
    {
        Single,
        Double,
        Bold,
        Rounded,
        BoldRounded,
        Classic,
    }

    public enum CommentStyles
    {
        None,
        DoubleSlash,
        HashTag,
        Semicolon,
        DoubleDashes,
        Percent,
        Asterisk,
    }

    public readonly Dictionary<CommentStyles, string> CommentTypes = new()
    {
        { CommentStyles.None, "" },
        { CommentStyles.DoubleSlash, "//" },
        { CommentStyles.HashTag, "#" },
        { CommentStyles.Semicolon, ";" },
        { CommentStyles.DoubleDashes, "--" },
        { CommentStyles.Percent, "%" },
        { CommentStyles.Asterisk, "*" }
    };

    public readonly Dictionary<TableStyles, TableCharacterConfig> TableTypes = new()
    {
        {
            TableStyles.Single,
            new TableCharacterConfig(
                '┌', '┐', '└', '┘', '└', '┘', '─', '│', '┼', '┬', '┴', '├', '┤'
            )
        },
        {
            TableStyles.Double,
            new TableCharacterConfig(
                '╔', '╗', '╚', '╝', '╚', '╝', '═', '║', '╬', '╦', '╩', '╠', '╣'
            )
        },
        {
            TableStyles.Bold,
            new TableCharacterConfig(
                '┏', '┓', '┗', '┛', '┗', '┛', '━', '┃', '╋', '┳', '┻', '┣', '┫'
            )
        },
        {
            TableStyles.Rounded,
            new TableCharacterConfig(
                '╭', '╮', '╰', '╯', '╰', '╯', '─', '│', '┼', '┬', '┴', '├', '┤'
            )
        },
        {
            TableStyles.BoldRounded,
            new TableCharacterConfig(
                '╭', '╮', '╰', '╯', '╰', '╯', '━', '┃', '╋', '┳', '┻', '┣', '┫'
            )
        },
        {
            TableStyles.Classic,
            new TableCharacterConfig(
                '+', '+', '+', '+', '+', '+', '-', '|', '+', '+', '+', '+', '+'
            )
        }
    };
    
    private TableStyles _SelectedTableType = TableStyles.Single;
    public TableStyles SelectedTableType
    {
        get => _SelectedTableType;
        set => SetField(ref _SelectedTableType, value);
    }
    
    private TextAlignment _SelectedTextAlignment = TextAlignment.Left;
    public TextAlignment SelectedTextAlignment
    {
        get => _SelectedTextAlignment;
        set => SetField(ref _SelectedTextAlignment, value);
    }
    
    private CommentStyles _SelectedCommentType = CommentStyles.None;
    public CommentStyles SelectedCommentType
    {
        get => _SelectedCommentType;
        set => SetField(ref _SelectedCommentType, value);
    }

    private bool _ForceRowSeparators;
    public bool ForceRowSeparators
    {
        get => _ForceRowSeparators;
        set => SetField(ref _ForceRowSeparators, value);
    }
}