using System.Text;
using Newtonsoft.Json.Linq;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;
using TableConverter.FileConverters.Tests.TestBase;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Tests.OutputConverterTests;

/// <summary>
///     Covers the single export path of the text converters. A handler has one write method, so these
///     tests drive it directly and assert on the text it produces.
/// </summary>
public class TextOutputStreamingTest
{
    private static readonly TableSnapshot Table = new(
        ["Name", "Age"],
        [["Alice", "30"], ["Bob", "25"]]);

    /// <summary>
    ///     Writes <paramref name="table" /> out through <paramref name="handler" /> and returns the text,
    ///     checking along the way that the conversion succeeded and did not take the caller's stream down
    ///     with it.
    /// </summary>
    private static async Task<string> WriteAsync(IConverterHandlerOutput handler, TableSnapshot table)
    {
        using var stream = new MemoryStream();

        var result = await handler.ConvertToStreamAsync(stream, table);

        Assert.True(result.IsSuccess, $"{handler.GetType().Name}: {result.Error}");

        // The stream is owned by the caller, so it must still be writable after the conversion.
        Assert.True(stream.CanWrite);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    [Fact]
    public async Task Every_Text_Handler_Writes_The_Table_Out()
    {
        IConverterHandlerOutput[] handlers =
        [
            new ConverterHandlerCsvOutput(),
            new ConverterHandlerHtmlOutput(),
            new ConverterHandlerJsonLinesOutput(),
            new ConverterHandlerMultiLineOutput(),
            new ConverterHandlerPhpOutput(),
            new ConverterHandlerRubyOutput(),
            new ConverterHandlerYamlOutput(),
            new ConverterHandlerSQLOutput(),
            new ConverterHandlerLaTexOutput(),
            new ConverterHandlerXmlOutput(),
            new ConverterHandlerMarkdownOutput(),
            new ConverterHandlerAsciiOutput(),
            new ConverterHandlerAspOutput(),
            new ConverterHandlerJsonOutput()
        ];

        foreach (var handler in handlers)
        {
            var text = await WriteAsync(handler, Table);

            Assert.False(string.IsNullOrEmpty(text), $"{handler.GetType().Name} wrote nothing.");
        }
    }

    [Fact]
    public async Task Sql_MultiRow_Streaming_Writes_A_Single_Insert()
    {
        var handler = new ConverterHandlerSQLOutput();
        handler.Options!.InsertMultiRowsAtOnce = true;

        var text = await WriteAsync(handler, Table);

        Assert.Contains("INSERT INTO", text);
        Assert.Contains($",{Environment.NewLine} (", text);
        Assert.EndsWith($";{Environment.NewLine}", text);
    }

    [Fact]
    public async Task LaTex_Every_Table_Style_Succeeds()
    {
        foreach (var style in Enum.GetValues<ConverterHandlerLaTexOutputOptions.TableTypes>())
        {
            var handler = new ConverterHandlerLaTexOutput();
            handler.Options!.SelectedTableType = style;

            var text = await WriteAsync(handler, Table);

            Assert.Contains("\\begin{tabular}", text);
            Assert.Contains("Alice", text);
        }
    }

    [Fact]
    public async Task Streaming_Handles_Ragged_Rows()
    {
        // A row shorter than the headers must be padded rather than producing too few cells.
        var table = new TableSnapshot(["A", "B", "C"], [["1"]]);

        var csv = await WriteAsync(new ConverterHandlerCsvOutput(), table);
        var sql = await WriteAsync(new ConverterHandlerSQLOutput(), table);
        var latex = await WriteAsync(new ConverterHandlerLaTexOutput(), table);

        Assert.Contains("1,,", csv);
        Assert.Contains("'1', '', ''", sql);
        Assert.Contains("1 &  & ", latex);
    }

    [Fact]
    public async Task Streaming_Handles_An_Empty_Table()
    {
        var table = new TableSnapshot([], []);

        await WriteAsync(new ConverterHandlerCsvOutput(), table);
        await WriteAsync(new ConverterHandlerSQLOutput(), table);
        await WriteAsync(new ConverterHandlerLaTexOutput(), table);
    }

    [Fact]
    public async Task Json_Every_Style_Produces_Valid_Json()
    {
        // JSON is a single document rather than a sequence of rows, so it is written through a JSON
        // writer that streams the text out, and every style must produce a well formed document.
        foreach (var style in Enum.GetValues<ConverterHandlerJsonOutputOptions.JsonStyles>())
        {
            var handler = new ConverterHandlerJsonOutput();
            handler.Options!.SelectedJsonFormatType = style;

            var text = await WriteAsync(handler, Table);

            // Parsing proves the document was fully flushed and is well formed.
            JArray.Parse(text);
        }
    }

    [Fact]
    public async Task Json_Minified_Has_No_Newlines()
    {
        var handler = new ConverterHandlerJsonOutput();
        handler.Options!.MinifyJson = true;

        var text = await WriteAsync(handler, Table);

        Assert.DoesNotContain(Environment.NewLine, text);
    }
}
