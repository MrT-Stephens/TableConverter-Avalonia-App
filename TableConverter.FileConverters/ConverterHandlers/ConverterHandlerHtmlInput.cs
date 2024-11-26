using System.Text.RegularExpressions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public partial class ConverterHandlerHtmlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        public override Result<TableData> ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            try
            {
                // Extract content within the <table> tags
                var tableMatches = TableRegex().Matches(text);

                if (tableMatches.Count > 0)
                {
                    // Assume the first table in the HTML as the target table
                    var tableHtml = tableMatches[0].Value;

                    // Extract data rows
                    var rowMatches = RowRegex().Matches(tableHtml);

                    foreach (var rowMatch in rowMatches.Cast<Match>())
                    {
                        if (rowMatch.Value.Contains("<th>") && rowMatch.Value.Contains("</th>"))
                        {
                            // Extract headers from the first row
                            var headerMatches = HeadersRegex().Matches(tableHtml);

                            headers.AddRange(headerMatches
                                .Select(headerMatch => HeaderTagsRegex().Replace(headerMatch.Value, string.Empty))
                                .Select(headerText => headerText.Trim()));
                        }
                        else
                        {
                            List<string> dataRow = [];

                            // Extract cells from each row
                            var cellMatches = CellRegex().Matches(rowMatch.Value);

                            dataRow.AddRange(cellMatches
                                .Select(cellMatch => CellTagsRegex().Replace(cellMatch.Value, string.Empty))
                                .Select(cellText => cellText.Trim()));

                            foreach (var item in dataRow)
                            {
                                if (item != "")
                                {
                                    rows.Add(dataRow.ToArray());
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Result<TableData>.Failure(ex.Message);
            }

            return Result<TableData>.Success(new TableData(headers, rows));
        }

        [GeneratedRegex(@"<tr[^>]*>[\s\S]*?</tr>", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex RowRegex();
        
        [GeneratedRegex(@"<table[^>]*>[\s\S]*?</table>", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex TableRegex();
        
        [GeneratedRegex(@"<th[^>]*>[\s\S]*?</th>", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex HeadersRegex();
        
        [GeneratedRegex("<.*?>")]
        private static partial Regex HeaderTagsRegex();
        
        [GeneratedRegex(@"<t[dh][^>]*>[\s\S]*?</t[dh]>", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex CellRegex();
        
        [GeneratedRegex("<.*?>")]
        private static partial Regex CellTagsRegex();
    }
}
