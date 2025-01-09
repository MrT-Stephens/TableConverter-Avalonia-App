using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerAspInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        try
        {
            using var reader = new StringReader(text);
            var firstLine = true;
            long columnsCount = 0, rowsCount = 0;
            long parsedRows = 0;

            for (var line = reader.ReadLine()?.Trim();
                 !string.IsNullOrEmpty(line);
                 line = reader?.ReadLine()?.Trim())
            {
                parsedRows++;

                if (firstLine)
                {
                    line = line.Replace("Dim arr(", "").Replace(")", "");

                    var values = line.Split(",");

                    columnsCount = long.Parse(values[0]);
                    rowsCount = long.Parse(values[1]);

                    for (long i = 0; i < columnsCount; i++) headers.Add(string.Empty);

                    for (long i = 0; i < rowsCount - 1; i++) rows.Add(new string[columnsCount]);

                    firstLine = false;
                }
                else
                {
                    line = line.Replace("arr(", "").Replace(")", "");

                    var indexes = line.Substring(0, line.IndexOf('=')).Trim().Split(',');

                    if (long.Parse(indexes[0]) < columnsCount && long.Parse(indexes[1]) < rowsCount)
                    {
                        if (int.Parse(indexes[1]) == 0)
                        {
                            headers[int.Parse(indexes[0])] = line.Substring(line.IndexOf('=') + 1).Trim();
                        }
                        else
                        {
                            var index1 = int.Parse(indexes[1]) - 1;
                            var index2 = int.Parse(indexes[0]);

                            rows[index1][index2] = line.Substring(line.IndexOf('=') + 1).Trim();
                        }
                    }
                }
            }

            if (columnsCount != 0 && rowsCount != 0 && parsedRows / columnsCount < rowsCount &&
                parsedRows / rowsCount < columnsCount)
                return Result<TableData>.Failure("Incorrect number of rows of data in the file");
        }
        catch (Exception ex)
        {
            return Result<TableData>.Failure(ex.Message);
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }
}