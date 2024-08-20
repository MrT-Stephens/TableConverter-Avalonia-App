namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public class ConverterHandlerJsonOutputOptions : ConverterHandlerBaseOptions
    {
        public readonly string[] JsonFormatTypes = 
        [
            "Array of Objects",
            "2D Arrays",
            "Column Arrays",
            "Keyed Arrays",
        ];

        public string SelectedJsonFormatType { get; set; } = "Array of Objects";

        public bool MinifyJson { get; set; } = false;
    }
}
