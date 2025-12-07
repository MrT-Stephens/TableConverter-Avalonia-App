using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerLaTexOutputOptions : ConverterHandlerBaseOptions
{
    public enum CaptionAlignments
    {
        Top,
        Bottom,
    }

    public enum TableTypes
    {
        All,
        MySQL,
        Excel,
        Horizontal,
        Markdown,
        None,
    }

    public TableTypes SelectedTableType { get; set; } = TableTypes.All;

    public TextAlignment SelectedTextAlignment { get; set; } = TextAlignment.Left;

    public TextAlignment SelectedTableAlignment { get; set; } = TextAlignment.Left;

    public CaptionAlignments SelectedCaptionAlignment { get; set; } = CaptionAlignments.Top;

    public string CaptionName { get; set; } = "";

    public string LabelName { get; set; } = "";

    public bool MinimalWorkingExample { get; set; } = false;

    public bool BoldHeader { get; set; } = false;

    public bool BoldFirstColumn { get; set; } = false;
}