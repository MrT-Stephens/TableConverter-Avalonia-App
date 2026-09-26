using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.ConverterProviders;
using TableConverter.FileConverters.Interfaces;
using TableConverter.FileConverters.Services;
using TableConverter.FileConverters.Tests.TestBase;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Tests.OutputConverterTests;

public class MarkdownOutputTest
{
    [Fact]
    public async Task Convert_Does_Not_Mutate_The_Caller_Headers_Or_Rows()
    {
        // Regression: this handler used to bold cells in place, corrupting the caller's table.
        var handler = new ConverterHandlerMarkdownOutput
        {
            Options = new ConverterHandlerMarkdownOutputOptions
            {
                BoldColumnNames = true,
                BoldFirstColumn = true
            }
        };

        var headers = new[] { "Name", "Age" };
        var rows = new[] { new[] { "Alice", "30" }, new[] { "Bob", "25" } };

        var result = await Utils.ConvertToTextAsync(handler, headers, rows);

        Assert.True(result.IsSuccess, result.Error);

        Assert.Equal(new[] { "Name", "Age" }, headers);
        Assert.Equal(new[] { "Alice", "30" }, rows[0]);
        Assert.Equal(new[] { "Bob", "25" }, rows[1]);

        Assert.Contains("**Name**", result.Value);
        Assert.Contains("**Alice**", result.Value);
    }

    [Fact]
    public async Task Convert_Does_Not_Mutate_The_Caller_Data_When_Bolding_Is_Disabled()
    {
        var handler = new ConverterHandlerMarkdownOutput
        {
            Options = new ConverterHandlerMarkdownOutputOptions()
        };

        var headers = new[] { "Name", "Age" };
        var rows = new[] { new[] { "Alice", "30" } };

        var result = await Utils.ConvertToTextAsync(handler, headers, rows);

        Assert.True(result.IsSuccess, result.Error);

        Assert.Equal(new[] { "Name", "Age" }, headers);
        Assert.Equal(new[] { "Alice", "30" }, rows[0]);

        Assert.DoesNotContain("**", result.Value);
    }

    [Fact]
    public async Task Convert_Renders_A_Table_With_The_Expected_Shape()
    {
        var handler = new ConverterHandlerMarkdownOutput
        {
            Options = new ConverterHandlerMarkdownOutputOptions()
        };

        var result = await Utils.ConvertToTextAsync(handler, ["Name", "Age"], [["Alice", "30"]]);

        Assert.True(result.IsSuccess, result.Error);

        var lines = result.Value.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, lines.Length);
        Assert.StartsWith("|", lines[0]);
        Assert.StartsWith("|", lines[1]);
        Assert.StartsWith("|", lines[2]);
        Assert.Contains("Name", lines[0]);
        Assert.Contains("Alice", lines[2]);
    }

    [Fact]
    public async Task Convert_Handles_Rows_With_Fewer_Cells_Than_Headers()
    {
        // Regression: this used to throw IndexOutOfRangeException.
        var handler = new ConverterHandlerMarkdownOutput
        {
            Options = new ConverterHandlerMarkdownOutputOptions()
        };

        var result = await Utils.ConvertToTextAsync(handler, ["A", "B", "C"], [["1"]]);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Contains("1", result.Value);
    }

    [Fact]
    public async Task Convert_Handles_Rows_With_More_Cells_Than_Headers()
    {
        // Regression: this used to throw IndexOutOfRangeException.
        var handler = new ConverterHandlerMarkdownOutput
        {
            Options = new ConverterHandlerMarkdownOutputOptions()
        };

        var result = await Utils.ConvertToTextAsync(handler, ["A"], [["1", "2"]]);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Contains("1", result.Value);
    }

    [Fact]
    public async Task Convert_Handles_Empty_Input()
    {
        var handler = new ConverterHandlerMarkdownOutput
        {
            Options = new ConverterHandlerMarkdownOutputOptions()
        };

        var result = await Utils.ConvertToTextAsync(handler, [], []);

        Assert.True(result.IsSuccess, result.Error);
    }

    [Theory]
    [InlineData(ConverterHandlerMarkdownOutputOptions.TableStyles.Normal)]
    [InlineData(ConverterHandlerMarkdownOutputOptions.TableStyles.Simple)]
    public async Task Convert_Does_Not_Mutate_Input_For_Any_Table_Style(
        ConverterHandlerMarkdownOutputOptions.TableStyles style)
    {
        var handler = new ConverterHandlerMarkdownOutput
        {
            Options = new ConverterHandlerMarkdownOutputOptions
            {
                SelectedTableType = style,
                BoldColumnNames = true,
                BoldFirstColumn = true
            }
        };

        var headers = new[] { "Name", "Age" };
        var rows = new[] { new[] { "Alice", "30" } };

        var result = await Utils.ConvertToTextAsync(handler, headers, rows);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(new[] { "Name", "Age" }, headers);
        Assert.Equal(new[] { "Alice", "30" }, rows[0]);
    }

    [Fact]
    public async Task Export_Does_Not_Corrupt_The_Source_Table()
    {
        // The end-to-end path: ConverterService hands the handler the *same* row arrays the table holds.
        var service = new ConverterService([new ConverterProviderMarkdown()]);

        var options = service.GetOutputOptionsByName<ConverterHandlerMarkdownOutputOptions>("Markdown");
        Assert.NotNull(options);
        options!.BoldColumnNames = true;
        options.BoldFirstColumn = true;

        var table = new TableSnapshot(["Name", "Age"], [["Alice", "30"]]);

        var path = Path.Combine(Path.GetTempPath(), $"tableconverter-{Guid.NewGuid():N}.md");

        try
        {
            await service.ExportFileAsync("Markdown", path, table);

            Assert.Equal(new[] { "Name", "Age" }, table.Headers);
            Assert.Equal(new[] { "Alice", "30" }, table.Rows[0]);

            Assert.True(File.Exists(path));
            Assert.Contains("**Alice**", File.ReadAllText(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void GetOutputByName_Returns_The_Same_Instance_When_Called_Concurrently()
    {
        var service = new ConverterService([new ConverterProviderMarkdown()]);

        var handlers = new IConverterHandlerOutput[64];

        Parallel.For(0, handlers.Length, i => handlers[i] = service.GetOutputByName("Markdown"));

        Assert.All(handlers, handler => Assert.Same(handlers[0], handler));
    }

    [Fact]
    public void GetOutputByName_Throws_A_Clear_Error_For_An_Unsupported_Name()
    {
        var service = new ConverterService([new ConverterProviderMarkdown()]);

        var exception = Assert.Throws<InvalidOperationException>(() => service.GetOutputByName("Nope"));

        Assert.Contains("not supported", exception.Message);
    }
}
