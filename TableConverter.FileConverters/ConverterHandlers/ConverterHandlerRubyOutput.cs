using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerRubyOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            using (var writer = new StringWriter())
            {
                writer.Write("[");
                writer.Write(Environment.NewLine);

                writer.Write(GenerateRubyArray(headers));

                for (long i = 0; i < rows.LongLength; i++)
                {
                    writer.Write(GenerateRubyArray(rows[i]));
                }

                writer.Write("];");
                writer.Write(Environment.NewLine);

                return writer.ToString();
            }
        }

        private static string GenerateRubyArray(object?[] values)
        {
            using (var writer = new StringWriter())
            {
                writer.Write("\t{");

                for (long i = 0; i < values.LongLength; i++)
                {
                    writer.Write($"\"val{i}\"=>\"{values[i]}\"");

                    if (i < values.LongLength - 1)
                    {
                        writer.Write(",");
                    }
                }

                writer.Write("}");
                writer.Write(Environment.NewLine);

                return writer.ToString();
            }
        }
    }
}
