using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderLaTex : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "LaTeX",
        [".tex"],
        ["application/x-tex"],
        ["public.latex"],
        "LaTeX is a typesetting and document preparation system that includes features designed for the production of technical and scientific documentation, LaTeX allows typesetting math easily.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return null!;
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerLaTexOutput();
    }
}