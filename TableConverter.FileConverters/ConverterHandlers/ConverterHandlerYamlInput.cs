using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerYamlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        public override Result<TableData> ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            Dictionary<string, string> yamlData = new();

            using (var reader = new StringReader(text))
            {
                var firstLine = true;

                for (var line = reader.ReadLine()?.Trim();
                        !string.IsNullOrEmpty(line);
                        line = reader?.ReadLine()?.Trim())
                {
                    if (line.StartsWith("---"))
                    {
                        continue;
                    }

                    if (line.StartsWith('-') && line.EndsWith('-'))
                    {
                        if (yamlData.Count >  0)
                        {
                            if (firstLine)
                            {
                                headers = yamlData.Keys.ToList();
                                firstLine = false;
                            }

                            rows.Add(yamlData.Select(values => values.Value).ToArray());
                        }

                        yamlData = new();
                    }
                    else
                    {
                        var lineData = line.Split(':');

                        if (lineData.Length == 2)
                        {
                            yamlData.Add(lineData[0].Trim(), lineData[1].Trim());
                        }
                    }
                }

                if (yamlData.Count > 0)
                {
                    rows.Add(yamlData.Select(values => values.Value).ToArray());
                }
            }

            return Result<TableData>.Success(new TableData(headers, rows));
        }
    }
}
