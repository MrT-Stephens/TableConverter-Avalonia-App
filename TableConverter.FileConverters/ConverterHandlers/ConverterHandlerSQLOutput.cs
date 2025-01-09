using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerSQLOutput : ConverterHandlerOutputAbstract<ConverterHandlerSQLOutputOptions>
{
    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        var sqlBuilder = new StringBuilder();

        var headersText = string.Join(", ", headers.Select(header =>
            $"{Options!.QuoteTypes[Options!.SelectedQuoteType]}" +
            $"{header.Replace(' ', '_')}" +
            $"{(Options!.QuoteTypes[Options!.SelectedQuoteType] == "[" ? "]" : Options!.QuoteTypes[Options!.SelectedQuoteType])}"
        ));

        for (long i = 0; i < rows.LongLength; i++)
        {
            var rowText = string.Join(", ", rows[i].Select(val => $"\'{val.Replace("\'", "\'\'")}\'"));

            if (Options!.InsertMultiRowsAtOnce)
            {
                if (i == 0)
                {
                    sqlBuilder.Append($"INSERT INTO " +
                                      $"{Options!.QuoteTypes[Options!.SelectedQuoteType]}" +
                                      $"{Options!.TableName.Replace(' ', '_')}" +
                                      $"{(Options!.QuoteTypes[Options!.SelectedQuoteType] == "[" ? "]" : Options!.QuoteTypes[Options!.SelectedQuoteType])} " +
                                      $"({headersText}) VALUES{Environment.NewLine} ({rowText})");
                }
                else
                {
                    sqlBuilder.Append($",{Environment.NewLine} ({rowText})");

                    if (i == rows.LongLength - 1) sqlBuilder.Append($";{Environment.NewLine}");
                }
            }
            else
            {
                sqlBuilder.AppendLine($"INSERT INTO " +
                                      $"{Options!.QuoteTypes[Options!.SelectedQuoteType]}" +
                                      $"{Options!.TableName.Replace(' ', '_')}" +
                                      $"{(Options!.QuoteTypes[Options!.SelectedQuoteType] == "[" ? "]" : Options!.QuoteTypes[Options!.SelectedQuoteType])} " +
                                      $"({headersText}) VALUES ({rowText});"
                );
            }
        }

        return Result<string>.Success(sqlBuilder.ToString());
    }
}