using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerJsonLinesInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
    {
        public override Result<TableData> ReadText(string text)
        {
            var headers = new List<string>();
            var rows = new List<string[]>();

            using (var reader = new StringReader(text))
            {
                var firstLine = true;

                for (var line = reader.ReadLine(); !string.IsNullOrEmpty(line); line = reader.ReadLine())
                { 
                    if (line is not null && line.StartsWith('{') && line.EndsWith('}'))
                    {
                        var row = new List<string>();
                        var strings = line.Trim('{', '}').Split(',');

                        foreach (var str in strings)
                        {
                            if (string.IsNullOrEmpty(str))
                            {
                                continue;
                            }

                            var keyValue = str.Split(':');

                            if (firstLine)
                            {
                                headers.Add(keyValue[0].Trim().Trim('"'));
                            }

                            row.Add(keyValue[1].Trim().Trim('"'));
                        }

                        rows.Add(row.ToArray());
                    }
                    else if (line is not null)
                    {
                        var strings = line.Trim('[', ']').Split(',');

                        if (firstLine)
                        {
                            headers.AddRange(strings.Select(str => str.Trim().Trim('"')));
                        }
                        else
                        {
                            rows.Add(strings.Select(str => str.Trim().Trim('"')).ToArray());
                        }
                    }

                    firstLine = false;
                }
            }

            return Result<TableData>.Success(new TableData(headers, rows));
        }
    }
}
