using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerMultiLineInput : ConverterHandlerInputAbstract<ConverterHandlerMultiLineOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        using (var reader = new StringReader(text))
        {
            var firstLine = true;

            var row = new List<string>();

            for (var line = reader?.ReadLine()?.Trim(); !string.IsNullOrEmpty(line); line = reader?.ReadLine()?.Trim())
            {
                if (line is null) continue;

                if (line == Options!.RowSeparator)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                    }
                    else
                    {
                        rows.Add(row.ToArray());
                        row.Clear();
                    }
                }
                else if (firstLine)
                {
                    headers.Add(line);
                }
                else
                {
                    row.Add(line);
                }
            }

            if (row.Count > 0) rows.Add(row.ToArray());
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }
}