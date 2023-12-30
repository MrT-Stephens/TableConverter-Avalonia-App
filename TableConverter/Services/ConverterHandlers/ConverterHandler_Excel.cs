using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Data;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Avalonia.Threading;
using System;
using System.Linq;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_Excel : ConverterHandlerBase
    {
        private IWorkbook? workbook { get; set; }
        private string SheetName { get; set; } = "Sheet1";

        public override void InitializeControls()
        {
            base.InitializeControls();

            var SheetNameStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var SheetNameLabel = new Label { Content = "Sheet Name:" };

            var SheetNameTextBox = new TextBox
            {
                Text = SheetName
            };

            SheetNameTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    SheetName = ((TextBox)sender).Text;
                }
            };

            SheetNameStackPanel.Children.Add(SheetNameLabel);
            SheetNameStackPanel.Children.Add(SheetNameTextBox);

            Controls?.Add(SheetNameStackPanel);
        }

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

                try
                {
                    IWorkbook workbook = new XSSFWorkbook(await input.OpenReadAsync());

                    ISheet sheet = workbook.GetSheetAt(0);

                    foreach (IRow row in sheet)
                    {
                        if (row.RowNum == 0)
                        {
                            foreach (var cell in row.Cells)
                            {
                                data_table.Columns.Add(cell.ToString());
                            }
                        }
                        else
                        {
                            data_table.Rows.Add(row.ToArray());
                        }
                    }

                    workbook.Close();
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
                workbook = new XSSFWorkbook();

                ISheet sheet = workbook.CreateSheet(SheetName);

                IRow header_row = sheet.CreateRow(0);

                for (int i = 0; i < input.Columns.Count; i++)
                {
                    header_row.CreateCell(i).SetCellValue(input.Columns[i].ColumnName);
                }

                for (int i = 0; i < input.Rows.Count; i++)
                {
                    IRow row = sheet.CreateRow(i + 1);

                    for (int j = 0; j < input.Columns.Count; j++)
                    {
                        row.CreateCell(j).SetCellValue(input.Rows[i][j].ToString());
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                }

                for (int i = 0; i < input.Columns.Count; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                return $"Please download the '.xlsx' file to view the generated data 😁{Environment.NewLine}";
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
                            workbook?.Write(task.Result);

                            task.Result.Close();
                        }
                    });
                }
            });
        }
    }
}
