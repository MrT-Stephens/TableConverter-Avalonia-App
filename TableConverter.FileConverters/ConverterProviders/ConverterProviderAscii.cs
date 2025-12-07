using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderAscii : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata();

    public override IConverterHandlerInput CreateInputHander()
    {
        throw new NotImplementedException();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        throw new NotImplementedException();
    }
}
