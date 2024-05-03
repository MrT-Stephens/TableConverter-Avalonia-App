using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.DataModels;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerPhpInputService : ConverterHandlerInputAbstract
    {
        public override void InitializeControls()
        {
            Controls = null;
        }

        public override Task<TableData> ReadTextAsync(string text)
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
                        if (line.StartsWith("array(") && line.EndsWith(")"))
                        {
                            string[] values = line.Replace("array(", string.Empty).Replace(")", string.Empty).Split(",");

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
            });
        }
    }
}
