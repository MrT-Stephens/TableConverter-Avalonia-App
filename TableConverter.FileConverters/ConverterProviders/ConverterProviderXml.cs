using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderXml : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "XML",
        [".xml"],
        ["application/xml"],
        ["public.xml"],
        "XML stands for eXtensible Markup Language. XML file is a markup language much like HTML and it was designed to store and transport data.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerXmlInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerXmlOutput();
    }
}