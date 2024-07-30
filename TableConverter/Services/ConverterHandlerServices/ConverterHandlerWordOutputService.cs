using Avalonia.Controls;
using Avalonia.Platform.Storage;
using NPOI.XWPF.UserModel;
using System;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerWordOutputService : ConverterHandlerOutputAbstract
    {
        private XWPFDocument? WordDocument { get; set; } = null;

        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                WordDocument = new XWPFDocument();

                var table = WordDocument.CreateTable(rows.Length + 1, headers.Length);

                for (long i = 0; i < headers.LongLength; i++)
                {
                    table.GetRow(0).GetCell((int)i).SetText(headers[i]);
                }

                for (long i = 0; i < rows.LongLength; i++)
                {
                    for (long j = 0; j < headers.LongLength; j++)
                    {
                        table.GetRow((int)i + 1).GetCell((int)j).SetText(rows[i][j]);
                    }

                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                }

                return $"Please save the '.docx' file to view the generated file 😁{Environment.NewLine}";
            });
        }

        public override Task SaveFileAsync(IStorageFile output, ReadOnlyMemory<byte> buffer)
        {
            return Task.Run(async () =>
            {
                if (output is not null)
                {
                    using (var writer = await output.OpenWriteAsync())
                    {
                        WordDocument?.Write(writer);

                        writer.Close();
                    }
                }
            });
        }
    }
}
