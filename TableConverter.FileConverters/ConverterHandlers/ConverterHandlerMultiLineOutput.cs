using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerMultiLineOutput : ConverterHandlerOutputAbstract<ConverterHandlerMultiLineOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        using var writer = new StringWriter();

        foreach (var column in headers) writer.WriteLine(column);

        foreach (var str in rows)
        {
            if (Options!.RowSeparator != string.Empty) writer.WriteLine(Options!.RowSeparator);

            for (long j = 0; j < headers.LongLength; j++) writer.WriteLine(str[j]);
        }

        return Result<string>.Success(writer.ToString());
    }
}