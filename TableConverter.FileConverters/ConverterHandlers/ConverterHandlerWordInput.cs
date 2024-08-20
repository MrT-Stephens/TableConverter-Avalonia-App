using NPOI.XWPF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerWordInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        private XWPFDocument? WordDocument { get; set; } = null;

        public override TableData ReadText(string text)
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
        }

        public override string ReadFile(Stream? stream)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            WordDocument = new XWPFDocument(stream);

            return $"Word files are not visible within this text box 😭{Environment.NewLine}";
        }
    }
}
