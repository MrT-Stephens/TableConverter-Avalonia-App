using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerYamlOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
    { 
        public override string Convert(string[] headers, string[][] rows)
        {
            using (var writer = new StringWriter())
            {
                writer.Write($"---{Environment.NewLine}");

                for (long i = 0; i < rows.LongLength; i++)
                {
                    writer.Write($"-{Environment.NewLine}");

                    for (long j = 0; j < headers.LongLength; j++)
                    {
                        writer.Write($"\t{headers[j].Replace(' ', '_')}: {rows[i][j]}{Environment.NewLine}");
                    }
                }

                return writer.ToString();
            }
        }
    }
}
