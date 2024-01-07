using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
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

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

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
                                    column_values.Add(value);
                                }

                                first_line = false;
                            }
                            else
                            {
                                row_values.Add(values);
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
                string output = string.Empty;

                output += "[" + Environment.NewLine;

                output += GenerateRubyArray(column_values);

                for (int i = 0; i < row_values.Length; ++i)
                {
                    output += GenerateRubyArray(row_values[i]);

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
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
