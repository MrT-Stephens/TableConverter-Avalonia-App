using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderCsv : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "CSV",
        [".csv", ".txt"],
        ["text/csv", "text/plain"],
        ["public.comma-separated-values", "public.plain-text"],
        "CSV stands for Comma-Separated Values. CSV file format is a text file that has a specific format which allows data to be saved in a table structured format.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerCsvInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerCsvOutput();
    }
}