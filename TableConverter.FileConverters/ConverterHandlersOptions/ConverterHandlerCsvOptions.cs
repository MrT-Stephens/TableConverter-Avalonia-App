namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public class ConverterHandlerCsvOptions : ConverterHandlerBaseOptions
    {
        public string Delimiter { get; set; } = ",";

        public bool Header { get; set; } = true;
    }
}
