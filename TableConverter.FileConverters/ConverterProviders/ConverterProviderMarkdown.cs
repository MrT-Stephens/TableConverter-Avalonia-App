using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderMarkdown : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "Markdown",
        [".md"],
        ["text/markdown"],
        ["public.markdown"],
        "Markdown is a text-to-HTML conversion tool for web writers. Markdown allows you to write using an easy-to-read, easy-to-write plain text format, then convert it to HTML.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return null!;
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerMarkdownOutput();
    }
}