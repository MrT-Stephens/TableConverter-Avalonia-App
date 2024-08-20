using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerSQLOutput : ConverterHandlerOutputAbstract<ConverterHandlerSQLOutputOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            var sql_builder = new StringBuilder();

            var headers_text = string.Join(", ", headers.Select(header =>
                    $"{Options!.QuoteTypes[Options!.SelectedQuoteType]}" +
                    $"{header.Replace(' ', '_')}" +
                    $"{(Options!.QuoteTypes[Options!.SelectedQuoteType] == "[" ? "]" : Options!.QuoteTypes[Options!.SelectedQuoteType])}"
                    ));

            for (long i = 0; i < rows.LongLength; i++)
            {
                var row_text = string.Join(", ", rows[i].Select(val => $"\'{val.Replace("\'", "\'\'")}\'"));

                if (Options!.InsertMultiRowsAtOnce)
                {
                    if (i == 0)
                    {
                        sql_builder.Append($"INSERT INTO " +
                                $"{Options!.QuoteTypes[Options!.SelectedQuoteType]}" +
                                $"{Options!.TableName.Replace(' ', '_')}" +
                                $"{(Options!.QuoteTypes[Options!.SelectedQuoteType] == "[" ? "]" : Options!.QuoteTypes[Options!.SelectedQuoteType])} " +
                                $"({headers_text}) VALUES{Environment.NewLine} ({row_text})");
                    }
                    else
                    {
                        sql_builder.Append($",{Environment.NewLine} ({row_text})");

                        if (i == rows.LongLength - 1)
                        {
                            sql_builder.Append($";{Environment.NewLine}");
                        }
                    }
                }
                else
                {
                    sql_builder.AppendLine($"INSERT INTO " +
                            $"{Options!.QuoteTypes[Options!.SelectedQuoteType]}" +
                            $"{Options!.TableName.Replace(' ', '_')}" +
                            $"{(Options!.QuoteTypes[Options!.SelectedQuoteType] == "[" ? "]" : Options!.QuoteTypes[Options!.SelectedQuoteType])} " +
                            $"({headers_text}) VALUES ({row_text});"
                            );
                }
            }

            return sql_builder.ToString();
        }
    }
}
