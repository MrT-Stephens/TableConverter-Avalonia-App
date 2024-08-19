namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public class ConverterHandlerJsonInputOptions : ConverterHandlerBaseOptions
    {
        public readonly string[] JsonFormatTypes = [
            "Array of Objects",
            "2D Arrays",
            "Column Arrays",
            "Keyed Arrays",
        ];

        public string SelectedJsonFormatType = "Array of Objects";
    }
}
