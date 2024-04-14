using Avalonia.Platform.Storage;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using TableConverter.Interfaces;
using NPOI.XSSF.UserModel;

namespace TableConverter.Services
{
    internal class ConverterHandlerExcelInputService : ConverterHandlerInputAbstract
    {
        private IWorkbook? ExcelWorkbook { get; set; } = null;

        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<(List<string>, List<string[]>)> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                try
                {
                    if (ExcelWorkbook == null)
                    {
                        throw new Exception("Excel Workbook is not initialized.");
                    }

                    ISheet sheet = ExcelWorkbook.GetSheetAt(0);

                    foreach (IRow row in sheet)
                    {
                        if (row.RowNum == 0)
                        {
                            foreach (var cell in row.Cells)
                            {
                                headers.Add(cell.ToString() ?? "");
                            }
                        }
                        else
                        {
                            rows.Add(row.Select(value => value.ToString() ?? "").ToArray());
                        }
                    }

                    ExcelWorkbook.Close();
                    ExcelWorkbook.Dispose();
                    ExcelWorkbook = null;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while reading the Excel file.", ex);
                }

                return (headers, rows);
            });
        }

        public override Task<string> ReadFileAsync(IStorageFile storage_file)
        {
            return Task.Run(async () =>
            {
                using (var stream = await storage_file.OpenReadAsync())
                {
                    ExcelWorkbook = new XSSFWorkbook(stream);

                    return $"Excel files are not visible within this text box 😭{Environment.NewLine}";
                }
            });
        }
    }
}
