using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
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

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

                try
                {
                    XWPFDocument document = new XWPFDocument(await input.OpenReadAsync());

                    foreach (var row in document.Tables[0].Rows)
                    {
                        if (row == document.Tables[0].Rows[0])
                        {
                            foreach (var cell in row.GetTableCells())
                            {
                                data_table.Columns.Add(cell.GetText());
                            }
                        }
                        else
                        {
                            data_table.Rows.Add(row.GetTableCells().Select(cell => cell.GetText()).ToArray());
                        }
                    }

                    document.Close();
                }
                catch (Exception)
                {
                    return new DataTable();
                }

                return data_table;
            });
        }

        public override Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                document = new XWPFDocument();

                var table = document.CreateTable(input.Rows.Count + 1, input.Columns.Count);

                for (int i = 0; i < input.Columns.Count; i++)
                {
                    table.GetRow(0).GetCell(i).SetText(input.Columns[i].ColumnName);
                }

                for (int i = 0; i < input.Rows.Count; i++)
                {
                    for (int j = 0; j < input.Columns.Count; j++)
                    {
                        table.GetRow(i + 1).GetCell(j).SetText(input.Rows[i][j].ToString());
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
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
