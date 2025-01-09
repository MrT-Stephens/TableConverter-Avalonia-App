using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerPhpOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        using var writer = new StringWriter();

        writer.Write("array(");
        writer.Write(Environment.NewLine);

        writer.Write(GeneratePhpArray(headers));

        for (long i = 0; i < rows.LongLength; i++) writer.Write(GeneratePhpArray(rows[i]));

        writer.Write(");");
        writer.Write(Environment.NewLine);

        return Result<string>.Success(writer.ToString());
    }

    private static string GeneratePhpArray(string[] values)
    {
        using var writer = new StringWriter();

        writer.Write("\tarray(");

        for (var i = 0; i < values.Length; i++)
        {
            writer.Write($"\"val{i}\"=>\"{values[i]}\"");

            if (i < values.LongLength - 1) writer.Write(",");
        }

        writer.Write(")");
        writer.Write(Environment.NewLine);

        return writer.ToString();
    }
}