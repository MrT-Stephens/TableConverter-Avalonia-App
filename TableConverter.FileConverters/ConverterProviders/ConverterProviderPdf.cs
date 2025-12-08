using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderPdf : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "PDF",
        [".pdf"],
        ["application/pdf"],
        ["com.adobe.pdf"],
        "PDF stands for Portable Document Format. PDF is a file format developed by Adobe in the 1990s to present documents, including text formatting and images, in a manner independent of application software, hardware, and operating systems.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return null!;
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerPdfOutput();
    }
}