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

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
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

                        SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                    }

                    return writer.ToString();
                }
            });
        }
    }
}
