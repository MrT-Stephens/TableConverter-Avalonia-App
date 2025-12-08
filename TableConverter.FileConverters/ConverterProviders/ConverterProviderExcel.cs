using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.ConverterProviders;

public class ConverterProviderExcel : ConverterProvider
{
    public override IConverterMetadata Metadata => new ConverterMetadata(
        "Excel",
        [".xlsx"],
        ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
        ["com.microsoft.excel.xls"],
        "Microsoft Excel is an electronic spreadsheet application that enables users to store, organize, calculate and manipulate the data with formulas using a spreadsheet system broken up by rows and columns.",
        ConverterSupport.Input | ConverterSupport.Output);
    
    public override IConverterHandlerInput CreateInputHander()
    {
        return new ConverterHandlerExcelInput();
    }

    public override IConverterHandlerOutput CreateOutputHander()
    {
        return new ConverterHandlerExcelOutput();
    }
}