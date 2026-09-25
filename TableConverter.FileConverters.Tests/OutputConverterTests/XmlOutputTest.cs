using System.Xml.Linq;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.ConverterProviders;
using TableConverter.FileConverters.Services;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Tests.OutputConverterTests;

public class XmlOutputTest
{
    private static ConverterHandlerXmlOutput CreateHandler(bool minify = false)
    {
        return new ConverterHandlerXmlOutput
        {
            Options = new ConverterHandlerXmlOutputOptions
            {
                XmlRootNodeName = "root",
                XmlElementNodeName = "record",
                MinifyXml = minify
            }
        };
    }

    [Fact]
    public void Convert_Produces_Well_Formed_Xml()
    {
        var handler = CreateHandler();

        var result = handler.Convert(["Name", "Age"], [["Alice", "30"], ["Bob", "25"]]);

        Assert.True(result.IsSuccess, result.Error);

        // Parsing proves the output was fully flushed and is well formed.
        var document = XDocument.Parse(result.Value);

        Assert.Equal("root", document.Root!.Name.LocalName);
        Assert.Equal(2, document.Root.Elements("record").Count());

        var first = document.Root.Elements("record").First();
        Assert.Equal("Alice", first.Element("Name")!.Value);
        Assert.Equal("30", first.Element("Age")!.Value);
    }

    [Fact]
    public void Convert_Replaces_Spaces_In_Node_Names()
    {
        var handler = CreateHandler();

        var result = handler.Convert(["First Name"], [["Alice"]]);

        Assert.True(result.IsSuccess, result.Error);

        var document = XDocument.Parse(result.Value);

        Assert.Equal("Alice", document.Root!.Element("record")!.Element("First_Name")!.Value);
    }

    [Fact]
    public void Convert_Handles_Rows_With_Fewer_Cells_Than_Headers()
    {
        // Regression: this used to throw IndexOutOfRangeException.
        var handler = CreateHandler();

        var result = handler.Convert(["A", "B", "C"], [["1"]]);

        Assert.True(result.IsSuccess, result.Error);

        var document = XDocument.Parse(result.Value);
        var record = document.Root!.Element("record")!;

        Assert.Equal("1", record.Element("A")!.Value);
        Assert.Equal(string.Empty, record.Element("B")!.Value);
        Assert.Equal(string.Empty, record.Element("C")!.Value);
    }

    [Fact]
    public void Convert_Handles_Empty_Input()
    {
        var handler = CreateHandler();

        var result = handler.Convert([], []);

        Assert.True(result.IsSuccess, result.Error);

        var document = XDocument.Parse(result.Value);

        Assert.Empty(document.Root!.Elements());
    }

    [Fact]
    public void Convert_Honours_The_Minify_Option()
    {
        var indented = CreateHandler().Convert(["A"], [["1"]]).Value;
        var minified = CreateHandler(minify: true).Convert(["A"], [["1"]]).Value;

        Assert.Contains(Environment.NewLine, indented);
        Assert.DoesNotContain(Environment.NewLine, minified);
    }

    [Fact]
    public void OutputFile_Writes_A_Parsable_Document()
    {
        var service = new ConverterService([new ConverterProviderXml()]);

        var options = service.GetOutputOptionsByName<ConverterHandlerXmlOutputOptions>("XML");
        Assert.NotNull(options);
        options!.XmlRootNodeName = "root";
        options.XmlElementNodeName = "record";

        var tableData = new TableData(["Name", "Age"], [["Alice", "30"]]);

        var path = Path.Combine(Path.GetTempPath(), $"tableconverter-{Guid.NewGuid():N}.xml");

        try
        {
            service.OutputFile("XML", path, tableData);

            Assert.True(File.Exists(path));

            var document = XDocument.Load(path);

            Assert.Equal("Alice", document.Root!.Element("record")!.Element("Name")!.Value);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
