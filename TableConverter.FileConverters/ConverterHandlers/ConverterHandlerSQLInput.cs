using System.Text.RegularExpressions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public partial class ConverterHandlerSQLInput : ConverterHandlerInputAbstract<ConverterHandlerSQLInputOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        try
        {
            if (Options!.HasColumnNames)
            {
                var matches = SqlWithColumnNamesRegex().Matches(text);

                var firstLoop = true;

                foreach (var match in matches.Cast<Match>())
                {
                    var tableName = match.Groups[1].Value;
                    var columns = match.Groups[2].Value.Split(',');
                    var values = match.Groups[3].Value.Split(',');

                    foreach (var quoteType in Options!.QuoteTypes.Skip(1))
                    {
                        if (tableName.StartsWith(quoteType.Value) && tableName.EndsWith(quoteType.Value) &&
                            quoteType.Key != Options!.SelectedQuoteType)
                        {
                            return Result<TableData>.Failure(
                                $"The table name is enclosed in {quoteType.Key} but the selected quote type is {Options!.SelectedQuoteType}.");
                        }
                    }

                    if (firstLoop)
                    {
                        firstLoop = false;

                        for (long i = 0; i < columns.Length; i++)
                        {
                            headers.Add(columns[i].Trim()
                                .TrimStart(Options!.QuoteTypes[Options!.SelectedQuoteType].ToCharArray())
                                .TrimEnd((Options!.QuoteTypes[Options!.SelectedQuoteType] == "["
                                    ? "]"
                                    : Options!.QuoteTypes[Options!.SelectedQuoteType]).ToCharArray()));
                        }
                    }

                    if (columns.Length != values.Length)
                    {
                        return Result<TableData>.Failure(
                            $"The number of columns and values do not match at row {rows.Count + 1}.");
                    }

                    rows.Add(values.Select(value => value.Trim().Trim('\'')).ToArray());
                }
            }
            else
            {
                var matches = SqlRegex().Matches(text);

                rows.AddRange(matches.Select(match => match.Groups[2].Value.Split(','))
                    .Select(values => values.Select(value => value.Trim().Trim('\'')).ToArray()));

                headers.AddRange(Enumerable.Range(1, rows[0].Length).Select(i => $"Column {i}"));
            }
        }
        catch (Exception ex)
        {
            return Result<TableData>.Failure(ex.Message);
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }

    [GeneratedRegex("""INSERT\sINTO\s([`"\[]?\w+[`"\]]?)\s*\((.*?)\)\s*VALUES\s*\((.*?)\);""", RegexOptions.Singleline)]
    private static partial Regex SqlWithColumnNamesRegex();

    [GeneratedRegex("""INSERT\sINTO\s([`"\[]?\w+[`"\]]?)\s*VALUES\s*\((.*?)\);""", RegexOptions.Singleline)]
    private static partial Regex SqlRegex();
}