using System.Drawing;

namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerPdfOutputOptions : ConverterHandlerBaseOptions
{
    public KnownColor SelectedBackgroundColor { get; set; } = KnownColor.White;

    public KnownColor SelectedForegroundColor { get; set; } = KnownColor.Black;

    public bool BoldHeader { get; set; } = true;

    public bool ShowGridLines { get; set; } = true;
}