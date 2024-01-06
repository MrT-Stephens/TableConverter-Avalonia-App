using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Data;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Avalonia.Threading;
using System;
using System.Linq;
using System.Collections.Generic;

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

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

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
                                column_values.Add(cell.ToString());
                            }
                        }
                        else
                        {
                            row_values.Add(row.Select(value => value.ToString()).ToArray());
                        }
                    }

                    workbook.Close();
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
                workbook = new XSSFWorkbook();

                ISheet sheet = workbook.CreateSheet(SheetName);

                IRow header_row = sheet.CreateRow(0);

                for (int i = 0; i < column_values.Length; i++)
                {
                    header_row.CreateCell(i).SetCellValue(column_values[i]);
                }

                for (int i = 0; i < row_values.Length; i++)
                {
                    IRow row = sheet.CreateRow(i + 1);

                    for (int j = 0; j < column_values.Length; j++)
                    {
                        row.CreateCell(j).SetCellValue(row_values[i][j]);
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                }

                for (int i = 0; i < column_values.Length; i++)
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
