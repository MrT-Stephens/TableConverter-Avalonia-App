namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerJsonInputOptions : ConverterHandlerBaseOptions
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
}