using TableConverter.DataModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerAspOutputService : ConverterHandlerOutputAbstract
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
                    writer.Write($"Dim arr({headers.LongLength},{rows.LongLength + 1}){Environment.NewLine}");

                    for (long i = 0; i < headers.LongLength; i++)
                    {
                        writer.Write($"arr({i},0) = {headers[i]}{Environment.NewLine}");
                    }

                    for (long i = 0; i < rows.LongLength; i++)
                    {
                        for (long j = 0; j < headers.LongLength; j++)
                        {
                            writer.Write($"arr({j},{i + 1}) = {rows[i][j]}{Environment.NewLine}");
                        }
                    }

                    return writer.ToString();
                }
            });
        }
    }
}
