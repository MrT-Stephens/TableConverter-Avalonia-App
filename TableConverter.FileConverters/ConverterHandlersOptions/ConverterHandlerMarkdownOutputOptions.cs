using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerMarkdownOutputOptions : ConverterHandlerBaseOptions
{
    public enum TableStyles
    {
        Normal,
        Simple,
    }

    public TableStyles SelectedTableType { get; set; } = TableStyles.Normal;

    public TextAlignment SelectedTextAlignment { get; set; } = TextAlignment.Left;

    public bool BoldColumnNames { get; set; } = false;

    public bool BoldFirstColumn { get; set; } = false;
}