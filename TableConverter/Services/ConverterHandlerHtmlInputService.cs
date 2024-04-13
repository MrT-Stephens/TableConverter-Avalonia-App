using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    internal class ConverterHandlerHtmlInputService : ConverterHandlerInputAbstract
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
                                headers.Add(header_text.Trim());
                            }
                        }
                        else
                        {
                            List<string> data_row = new List<string>();

                            // Extract cells from each row
                            string cell_pattern = @"<t[dh][^>]*>[\s\S]*?</t[dh]>";
                            MatchCollection cell_matches = Regex.Matches(row_match.Value, cell_pattern, RegexOptions.IgnoreCase);

                            foreach (Match cell_match in cell_matches)
                            {
                                string cell_text = Regex.Replace(cell_match.Value, "<.*?>", string.Empty);
                                data_row.Add(cell_text.Trim());
                            }

                            rows.Add(data_row.ToArray());
                        }
                    }
                }

                return (headers, rows);
            });
        }
    }
}
