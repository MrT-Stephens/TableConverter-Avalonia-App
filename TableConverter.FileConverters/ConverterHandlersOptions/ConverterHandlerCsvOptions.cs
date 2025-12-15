namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerCsvOptions : ConverterHandlerBaseOptions
{
    private string _Delimiter = ",";
    public string Delimiter
    {
        get => _Delimiter;
        set => SetField(ref _Delimiter, value);
    }

    private bool _IncludeHeader = true;
    public bool IncludeHeader
    {
        get => _IncludeHeader;
        set => SetField(ref _IncludeHeader, value);
    }
}