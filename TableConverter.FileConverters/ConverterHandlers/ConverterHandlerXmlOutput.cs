using System.Xml;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerXmlOutput : ConverterHandlerOutputAbstract<ConverterHandlerXmlOutputOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            XmlDocument xml_document = new XmlDocument();

            // Create XML declaration
            XmlDeclaration xml_declaration = xml_document.CreateXmlDeclaration("1.0", "UTF-8", null);
            xml_document.AppendChild(xml_declaration);

            // Create root element
            XmlElement root_element = xml_document.CreateElement(Options!.XmlRootNodeName.Replace(' ', '_'));
            xml_document.AppendChild(root_element);

            // Iterate over DataTable rows
            for (long i = 0; i < rows.LongLength; i++)
            {
                // Create record element
                XmlElement record_element = xml_document.CreateElement(Options!.XmlElementNodeName.Replace(' ', '_'));

                // Iterate over DataTable columns
                for (long j = 0; j < headers.LongLength; j++)
                {
                    // Create element for each column and set its value
                    XmlElement columnElement = xml_document.CreateElement(headers[j].Replace(' ', '_'));
                    columnElement.InnerText = rows[i][j];
                    record_element.AppendChild(columnElement);
                }

                // Append record element to the root
                root_element.AppendChild(record_element);
            }

            StringWriter text_writer = new StringWriter();
            XmlTextWriter xml_writer = new XmlTextWriter(text_writer);

            xml_writer.Formatting = (Options!.MinifyXml) ? Formatting.None : Formatting.Indented;

            xml_document.WriteTo(xml_writer);

            return text_writer.ToString();
        }
    }
}
