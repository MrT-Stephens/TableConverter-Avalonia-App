namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerJsonOutputOptions : ConverterHandlerBaseOptions
{
    public enum JsonStyles
    {
        ArrayOfObjects,
        TwoDimensionalArrays,
        ColumnArrays,
        KeyedArrays,
    }

    private JsonStyles _SelectedJsonFormatType = JsonStyles.ArrayOfObjects;
    public JsonStyles SelectedJsonFormatType
    {
        get => _SelectedJsonFormatType;
        set => SetField(ref _SelectedJsonFormatType, value);
    }

    private bool _MinifyJson;
    public bool MinifyJson
    {
        get => _MinifyJson;
        set => SetField(ref _MinifyJson, value);
    }
}