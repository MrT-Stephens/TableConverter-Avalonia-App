using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerJsonLinesInputService : ConverterHandlerInputAbstract
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

                    for (string? line = reader.ReadLine(); !string.IsNullOrEmpty(line); line = reader.ReadLine())
                    { 
                        if (line is not null && line.StartsWith('{') && line.EndsWith('}'))
                        {
                            List<string> row = new List<string>();
                            string[] strings = line.Trim('{', '}').Split(',');

                            foreach (string str in strings)
                            {
                                if (string.IsNullOrEmpty(str))
                                {
                                    continue;
                                }

                                string[] key_value = str.Split(':');

                                if (first_line)
                                {
                                    headers.Add(key_value[0].Trim().Trim('"'));
                                }

                                row.Add(key_value[1].Trim().Trim('"'));
                            }

                            rows.Add(row.ToArray());
                        }
                        else if (line is not null)
                        {
                            string[] strings = line.Trim('[', ']').Split(',');

                            if (first_line)
                            {
                                foreach (string str in strings)
                                {
                                    headers.Add(str.Trim().Trim('"'));
                                }
                            }
                            else
                            {
                                rows.Add(strings.Select(str => str.Trim().Trim('"')).ToArray());
                            }
                        }

                        first_line = false;
                    }
                }

                return (headers, rows);
            });
        }
    }
}
