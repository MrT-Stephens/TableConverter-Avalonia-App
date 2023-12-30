using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_Ruby : ConverterHandlerBase
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
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    while (!reader.EndOfStream)
                    {
                        string line = (await reader.ReadLineAsync()).Trim().Replace("\t", "");

                        if (line.StartsWith("{") && line.EndsWith("}"))
                        {
                            string[] values = line.Replace("{", string.Empty).Replace("}", string.Empty).Split(",");

                            values = values.Select(val => val.Substring(val.IndexOf("=>") + 2).Trim('"')).ToArray();

                            if (first_line)
                            {
                                foreach (string value in values)
                                {
                                    data_table.Columns.Add(value);
                                }

                                first_line = false;
                            }
                            else
                            {
                                data_table.Rows.Add(values);
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
                string output = string.Empty;

                output += "[" + Environment.NewLine;

                output += GenerateRubyArray(input.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray());

                for (int i = 0; i < input.Rows.Count; ++i)
                {
                    output += GenerateRubyArray(input.Rows[i].ItemArray.ToArray());

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                }

                return output += "];" + Environment.NewLine;
            });
        }

        private static string GenerateRubyArray(object?[] values)
        {
            string output = "\t{";

            for (int i = 0; i < values.Length; ++i)
            {
                output += $"\"val{i}\"=>\"{values[i]}\"";

                if (i < values.Length - 1)
                {
                    output += ",";
                }
            }

            return output + "}" + Environment.NewLine;
        }
    }
}
