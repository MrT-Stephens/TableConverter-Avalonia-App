namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerJsonLinesOutputOptions : ConverterHandlerBaseOptions
{
    public enum JsonLinesStyles
    {
        Objects,
        Arrays
    }

    private JsonLinesStyles _SelectedJsonLinesFormatType = JsonLinesStyles.Arrays;
    public JsonLinesStyles SelectedJsonLinesFormatType
    {
        get => _SelectedJsonLinesFormatType;
        set => SetField(ref _SelectedJsonLinesFormatType, value);
    }
}