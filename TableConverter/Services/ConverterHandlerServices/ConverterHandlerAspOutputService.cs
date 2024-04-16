using Avalonia.Threading;
using System;
using System.IO;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerAspOutputService : ConverterHandlerOutputAbstract
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

                        SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                    }

                    return writer.ToString();
                }
            });
        }
    }
}
