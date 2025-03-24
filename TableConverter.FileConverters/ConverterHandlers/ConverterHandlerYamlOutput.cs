using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerYamlOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        using var writer = new StringWriter();

        writer.Write($"---{Environment.NewLine}");

        foreach (var str in rows)
        {
            writer.Write($"-{Environment.NewLine}");

            for (long j = 0; j < headers.LongLength; j++)
                writer.Write($"    {headers[j].Replace(' ', '_')}: {str[j]}{Environment.NewLine}");
        }

        return Result<string>.Success(writer.ToString());
    }
}