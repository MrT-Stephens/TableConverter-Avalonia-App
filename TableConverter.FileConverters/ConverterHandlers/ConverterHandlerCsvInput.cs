﻿using CsvHelper.Configuration;
using System.Globalization;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerCsvInput : ConverterHandlerInputAbstract<ConverterHandlerCsvOptions>
    {
        public override TableData ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            using (var csv_reader = new CsvHelper.CsvReader(new StringReader(text), new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = Options!.Delimiter,
                NewLine = Environment.NewLine,
                BadDataFound = null,
            }))
            {
                foreach (var row in csv_reader.GetRecords<dynamic>())
                {
                    if (Options!.Header && headers.Count == 0)
                    {
                        headers.AddRange(((IDictionary<string, object>)row).Keys.ToList());
                    }
                    else if (!Options!.Header && headers.Count == 0)
                    {
                        for (long i = 0; i < ((IDictionary<string, object>)row).Keys.LongCount(); i++)
                        {
                            headers.Add($"Column {i}");
                        }
                    }

                    rows.Add(((IDictionary<string, object>)row).Select(x => x.Value?.ToString() ?? string.Empty).ToArray());
                }
            }

            return new TableData(headers, rows);
        }
    }
}