using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderJsonInput : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "JSON",
        [".json"],
        ["application/json"],
        ["public.json"],
        "JSON stands for JavaScript Object Notation. JSON file is a text-based format for representing structured data based on JavaScript object syntax.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerJsonInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerJsonOutput();
    }
}