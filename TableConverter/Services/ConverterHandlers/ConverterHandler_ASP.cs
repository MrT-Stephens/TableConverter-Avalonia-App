using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_ASP : ConverterHandlerBase
    {
        public override void InitializeControls()
        {
            base.InitializeControls();
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    bool first_line = true;
                    int columns = 0, rows = 0;
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        if (first_line)
                        {
                            line = line.Replace("Dim arr(", string.Empty).Replace(")", string.Empty);

                            string[] values = line.Split(",");

                            columns = int.Parse(values[0]);
                            rows = int.Parse(values[1]);

                            for (int i = 0; i < columns; i++)
                            {
                                column_values.Add(string.Empty);
                            }

                            for (int i = 0; i < rows - 1; i++)
                            {
                                row_values.Add(new string[columns]);
                            }

                            first_line = false;
                        }
                        else
                        {
                            line = line.Replace("arr(", string.Empty).Replace(")", string.Empty);

                            string[] indexes = line.Substring(0, line.IndexOf("=")).Trim().Split(',');

                            if (int.Parse(indexes[0]) < columns && int.Parse(indexes[1]) < rows)
                            {
                                if (int.Parse(indexes[1]) == 0)
                                {
                                    column_values[int.Parse(indexes[0])] = line.Substring(line.IndexOf("=") + 1).Trim();
                                }
                                else
                                {
                                    row_values[int.Parse(indexes[1]) - 1][int.Parse(indexes[0])] = line.Substring(line.IndexOf("=") + 1).Trim();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return (new List<string>(), new List<string[]>());
                }

                return (column_values, row_values);
            });
        }

        public override Task<string> ConvertAsync(string[] column_values, string[][] row_values, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = $"Dim arr({column_values.Length},{row_values.Length + 1}){Environment.NewLine}";

                for (int i = 0; i < column_values.Length; ++i)
                {
                    output += $"arr({i},0) = {column_values[i]}{Environment.NewLine}";
                }

                for (int i = 0; i < row_values.Length; i++)
                {
                    for (int j = 0; j < column_values.Length; j++)
                    {
                        output += $"arr({j},{i + 1}) = {row_values[i][j]}{Environment.NewLine}";
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                }

                return output;
            });
        }
    }
}