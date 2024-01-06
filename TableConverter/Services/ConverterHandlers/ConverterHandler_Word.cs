using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_Word : ConverterHandlerBase
    {
        private XWPFDocument? document { get; set; }

        public override void InitializeControls()
        {
            base.InitializeControls();
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    XWPFDocument document = new XWPFDocument(await input.OpenReadAsync());

                    foreach (var row in document.Tables[0].Rows)
                    {
                        if (row == document.Tables[0].Rows[0])
                        {
                            foreach (var cell in row.GetTableCells())
                            {
                                column_values.Add(cell.GetText());
                            }
                        }
                        else
                        {
                            row_values.Add(row.GetTableCells().Select(cell => cell.GetText()).ToArray());
                        }
                    }

                    document.Close();
                }
                catch (Exception)
                {
                    return (new List<string>(), new List<string[]>());
                }

                return (column_values, row_values);
            });
        }

        public override Task<string> ConvertAsync(string[] column_values, string[][] row_values, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                document = new XWPFDocument();

                var table = document.CreateTable(row_values.Length + 1, column_values.Length);

                for (int i = 0; i < column_values.Length; i++)
                {
                    table.GetRow(0).GetCell(i).SetText(column_values[i]);
                }

                for (int i = 0; i < row_values.Length; i++)
                {
                    for (int j = 0; j < column_values.Length; j++)
                    {
                        table.GetRow(i + 1).GetCell(j).SetText(row_values[i][j]);
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                }

                return $"Please download the '.docx' file to view the generated data 😁{Environment.NewLine}";
            });
        }

        public override Task SaveFileAsync(IStorageFile output, string data)
        {
            return Task.Run(async () =>
            {
                if (output != null)
                {
                    await output.OpenWriteAsync().ContinueWith((task) =>
                    {
                        if (task.Result != null)
                        {
                            document?.Write(task.Result);

                            task.Result.Close();
                        }
                    });
                }
            });
        }
    }
}
