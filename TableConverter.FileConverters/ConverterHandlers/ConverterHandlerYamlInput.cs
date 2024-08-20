using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerYamlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        public override TableData ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            Dictionary<string, string> yaml_data = new();

            using (var reader = new StringReader(text))
            {
                bool first_line = true;

                for (string? line = reader?.ReadLine()?.Trim();
                        !string.IsNullOrEmpty(line);
                        line = reader?.ReadLine()?.Trim())
                {
                    if (line.StartsWith("---"))
                    {
                        continue;
                    }

                    if (line.StartsWith('-') && line.EndsWith('-'))
                    {
                        if (yaml_data.Count >  0)
                        {
                            if (first_line)
                            {
                                headers = yaml_data.Keys.ToList();
                                first_line = false;
                            }

                            rows.Add(yaml_data.Select(values => values.Value).ToArray());
                        }

                        yaml_data = new();
                    }
                    else
                    {
                        string[] line_data = line.Split(':');

                        if (line_data.Length == 2)
                        {
                            yaml_data.Add(line_data[0].Trim(), line_data[1].Trim());
                        }
                    }
                }

                if (yaml_data.Count > 0)
                {
                    rows.Add(yaml_data.Select(values => values.Value).ToArray());
                }
            }

            return new TableData(headers, rows);
        }
    }
}
