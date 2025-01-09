using System.Dynamic;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerCsvOutput : ConverterHandlerOutputAbstract<ConverterHandlerCsvOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        using var writer = new StringWriter();

        try
        {
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = Options!.Delimiter,
                NewLine = Environment.NewLine,
                HasHeaderRecord = Options!.Header
            });
            {
                List<object> records = [];

                for (long i = 0; i < rows.LongLength; i++)
                {
                    dynamic record = new ExpandoObject();

                    for (long j = 0; j < headers.LongLength; j++)
                        ((IDictionary<string, object>)record)[headers[j]] = rows[i][j];

                    records.Add(record);
                }

                csv.WriteRecords(records);
            }
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }

        return Result<string>.Success(writer.ToString());
    }
}