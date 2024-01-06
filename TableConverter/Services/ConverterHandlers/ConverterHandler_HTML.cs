using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_HTML : ConverterHandlerBase
    {
        public bool MinifyHtml { get; set; } = false;
        public bool TheadTbody { get; set; } = false;

        public override void InitializeControls()
        {
            base.InitializeControls();

            var MinifyHtmlCheckBox = new CheckBox
            {
                Content = "Minify HTML"
            };

            MinifyHtmlCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    MinifyHtml = ((CheckBox)sender).IsChecked.Value;
                }
            };

            var TheadTbodyCheckBox = new CheckBox
            {
                Content = "Thead & Tbody"
            };

            TheadTbodyCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    TheadTbody = ((CheckBox)sender).IsChecked.Value;
                }
            };

            Controls?.Add(MinifyHtmlCheckBox);
            Controls?.Add(TheadTbodyCheckBox);
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    // Extract content within the <table> tags
                    string table_pattern = @"<table[^>]*>[\s\S]*?</table>";
                    MatchCollection table_matches = Regex.Matches(await reader.ReadToEndAsync(), table_pattern, RegexOptions.IgnoreCase);

                    if (table_matches.Count > 0)
                    {
                        // Assume the first table in the HTML as the target table
                        string table_html = table_matches[0].Value;

                        // Extract data rows
                        string row_pattern = @"<tr[^>]*>[\s\S]*?</tr>";
                        MatchCollection row_matches = Regex.Matches(table_html, row_pattern, RegexOptions.IgnoreCase);

                        foreach (Match row_match in row_matches)
                        {
                            if (row_match.Value.Contains("<th>") && row_match.Value.Contains("</th>"))
                            {
                                // Extract headers from the first row
                                string header_pattern = @"<th[^>]*>[\s\S]*?</th>";
                                MatchCollection header_matches = Regex.Matches(table_html, header_pattern, RegexOptions.IgnoreCase);

                                foreach (Match header_match in header_matches)
                                {
                                    string header_text = Regex.Replace(header_match.Value, "<.*?>", string.Empty);
                                    column_values.Add(header_text.Trim());
                                }
                            }
                            else
                            {
                                List<string> data_row = new List<string>();
                                int column_index = 0;

                                // Extract cells from each row
                                string cell_pattern = @"<t[dh][^>]*>[\s\S]*?</t[dh]>";
                                MatchCollection cell_matches = Regex.Matches(row_match.Value, cell_pattern, RegexOptions.IgnoreCase);

                                foreach (Match cell_match in cell_matches)
                                {
                                    string cell_text = Regex.Replace(cell_match.Value, "<.*?>", string.Empty);
                                    data_row[column_index++] = cell_text.Trim();
                                }

                                row_values.Add(data_row.ToArray());
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
                StringWriter string_writer = new StringWriter();

                int tab_count = 0;

                string_writer.Write($"<table>{(MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tab_count))}");

                if (TheadTbody)
                {
                    string_writer.Write($"<thead>{(MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tab_count))}");
                }

                string_writer.Write($"<tr>{(MinifyHtml ? "" : Environment.NewLine)}");

                tab_count++;

                for (int i = 0; i < column_values.Length; ++i)
                {
                    string_writer.Write($"{(MinifyHtml ? "" : new string('\t', tab_count))}<th>");
                    string_writer.Write(column_values[i]);
                    string_writer.Write($"</th>{(MinifyHtml ? "" : Environment.NewLine)}");
                }

                string_writer.Write($"{(MinifyHtml ? "" : new string('\t', --tab_count))}</tr>");

                if (TheadTbody)
                {
                    string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</thead>");
                }

                for (int i = 0; i < row_values.Length; ++i)
                {
                    if (TheadTbody)
                    {
                        string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count++))}<tbody>");
                    }

                    string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count++))}<tr>");

                    for (int j = 0; j < column_values.Length; ++j)
                    {
                        string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count))}<td>");
                        string_writer.Write(row_values[i][j]);
                        string_writer.Write("</td>");
                    }

                    string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</tr>");

                    if (TheadTbody)
                    {
                        string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</tbody>");
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                }

                string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine)}</table>");

                return string_writer.ToString();
            });
        }
    }
}
