using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerAspInputService : ConverterHandlerInputAbstract
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
                    long columns_count = 0, rows_count = 0;

                    for (string? line = reader?.ReadLine()?.Trim();
                         !string.IsNullOrEmpty(line);
                         line = reader?.ReadLine()?.Trim())
                    {
                        if (first_line)
                        {
                            line = line.Replace("Dim arr(", "").Replace(")", "");

                            string[] values = line.Split(",");

                            columns_count = long.Parse(values[0]);
                            rows_count = long.Parse(values[1]);

                            for (long i = 0; i < columns_count; i++)
                            {
                                headers.Add(string.Empty);
                            }

                            for (long i = 0; i < rows_count - 1; i++)
                            {
                                rows.Add(new string[columns_count]);
                            }

                            first_line = false;
                        }
                        else
                        {
                            line = line.Replace("arr(", "").Replace(")", "");

                            string[] indexes = line.Substring(0, line.IndexOf("=")).Trim().Split(',');

                            if (long.Parse(indexes[0]) < columns_count && long.Parse(indexes[1]) < rows_count)
                            {
                                if (int.Parse(indexes[1]) == 0)
                                {
                                    headers[int.Parse(indexes[0])] = line.Substring(line.IndexOf("=") + 1).Trim();
                                }
                                else
                                {
                                    rows[int.Parse(indexes[1]) - 1][int.Parse(indexes[0])] = line.Substring(line.IndexOf("=") + 1).Trim();
                                }
                            }
                        }
                    }
                }

                return (headers, rows);
            });
        }
    }
}
