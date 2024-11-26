using System.Xml;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerXmlOutput : ConverterHandlerOutputAbstract<ConverterHandlerXmlOutputOptions>
    {
        public override Result<string> Convert(string[] headers, string[][] rows)
        {
            var xmlDocument = new XmlDocument();

            // Create XML declaration
            var xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDocument.AppendChild(xmlDeclaration);

            // Create root element
            var rootElement = xmlDocument.CreateElement(Options!.XmlRootNodeName.Replace(' ', '_'));
            xmlDocument.AppendChild(rootElement);

            // Iterate over DataTable rows
            for (var i = 0; i < rows.Length; i++)
            {
                // Create record element
                var recordElement = xmlDocument.CreateElement(Options!.XmlElementNodeName.Replace(' ', '_'));

                // Iterate over DataTable columns
                for (var j = 0; j < headers.Length; j++)
                {
                    // Create element for each column and set its value
                    var columnElement = xmlDocument.CreateElement(headers[j].Replace(' ', '_'));
                    columnElement.InnerText = rows[i][j];
                    recordElement.AppendChild(columnElement);
                }

                // Append record element to the root
                rootElement.AppendChild(recordElement);
            }

            var textWriter = new StringWriter();
            var xmlWriter = new XmlTextWriter(textWriter);

            xmlWriter.Formatting = (Options!.MinifyXml) ? Formatting.None : Formatting.Indented;

            xmlDocument.WriteTo(xmlWriter);

            return Result<string>.Success(textWriter.ToString());
        }
    }
}
