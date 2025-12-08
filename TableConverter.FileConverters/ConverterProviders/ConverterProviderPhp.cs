using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderPhp : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "PHP",
        [".php"],
        ["application/x-httpd-php"],
        ["public.php"],
        "PHP (recursive acronym for PHP Hypertext Preprocessor ) is a widely-used open source general-purpose scripting language that is especially suited for web development and can be embedded into HTML.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerPhpInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerPhpOutput();
    }
}