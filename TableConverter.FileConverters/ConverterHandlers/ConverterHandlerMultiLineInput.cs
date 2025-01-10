using System.Text.RegularExpressions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public partial class ConverterHandlerMultiLineInput : ConverterHandlerInputAbstract<ConverterHandlerMultiLineOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        // Validate row separator
        if (!SpecialCharacterRegex().IsMatch(Options!.RowSeparator))
            return Result<TableData>.Failure(
                $"Invalid row separator '{Options.RowSeparator}'. It must contain only special characters.");

        if (!text.Contains(Options.RowSeparator))
            return Result<TableData>.Failure($"Row separator '{Options.RowSeparator}' not found in the input text.");

        using (var reader = new StringReader(text))
        {
            var firstLine = true;
            var row = new List<string>();
            var rowNumber = 0;

            for (var line = reader.ReadLine()?.Trim(); !string.IsNullOrEmpty(line); line = reader?.ReadLine()?.Trim())
            {
                rowNumber++;

                if (line == Options.RowSeparator)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                    }
                    else
                    {
                        if (row.Count != headers.Count)
                            return Result<TableData>.Failure(
                                $"Row {rowNumber - 1} has inconsistent column count. Expected {headers.Count}, found {row.Count}.\nRow data: {string.Join(", ", row)}");

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

            // Validate last row if not empty
            if (row.Count > 0)
            {
                if (row.Count != headers.Count)
                    return Result<TableData>.Failure(
                        $"Row {rowNumber} has inconsistent column count. Expected {headers.Count}, found {row.Count}.\nRow data: {string.Join(", ", row)}");

                rows.Add(row.ToArray());
            }
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }

    [GeneratedRegex(@"^[^\p{L}\p{N}\s]+$")]
    private static partial Regex SpecialCharacterRegex();
}