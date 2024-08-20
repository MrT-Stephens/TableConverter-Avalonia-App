using System.Text.RegularExpressions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerSQLInput : ConverterHandlerInputAbstract<ConverterHandlerSQLInputOptions>
    {
        public override TableData ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            string sql_regex_w_column_names = @"INSERT\sINTO\s([`""\[]?\w+[`""\]]?)\s*\((.*?)\)\s*VALUES\s*\((.*?)\);";
            string sql_regex_wo_column_names = @"INSERT\sINTO\s([`""\[]?\w+[`""\]]?)\s*VALUES\s*\((.*?)\);";

            if (Options!.HasColumnNames)
            {
                MatchCollection matches = Regex.Matches(text, sql_regex_w_column_names, RegexOptions.Singleline);

                bool first_loop = true;

                foreach (Match match in matches.Cast<Match>())
                {
                    string[] columns = match.Groups[2].Value.Split(',');
                    string[] values = match.Groups[3].Value.Split(',');

                    if (first_loop)
                    {
                        first_loop = false;

                        for (long i = 0; i < columns.Length; i++)
                        {
                            if (columns[i].StartsWith(Options!.SelectedQuoteType) && columns[i].EndsWith(Options!.SelectedQuoteType == "[" ? "]" : Options!.SelectedQuoteType))
                            {
                                columns[i] = columns[i].Substring(1, columns[i].Length - 2);
                                headers.Add(columns[i].Trim());
                            }
                            else
                            {
                                headers.Add(columns[i].Trim());
                            }
                        }
                    }

                    rows.Add(values.Select(value => value.Trim().Trim('\'')).ToArray());
                }
            }
            else
            {
                MatchCollection matches = Regex.Matches(text, sql_regex_wo_column_names, RegexOptions.Singleline);

                foreach (Match match in matches.Cast<Match>())
                {
                    string[] values = match.Groups[2].Value.Split(',');

                    rows.Add(values.Select(value => value.Trim().Trim('\'')).ToArray());
                }

                headers.AddRange(Enumerable.Range(1, rows[0].Length).Select(i => $"Column {i}"));
            }

            return new TableData(headers, rows);
        }
    }
}
