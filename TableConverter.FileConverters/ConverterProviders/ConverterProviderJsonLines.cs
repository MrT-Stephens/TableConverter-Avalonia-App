using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderJsonLines : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "JSONLines",
        [".jsonl"],
        ["application/x-jsonlines"],
        ["public.json"],
        "JSON Lines is a convenient format for storing structured data that may be processed one record at a time. It works well with unix-style text processing tools and shell pipelines. It's a great format for log files. It's also a flexible format for passing messages between cooperating processes.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerJsonLinesInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerJsonLinesOutput();
    }
}