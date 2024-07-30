using Avalonia.Platform.Storage;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerWordInputService : ConverterHandlerInputAbstract
    {
        private XWPFDocument? WordDocument { get; set; } = null;

        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<TableData> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                try
                {
                    if (WordDocument == null)
                    {
                        throw new Exception("Word Document is not initialized.");
                    }

                    foreach (var row in WordDocument.Tables[0].Rows)
                    {
                        if (row == WordDocument.Tables[0].Rows[0])
                        {
                            foreach (var cell in row.GetTableCells())
                            {
                                headers.Add(cell.GetText());
                            }
                        }
                        else
                        {
                            rows.Add(row.GetTableCells().Select(cell => cell.GetText()).ToArray());
                        }
                    }

                    WordDocument.Close();
                    WordDocument.Dispose();
                    WordDocument = null;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while reading the Word file.", ex);
                }

                return new TableData(headers, rows);
            });
        }

        public override Task<string> ReadFileAsync(IStorageFile storage_file)
        {
            return Task.Run(async () =>
            {
                using (var stream = await storage_file.OpenReadAsync())
                {
                    WordDocument = new XWPFDocument(stream);

                    return $"Word files are not visible within this text box 😭{Environment.NewLine}";
                }
            });
        }
    }
}
