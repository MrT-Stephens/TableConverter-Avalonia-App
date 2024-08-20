using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerMultiLineOutput : ConverterHandlerOutputAbstract<ConverterHandlerMultiLineOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            using (var writer = new StringWriter())
            {
                foreach (string column in headers)
                {
                    writer.WriteLine(column);
                }

                for (long i = 0; i < rows.LongLength; i++)
                {
                    if (Options!.RowSeparator != string.Empty)
                    {
                        writer.WriteLine(Options!.RowSeparator);
                    }

                    for (long j = 0; j < headers.LongLength; j++)
                    {
                        writer.WriteLine(rows[i][j]);
                    }
                }

                return writer.ToString();
            }
        }
    }
}
