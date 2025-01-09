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
                    var columns = match.Groups[2].Value.Split(',');
                    var values = match.Groups[3].Value.Split(',');

                    if (firstLoop)
                    {
                        firstLoop = false;

                        for (long i = 0; i < columns.Length; i++)
                        {
                            if (columns[i].StartsWith(Options!.SelectedQuoteType) && columns[i]
                                    .EndsWith(Options!.SelectedQuoteType == "[" ? "]" : Options!.SelectedQuoteType))
                                columns[i] = columns[i].Substring(1, columns[i].Length - 2);

                            headers.Add(columns[i].Trim());
                        }
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