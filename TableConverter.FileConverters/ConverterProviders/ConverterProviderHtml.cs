using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderHtml : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "HTML",
        [".html"],
        ["text/html"],
        ["public.html"],
        "HTML stands for Hypertext Markup Language. HTML is the code that is used to structure a web page and its content, paragraphs, list, images and tables etc.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerHtmlInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerHtmlOutput();
    }
}