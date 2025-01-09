namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerPdfOutputOptions : ConverterHandlerBaseOptions
{
    public readonly string[] Colours =
    [
        "#FFFFFF",
        "#F0F0F0",
        "#D9D9D9",
        "#BFBFBF",
        "#A6A6A6",
        "#8C8C8C",
        "#737373",
        "#595959",
        "#404040",
        "#262626",
        "#0D0D0D",
        "#000000"
    ];

    public string SelectedBackgroundColor { get; set; } = "#FFFFFF";

    public string SelectedForegroundColor { get; set; } = "#000000";

    public bool BoldHeader { get; set; } = true;

    public bool ShowGridLines { get; set; } = true;
}