using Avalonia.Controls;
using Avalonia.Layout;
using NPOI.OpenXmlFormats.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerPhpOutputService : ConverterHandlerOutputAbstract
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
                    writer.Write("array(");
                    writer.Write(Environment.NewLine);

                    writer.Write(GeneratePHPArray(headers));

                    for (long i = 0; i < rows.LongLength; i++)
                    {
                        writer.Write(GeneratePHPArray(rows[i]));

                        SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                    }

                    writer.Write(");"); 
                    writer.Write(Environment.NewLine);

                    return writer.ToString();
                }
            });
        }

        private static string GeneratePHPArray(object?[] values)
        {
            using (var writer = new StringWriter())
            {
                writer.Write("\tarray(");

                for (long i = 0; i < values.LongLength; i++)
                {
                    writer.Write($"\"val{i}\"=>\"{values[i]}\"");

                    if (i < values.LongLength - 1)
                    {
                        writer.Write(",");
                    }
                }

                writer.Write(")");
                writer.Write(Environment.NewLine);

                return writer.ToString();
            }
        }
    }
}
