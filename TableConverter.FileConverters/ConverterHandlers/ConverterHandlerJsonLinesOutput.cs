using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerJsonLinesOutput : ConverterHandlerOutputAbstract<ConverterHandlerJsonLinesOutputOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        using var writer = new StringWriter();

        switch (Options!.SelectedJsonLinesFormatType)
        {
            case ConverterHandlerJsonLinesOutputOptions.JsonLinesStyles.Objects:
            {
                for (long i = 0; i < rows.LongLength; i++)
                {
                    writer.Write("{");

                    for (long j = 0; j < headers.LongLength; j++)
                    {
                        writer.Write($"\"{headers[j]}\":\"{rows[i][j]}\"");

                        if (j != headers.LongLength - 1) writer.Write(",");
                    }

                    writer.Write("}");

                    if (i != rows.LongLength - 1) writer.Write(Environment.NewLine);
                }

                break;
            }
            case ConverterHandlerJsonLinesOutputOptions.JsonLinesStyles.Arrays:
            {
                // Write headers
                writer.Write("[");

                writer.Write(string.Join(",", headers.Select(column => $"\"{column}\"").ToArray()));

                writer.Write("]");

                writer.Write(Environment.NewLine);

                // Write rows
                for (long i = 0; i < rows.LongLength; i++)
                {
                    writer.Write("[");

                    writer.Write(string.Join(",", rows[i].Select(str => $"\"{str}\"").ToArray()));

                    writer.Write("]");

                    if (i != rows.LongLength - 1) writer.Write(Environment.NewLine);
                }

                break;
            }
        }

        return Result<string>.Success(writer.ToString());
    }
}