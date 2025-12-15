namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerExcelOutputOptions : ConverterHandlerBaseOptions
{
    private string _SheetName = string.Empty;
    public string SheetName
    {
        get => _SheetName;
        set => SetField(ref _SheetName, value);
    }
}