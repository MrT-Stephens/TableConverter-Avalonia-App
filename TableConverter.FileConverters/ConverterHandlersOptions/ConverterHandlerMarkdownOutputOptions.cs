using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public class ConverterHandlerMarkdownOutputOptions : ConverterHandlerBaseOptions
    {
        public readonly string[] TableTypes = 
        [
            "Markdown Table (Normal)",
            "Markdown Table (Simple)",
        ];

        public string SelectedTableType { get; set; } = "Markdown Table (Normal)";

        public readonly Dictionary<string, TextAlignment> TextAlignment = new()
        {
            { "Left", (TextAlignment)0 },
            { "Center", (TextAlignment)1 },
            { "Right", (TextAlignment)2 }
        };

        public string SelectedTextAlignment { get; set; } = "Left";

        public bool BoldColumnNames { get; set; } = false;

        public bool BoldFirstColumn { get; set; } = false;
    }
}
