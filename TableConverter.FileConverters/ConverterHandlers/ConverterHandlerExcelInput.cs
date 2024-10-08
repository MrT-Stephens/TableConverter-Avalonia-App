﻿using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerExcelInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        private IWorkbook? ExcelWorkbook { get; set; } = null;

        public override TableData ReadText(string text)
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
                        List<string> values = new List<string>();

                        for (int i = 0; i < headers.Count; i++)
                        {
                            var cell = row.GetCell(i);

                            if (cell == null)
                            {
                                values.Add("");
                                continue;
                            }

                            values.Add(cell.ToString() ?? "");
                        }

                        rows.Add(values.ToArray());
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

            return new TableData(headers, rows);
        }

        public override string ReadFile(Stream? stream)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            ExcelWorkbook = new XSSFWorkbook(stream);

            return $"Excel files are not visible within this text box 😭{Environment.NewLine}";
        }
    }
}
