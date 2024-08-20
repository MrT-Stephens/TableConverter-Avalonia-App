using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerMultiLineInput : ConverterHandlerInputAbstract<ConverterHandlerMultiLineOptions>
    {
        public override TableData ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            using (var reader = new StringReader(text))
            {
                bool first_line = true;

                List<string> row = new List<string>();

                for (string? line = reader?.ReadLine()?.Trim();
                        !string.IsNullOrEmpty(line);
                        line = reader?.ReadLine()?.Trim())
                {
                    if (line == Options!.RowSeparator)
                    {
                        if (first_line)
                        {
                            first_line = false;
                        }
                        else
                        {
                            rows.Add(row.ToArray());
                            row.Clear();
                        }
                    }
                    else if (first_line)
                    {
                        headers.Add(line);
                    }
                    else
                    {
                        row.Add(line);
                    }
                }

                if (row.Count > 0)
                {
                    rows.Add(row.ToArray());
                }
            }

            return new TableData(headers, rows);
        }
    }
}
