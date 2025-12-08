using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderWord : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "Word",
        [".docx"],
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        ["com.microsoft.word.docx"],
        "Microsoft Word is a word processor developed by Microsoft. It was first released on October 25, 1983 under the name Multi-Tool Word for Xenix systems.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerWordInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerWordOutput();
    }
}