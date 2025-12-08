using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderYaml : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "YAML",
        [".yaml"],
        ["application/x-yaml"],
        ["public.yaml"],
        "YAML stands for YAML Ain't Markup Language. YAML is a human-readable data serialization language. It is commonly used for configuration files and in applications where data is being stored or transmitted.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerYamlInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerYamlOutput();
    }
}