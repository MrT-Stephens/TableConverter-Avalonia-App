namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerHtmlOutputOptions : ConverterHandlerBaseOptions
{
    private bool _MinifyHtml;
    public bool MinifyHtml
    {
        get => _MinifyHtml;
        set => SetField(ref _MinifyHtml, value);
    }

    private bool _IncludeTheadTbody;
    public bool IncludeTheadTbody
    {
        get => _IncludeTheadTbody;
        set => SetField(ref _IncludeTheadTbody, value);
    }
}