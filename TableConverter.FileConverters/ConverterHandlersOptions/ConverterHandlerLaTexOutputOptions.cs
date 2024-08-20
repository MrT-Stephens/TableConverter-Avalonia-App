namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public class ConverterHandlerLaTexOutputOptions : ConverterHandlerBaseOptions
    {
        public readonly string[] TableTypes = 
        [
            "All",
            "MySQL",
            "Excel",
            "Horizontal",
            "Markdown",
            "None"
        ];

        public string SelectedTableType { get; set; } = "All";

        public readonly string[] Alignments = 
        [
            "Left",
            "Center",
            "Right"
        ];

        public string SelectedTextAlignment { get; set; } = "Left";

        public string SelectedTableAlignment { get; set; } = "Left";

        public readonly string[] CaptionAlignments = 
        [
            "Top",
            "Bottom"
        ];

        public string SelectedCaptionAlignment { get; set; } = "Top";

        public string CaptionName { get; set; } = "";

        public string LabelName { get; set; } = "";

        public bool MinimalWorkingExample { get; set; } = false;

        public bool BoldHeader { get; set; } = false;

        public bool BoldFirstColumn { get; set; } = false;
    }
}
