using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerRubyInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        using (var reader = new StringReader(text))
        {
            var firstLine = true;

            for (var line = reader?.ReadLine()?.Trim().Replace("\t", "");
                 !string.IsNullOrEmpty(line);
                 line = reader?.ReadLine()?.Trim().Replace("\t", ""))
                if (line.StartsWith("{") && line.EndsWith("}"))
                {
                    var values = line.Replace("{", string.Empty).Replace("}", string.Empty).Split(",");

                    values = values
                        .Select(val => val.Substring(val.IndexOf("=>", StringComparison.Ordinal) + 2).Trim('"'))
                        .ToArray();

                    if (firstLine)
                    {
                        headers.AddRange(values);

                        firstLine = false;
                    }
                    else
                    {
                        if (values.Length != headers.Count)
                            return Result<TableData>.Failure($"Incorrect number of columns at row {rows.Count}.");

                        rows.Add(values);
                    }
                }
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }
}