using System;
using System.IO;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerYamlOutputService : ConverterHandlerOutputAbstract
    {
        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows)
        {
            return Task.Run(() =>
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
            });
        }
    }
}
