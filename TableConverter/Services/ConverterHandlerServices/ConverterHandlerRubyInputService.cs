using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerRubyInputService : ConverterHandlerInputAbstract
    {
        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<(List<string>, List<string[]>)> ReadTextAsync(string text)
        {
            return Task.Run(() =>
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

                return (headers, rows);
            });
        }
    }
}
