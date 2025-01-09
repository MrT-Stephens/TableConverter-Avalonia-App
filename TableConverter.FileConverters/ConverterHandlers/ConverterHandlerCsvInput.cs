using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerCsvInput : ConverterHandlerInputAbstract<ConverterHandlerCsvOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        try
        {
            using var csvReader = new CsvReader(new StringReader(text),
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = Options!.Delimiter,
                    NewLine = Environment.NewLine,
                    DetectColumnCountChanges = true,
                    TrimOptions = TrimOptions.Trim
                });

            foreach (var row in csvReader.GetRecords<dynamic>())
            {
                if (Options!.Header && headers.Count == 0)
                    headers.AddRange(((IDictionary<string, object>)row).Keys.ToList());
                else if (!Options!.Header && headers.Count == 0)
                    for (long i = 0; i < ((IDictionary<string, object>)row).Keys.Count; i++)
                        headers.Add($"Column {i}");

                rows.Add(((IDictionary<string, object>)row).Select(x => x.Value?.ToString() ?? string.Empty)
                    .ToArray());
            }
        }
        catch (Exception ex)
        {
            return Result<TableData>.Failure(ex.Message);
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }
}