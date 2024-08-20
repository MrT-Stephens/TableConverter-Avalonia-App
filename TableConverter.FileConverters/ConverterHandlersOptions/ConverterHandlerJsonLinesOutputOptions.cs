namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public class ConverterHandlerJsonLinesOutputOptions : ConverterHandlerBaseOptions
    {
        public readonly string[] JsonLinesFormatTypes = 
        [
            "Objects",
            "Arrays",
        ];

        public string SelectedJsonLinesFormatType { get; set; } = "Objects";
    }
}
