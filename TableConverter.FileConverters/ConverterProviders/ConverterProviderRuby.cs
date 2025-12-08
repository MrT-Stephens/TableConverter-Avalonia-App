using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderRuby : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "Ruby",
        [".rb", ".txt"],
        ["application/x-ruby", "text/plain"],
        ["public.ruby", "public.plain-text"],
        "Ruby is a dynamic, open source programming language with a focus on simplicity and productivity. It has an elegant syntax that is natural to read and easy to write.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerRubyInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerRubyOutput();
    }
}