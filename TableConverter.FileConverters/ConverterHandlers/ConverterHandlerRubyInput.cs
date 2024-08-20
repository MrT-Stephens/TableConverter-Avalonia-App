using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerRubyInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        public override TableData ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            using (var reader = new StringReader(text))
            {
                bool first_line = true;

                for (string? line = reader?.ReadLine()?.Trim().Replace("\t", ""); 
                        !string.IsNullOrEmpty(line);
                        line = reader?.ReadLine()?.Trim().Replace("\t", ""))
                {
                    if (line.StartsWith("{") && line.EndsWith("}"))
                    {
                        string[] values = line.Replace("{", string.Empty).Replace("}", string.Empty).Split(",");

                        values = values.Select(val => val.Substring(val.IndexOf("=>") + 2).Trim('"')).ToArray();

                        if (first_line)
                        {
                            foreach (string value in values)
                            {
                                headers.Add(value);
                            }

                            first_line = false;
                        }
                        else
                        {
                            rows.Add(values);
                        }
                    }
                }
            }

            return new TableData(headers, rows);
        }
    }
}
