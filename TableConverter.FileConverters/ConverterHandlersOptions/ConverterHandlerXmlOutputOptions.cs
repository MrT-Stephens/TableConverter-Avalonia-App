namespace TableConverter.FileConverters.ConverterHandlersOptions
{
    public class ConverterHandlerXmlOutputOptions : ConverterHandlerBaseOptions
    {
        public string XmlRootNodeName { get; set; } = "root";

        public string XmlElementNodeName { get; set; } = "element";

        public bool MinifyXml { get; set; } = false;
    }
}
