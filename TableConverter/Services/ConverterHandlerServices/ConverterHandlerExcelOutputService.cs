using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Avalonia.Platform.Storage;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerExcelOutputService : ConverterHandlerOutputAbstract
    {
        private IWorkbook? ExcelWorkbook { get; set; } = null;

        private string ExcelSheetName { get; set; } = "Sheet1";

        public override void InitializeControls()
        {
            var delimiter_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var delimiter_label = new Label()
            {
                Content = "Excel Workbook Sheet Name:",
            };

            var delimiter_text_box = new TextBox()
            {
                Text = ExcelSheetName,
            };

            delimiter_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    ExcelSheetName = text_box.Text ?? "Sheet1";
                }
            };

            delimiter_stack_panel.Children.Add(delimiter_label);
            delimiter_stack_panel.Children.Add(delimiter_text_box);

            Controls?.Add(delimiter_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                ExcelWorkbook = new XSSFWorkbook();

                ISheet sheet = ExcelWorkbook.CreateSheet(ExcelSheetName);

                IRow header_row = sheet.CreateRow(0);

                for (long i = 0; i < headers.LongLength; i++)
                {
                    header_row.CreateCell((int)i).SetCellValue(headers[i]);
                }

                for (long i = 0; i < rows.LongLength; i++)
                {
                    IRow row = sheet.CreateRow((int)i + 1);

                    for (long j = 0; j < headers.LongLength; j++)
                    {
                        sheet.AutoSizeColumn((int)i);
                        row.CreateCell((int)j).SetCellValue(rows[i][j]);
                    }

                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                }

                return $"Please save the '.xlsx' file to view the generated file 😁{Environment.NewLine}";
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
                        ExcelWorkbook?.Write(writer);

                        writer.Close();
                    }
                }
            });
        }
    }
}
