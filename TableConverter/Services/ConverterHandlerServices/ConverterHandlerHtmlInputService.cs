using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerHtmlInputService : ConverterHandlerInputAbstract
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

                // Extract content within the <table> tags
                string table_pattern = @"<table[^>]*>[\s\S]*?</table>";
                MatchCollection table_matches = Regex.Matches(text, table_pattern, RegexOptions.IgnoreCase);

                if (table_matches.Count > 0)
                {
                    // Assume the first table in the HTML as the target table
                    string table_html = table_matches[0].Value;

                    // Extract data rows
                    string row_pattern = @"<tr[^>]*>[\s\S]*?</tr>";
                    MatchCollection row_matches = Regex.Matches(table_html, row_pattern, RegexOptions.IgnoreCase);

                    foreach (Match row_match in row_matches.Cast<Match>())
                    {
                        if (row_match.Value.Contains("<th>") && row_match.Value.Contains("</th>"))
                        {
                            // Extract headers from the first row
                            string header_pattern = @"<th[^>]*>[\s\S]*?</th>";
                            MatchCollection header_matches = Regex.Matches(table_html, header_pattern, RegexOptions.IgnoreCase);

                            foreach (Match header_match in header_matches.Cast<Match>())
                            {
                                string header_text = Regex.Replace(header_match.Value, "<.*?>", string.Empty);
                                headers.Add(header_text.Trim());
                            }
                        }
                        else
                        {
                            List<string> data_row = [];

                            // Extract cells from each row
                            string cell_pattern = @"<t[dh][^>]*>[\s\S]*?</t[dh]>";
                            MatchCollection cell_matches = Regex.Matches(row_match.Value, cell_pattern, RegexOptions.IgnoreCase);

                            foreach (Match cell_match in cell_matches.Cast<Match>())
                            {
                                string cell_text = Regex.Replace(cell_match.Value, "<.*?>", string.Empty);
                                data_row.Add(cell_text.Trim());
                            }

                            foreach (var item in data_row)
                            {
                                if (item != "")
                                {
                                    rows.Add(data_row.ToArray());
                                    break;
                                }
                            }
                        }
                    }
                }

                return new TableData(headers, rows);
            });
        }
    }
}
