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

    public JsonStyles SelectedJsonFormatType { get; set; } = JsonStyles.ArrayOfObjects;
}