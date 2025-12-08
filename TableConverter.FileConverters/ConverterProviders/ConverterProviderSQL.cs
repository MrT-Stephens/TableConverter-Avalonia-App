using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderSQL : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "SQL",
        [".sql", ".txt"],
        ["application/sql", "text/plain"],
        ["public.sql", "public.plain-text"],
        "SQL stands for Structured Query Language. It is used for storing, retrieving, managing and manipulating data in relational database management system (RDMS).",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerSQLInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerSQLOutput();
    }
}