using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderAsp : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "ASP",
        [".asp", ".txt"],
        ["application/x-asp", "text/plain"],
        ["public.asp", "public.plain-text"],
        "ASP stands for Active Server Pages. ASP is a development framework for building web pages. ASP supports many different development models Classic ASP. ASP.NET Web Forms.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerAspInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerAspOutput();
    }
}