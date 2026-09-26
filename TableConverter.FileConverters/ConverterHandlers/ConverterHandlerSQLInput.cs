using System.Text.RegularExpressions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public partial class ConverterHandlerSQLInput : ConverterHandlerInputAbstract<ConverterHandlerSQLInputOptions>
{
    public override async Task<Result> ReadStreamAsync(
        Stream? stream,
        ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var opened = TableRowStream.OpenTextReader(stream);

        if (opened.IsSuccess is false)
        {
            return Result.Failure(opened.Error!);
        }

        using var reader = opened.Value;

        var headers = new List<string>();
        var rows = new List<string[]>();

        try
        {
            // The INSERT statements are pulled out with a regex, so the whole file is needed before the
            // first row can be written.
            var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

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
                            return Result.Failure(
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
                        return Result.Failure(
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
            return Result.Failure(ex.Message);
        }

        return await TableRowStream.PushAsync(headers, rows, sink, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    [GeneratedRegex("""INSERT\sINTO\s([`"\[]?\w+[`"\]]?)\s*\((.*?)\)\s*VALUES\s*\((.*?)\);""", RegexOptions.Singleline)]
    private static partial Regex SqlWithColumnNamesRegex();

    [GeneratedRegex("""INSERT\sINTO\s([`"\[]?\w+[`"\]]?)\s*VALUES\s*\((.*?)\);""", RegexOptions.Singleline)]
    private static partial Regex SqlRegex();
}