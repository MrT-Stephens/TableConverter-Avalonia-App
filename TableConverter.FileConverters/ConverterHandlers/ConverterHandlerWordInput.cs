using NPOI.XWPF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerWordInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    private XWPFDocument? WordDocument { get; set; }

    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        try
        {
            if (WordDocument == null) throw new Exception("Word Document is not initialized.");

            foreach (var row in WordDocument.Tables[0].Rows)
                if (row == WordDocument.Tables[0].Rows[0])
                    headers.AddRange(row.GetTableCells().Select(cell => cell.GetText()));
                else
                    rows.Add(row.GetTableCells().Select(cell => cell.GetText()).ToArray());

            WordDocument.Close();
            WordDocument.Dispose();
            WordDocument = null;
        }
        catch (Exception ex)
        {
            return Result<TableData>.Failure(ex.Message);
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }

    public override Result<string> ReadFile(Stream? stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        try
        {
            WordDocument = new XWPFDocument(stream);

            if (WordDocument.Tables.Count == 0)
                return Result<string>.Failure("No tables found in the Word document");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error reading Word file: '{ex.Message}'");
        }

        return Result<string>.Success($"Word files are not visible within this text box 😭{Environment.NewLine}");
    }
}