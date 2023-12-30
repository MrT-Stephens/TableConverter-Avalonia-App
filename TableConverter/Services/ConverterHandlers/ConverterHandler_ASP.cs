using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
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

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

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
                                data_table.Columns.Add();
                            }

                            for (int i = 0; i < rows - 1; i++)
                            {
                                data_table.Rows.Add();
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
                                    data_table.Columns[int.Parse(indexes[0])].ColumnName = line.Substring(line.IndexOf("=") + 1).Trim();
                                }
                                else
                                {
                                    data_table.Rows[int.Parse(indexes[1]) - 1][int.Parse(indexes[0])] = line.Substring(line.IndexOf("=") + 1).Trim();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return new DataTable();
                }

                return data_table;
            });
        }

        public override Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = $"Dim arr({input.Columns.Count},{input.Rows.Count + 1}){Environment.NewLine}";

                foreach (DataColumn column in input.Columns)
                {
                    output += $"arr({column.Ordinal},0) = {column.ColumnName}{Environment.NewLine}";
                }

                for (int i = 0; i < input.Rows.Count; i++)
                {
                    for (int j = 0; j < input.Columns.Count; j++)
                    {
                        output += $"arr({j},{i + 1}) = {input.Rows[i][j]}{Environment.NewLine}";
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                }

                return output;
            });
        }
    }
}