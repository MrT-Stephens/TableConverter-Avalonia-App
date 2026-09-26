using System.Xml;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerXmlOutput : ConverterHandlerOutputAbstract<ConverterHandlerXmlOutputOptions>
{
    public override async Task<Result> ConvertToStreamAsync(
        Stream? stream,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(source);

        await using var writer = TableRowStream.CreateTextWriter(stream);

        var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

        var settings = new XmlWriterSettings
        {
            Indent = !Options!.MinifyXml,
            OmitXmlDeclaration = true,
            // The writer belongs to the caller, so it must survive being disposed here.
            CloseOutput = false
        };

        // The declaration is written with a fixed encoding so the output does not depend on which writer
        // the caller supplied; the converters have always emitted UTF-8.
        writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

        using var xmlWriter = XmlWriter.Create(writer, settings);

        var rootName = Options!.XmlRootNodeName.Replace(' ', '_');
        var elementName = Options!.XmlElementNodeName.Replace(' ', '_');

        xmlWriter.WriteStartElement(rootName);

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            xmlWriter.WriteStartElement(elementName);

            // Iterate over DataTable columns
            for (var j = 0; j < headers.Count; j++)
            {
                // Create element for each column and set its value
                xmlWriter.WriteStartElement(headers[j].Replace(' ', '_'));

                // Guard against ragged rows: the caller may supply fewer cells than there are headers.
                xmlWriter.WriteString(j < row.Length ? row[j] : string.Empty);

                xmlWriter.WriteEndElement();
            }

            // Append record element to the root
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.Flush();

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}