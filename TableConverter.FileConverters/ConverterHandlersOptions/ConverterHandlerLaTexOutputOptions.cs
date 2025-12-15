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

    private TableTypes _SelectedTableType = TableTypes.All;
    public TableTypes SelectedTableType
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

    private TextAlignment _SelectedTableAlignment = TextAlignment.Center;
    public TextAlignment SelectedTableAlignment
    {
        get => _SelectedTableAlignment;
        set => SetField(ref _SelectedTableAlignment, value);
    }

    private CaptionAlignments _SelectedCaptionAlignment = CaptionAlignments.Top;
    public CaptionAlignments SelectedCaptionAlignment
    {
        get => _SelectedCaptionAlignment;
        set => SetField(ref _SelectedCaptionAlignment, value);
    }

    private string _CaptionName = string.Empty;
    public string CaptionName
    {
        get => _CaptionName;
        set => SetField(ref _CaptionName, value);
    }

    private string _LabelName = string.Empty;
    public string LabelName
    {
        get => _LabelName;
        set => SetField(ref _LabelName, value);
    }

    private bool _MinimalWorkingExample;
    public bool MinimalWorkingExample
    {
        get => _MinimalWorkingExample;
        set => SetField(ref _MinimalWorkingExample, value);
    }

    private bool _BoldHeader;
    public bool BoldHeader
    {
        get => _BoldHeader;
        set => SetField(ref _BoldHeader, value);
    }

    private bool _BoldFirstColumn;
    public bool BoldFirstColumn
    {
        get => _BoldFirstColumn;
        set => SetField(ref _BoldFirstColumn, value);
    }
}