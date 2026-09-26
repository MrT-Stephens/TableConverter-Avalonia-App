using Newtonsoft.Json.Linq;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;

namespace TableConverter.FileConverters.Tests.OutputConverterTests;

public class JsonOutputTest
{
    private static readonly string[] Headers = ["Name", "Age"];
    private static readonly string[][] Rows = [["Alice", "30"], ["Bob", "25"]];

    private static ConverterHandlerJsonOutput CreateHandler(
        ConverterHandlerJsonOutputOptions.JsonStyles style,
        bool minify = false)
    {
        return new ConverterHandlerJsonOutput
        {
            Options = new ConverterHandlerJsonOutputOptions
            {
                SelectedJsonFormatType = style,
                MinifyJson = minify
            }
        };
    }

    [Fact]
    public async Task ArrayOfObjects_Produces_An_Object_Per_Row()
    {
        var result = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.ArrayOfObjects), Headers, Rows);

        Assert.True(result.IsSuccess, result.Error);

        var json = JArray.Parse(result.Value);

        Assert.Equal(2, json.Count);
        Assert.Equal("Alice", json[0]!["Name"]!.Value<string>());
        Assert.Equal("30", json[0]!["Age"]!.Value<string>());
        Assert.Equal("Bob", json[1]!["Name"]!.Value<string>());
    }

    [Fact]
    public async Task TwoDimensionalArrays_Produces_Headers_Then_Rows()
    {
        var result = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.TwoDimensionalArrays), Headers, Rows);

        Assert.True(result.IsSuccess, result.Error);

        var json = JArray.Parse(result.Value);

        Assert.Equal(3, json.Count);
        Assert.Equal("Name", json[0]![0]!.Value<string>());
        Assert.Equal("Age", json[0]![1]!.Value<string>());
        Assert.Equal("Alice", json[1]![0]!.Value<string>());
    }

    [Fact]
    public async Task ColumnArrays_Produces_An_Array_Per_Column()
    {
        var result = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.ColumnArrays), Headers, Rows);

        Assert.True(result.IsSuccess, result.Error);

        var json = JArray.Parse(result.Value);

        Assert.Equal(2, json.Count);
        Assert.Equal("Alice", json[0]!["Name"]![0]!.Value<string>());
        Assert.Equal("Bob", json[0]!["Name"]![1]!.Value<string>());
        Assert.Equal("30", json[1]!["Age"]![0]!.Value<string>());
    }

    [Fact]
    public async Task KeyedArrays_Produces_A_Header_Row_Then_Keyed_Rows()
    {
        var result = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.KeyedArrays), Headers, Rows);

        Assert.True(result.IsSuccess, result.Error);

        var json = JArray.Parse(result.Value);

        Assert.Equal(3, json.Count);
        Assert.Equal("Name", json[0]!["0"]![0]!.Value<string>());
        Assert.Equal("Alice", json[1]!["1"]![0]!.Value<string>());
        Assert.Equal("Bob", json[2]!["2"]![0]!.Value<string>());
    }

    [Fact]
    public async Task Minify_Removes_The_Indentation()
    {
        var indented = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.ArrayOfObjects), Headers, Rows);

        var minified = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.ArrayOfObjects, true), Headers, Rows);

        Assert.True(indented.IsSuccess, indented.Error);
        Assert.True(minified.IsSuccess, minified.Error);

        Assert.Contains(Environment.NewLine, indented.Value);
        Assert.DoesNotContain(Environment.NewLine, minified.Value);
    }

    [Fact]
    public async Task Replaces_Spaces_In_Property_Names()
    {
        var result = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.ArrayOfObjects), ["First Name"], [["Alice"]]);

        Assert.True(result.IsSuccess, result.Error);

        var json = JArray.Parse(result.Value);

        Assert.Equal("Alice", json[0]!["First_Name"]!.Value<string>());
    }

    [Fact]
    public async Task Handles_An_Empty_Table()
    {
        var result = await Utils.ConvertToTextAsync(
            CreateHandler(ConverterHandlerJsonOutputOptions.JsonStyles.ArrayOfObjects), [], []);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Empty(JArray.Parse(result.Value));
    }
}
