using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderMultiLine : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "Multi-Line",
        [".txt"],
        ["text/plain"],
        ["public.plain-text"],
        "Multi-Line is a text file that has a specific format which allows data to be saved in a table structured format.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerMultiLineInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerMultiLineOutput();
    }
}