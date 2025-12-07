namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerJsonLinesOutputOptions : ConverterHandlerBaseOptions
{
    public enum JsonLinesStyles
    {
        Objects,
        Arrays
    }

    public JsonLinesStyles SelectedJsonLinesFormatType { get; set; } = JsonLinesStyles.Objects;
}