using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerYamlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        Dictionary<string, string> yamlData = new();

        using (var reader = new StringReader(text))
        {
            int position = 0, lineNumber = 0;
            var firstLine = true;

            for (var line = reader.ReadLine()?.Trim();
                 !string.IsNullOrEmpty(line);
                 line = reader?.ReadLine()?.Trim())
            {
                position += line.Length + 1;
                lineNumber += 1;

                if (line.StartsWith("---")) continue;

                if (line.StartsWith('-'))
                {
                    if (yamlData.Count > 0)
                    {
                        if (firstLine)
                        {
                            headers = yamlData.Keys.ToList();
                            firstLine = false;
                        }

                        if (yamlData.Count != headers.Count)
                            return Result<TableData>.Failure(
                                $"Incorrect number of columns at char position '{position}' in line '{lineNumber}'");

                        rows.Add(yamlData.Select(values => values.Value).ToArray());
                    }

                    yamlData = new Dictionary<string, string>();
                }
                else
                {
                    var lineData = line.Split(':');

                    if (lineData.Length == 2) yamlData.Add(lineData[0].Trim(), lineData[1].Trim());
                }
            }

            if (yamlData.Count > 0)
            {
                if (yamlData.Count != headers.Count)
                    return Result<TableData>.Failure(
                        $"Incorrect number of columns at char position '{position}' in line '{lineNumber}'");

                rows.Add(yamlData.Select(values => values.Value).ToArray());
            }
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }
}