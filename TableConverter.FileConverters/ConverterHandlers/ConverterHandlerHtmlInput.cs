using System.Text.RegularExpressions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public partial class ConverterHandlerHtmlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        try
        {
            if (ValidateHtmlTags(text) is { IsSuccess: false } result)
                return Result<TableData>.Failure($"Invalid HTML tags. {result.Error}");

            // Extract content within the <table> tags
            var tableMatches = TableRegex().Matches(text);

            if (tableMatches.Count > 0)
            {
                // Assume the first table in the HTML as the target table
                var tableHtml = tableMatches[0].Value;

                // Extract data rows
                var rowMatches = RowRegex().Matches(tableHtml);

                foreach (var rowMatch in rowMatches.Cast<Match>())
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
                            if (item != "")
                            {
                                rows.Add(dataRow.ToArray());
                                break;
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

    // Helper function to validate HTML tags
    private static Result ValidateHtmlTags(string html)
    {
        var stack = new Stack<string>();
        var regex = ValidateHtmlTagsRegex();
        var matches = regex.Matches(html);

        // List of table-related tags to validate
        var tableTags = new HashSet<string> { "table", "tr", "th", "td", "tbody", "thead", "tfoot" };

        foreach (Match match in matches)
        {
            var tagName = match.Groups[1].Value.ToLower();
            var tagPosition = match.Index; // Position of the tag in the HTML string

            // Process only table-related tags using the pre-defined regex
            if (!tableTags.Contains(tagName)) continue;

            // Get a snippet of the surrounding text to include in the error for context
            var contextSnippet = GetContextSnippet(html, tagPosition);

            if (match.Value.StartsWith("</")) // Closing tag
            {
                if (stack.Count == 0 || stack.Peek() != tagName)
                    // Found a closing tag without a matching opening tag
                    return Result.Failure(
                        $"Unexpected closing tag: </{tagName}> at position {tagPosition}. Context: \"{contextSnippet}\". Stack: \"{string.Join(", ", stack)}\"");

                stack.Pop(); // Remove matching opening tag
            }
            else if (!match.Value.EndsWith("/>")) // Opening tag
            {
                stack.Push(tagName); // Track opening tag
            }
        }

        if (stack.Count > 0)
            // There are unmatched opening tags remaining
            return Result.Failure($"Unclosed tag: <{stack.Peek()}>. Stack: {string.Join(", ", stack)}");

        return Result.Success();
    }

    // Helper method to get a snippet of the surrounding context (for better error messages)
    private static string GetContextSnippet(string html, int position, int contextLength = 30)
    {
        // Get the substring around the tag for context (before and after the tag)
        var start = Math.Max(position - contextLength, 0);
        var end = Math.Min(position + contextLength, html.Length);

        return html.Substring(start, end - start).Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t").Trim();
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

    [GeneratedRegex(@"</?([a-zA-Z0-9]+)[^>]*>", RegexOptions.IgnoreCase, "en-GB")]
    private static partial Regex ValidateHtmlTagsRegex();
}