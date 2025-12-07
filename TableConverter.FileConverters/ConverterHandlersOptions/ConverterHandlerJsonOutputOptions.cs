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

    public JsonStyles SelectedJsonFormatType { get; set; } = JsonStyles.ArrayOfObjects;

    public bool MinifyJson { get; set; } = false;
}