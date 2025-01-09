namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerHtmlOutputOptions : ConverterHandlerBaseOptions
{
    public bool MinifyHtml { get; set; } = false;

    public bool IncludeTheadTbody { get; set; } = false;
}