using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerJsonLinesInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        using (var reader = new StringReader(text))
        {
            var firstLine = true;
            int lineNumber = 0;

            for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
            {
                lineNumber++;
                line = line.Trim(); // Clean up whitespace

                if (string.IsNullOrEmpty(line))
                    continue;

                // Validate the schema for the current line
                if (ValidateJsonSchema(line, lineNumber) is { IsSuccess: false } validationResult)
                    return Result<TableData>.Failure($"Error in line {lineNumber}: {line} Details: {validationResult.Error}");

                if (line.StartsWith('{') && line.EndsWith('}'))
                {
                    var row = new List<string>();
                    var strings = line.Trim('{', '}').Split(',');

                    foreach (var str in strings)
                    {
                        if (string.IsNullOrEmpty(str)) continue;

                        var keyValue = str.Split(':');

                        if (firstLine) headers.Add(keyValue[0].Trim().Trim('"'));

                        row.Add(keyValue[1].Trim().Trim('"'));
                    }

                    rows.Add(row.ToArray());
                }
                else if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    var strings = line.Trim('[', ']').Split(',');

                    if (firstLine)
                        headers.AddRange(strings.Select(str => str.Trim().Trim('"')));
                    else
                        rows.Add(strings.Select(str => str.Trim().Trim('"')).ToArray());
                }

                firstLine = false;
            }
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }

    private static Result ValidateJsonSchema(string line, int lineNumber)
    {
        var stack = new Stack<(char Bracket, int Index)>();

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '{' || ch == '[')
            {
                stack.Push((ch, i));
            }
            else if (ch == '}' || ch == ']')
            {
                if (stack.Count == 0)
                    return Result.Failure(
                        $"Unexpected closing bracket '{ch}' at character {i + 1} (line {lineNumber}).");

                var (openBracket, openIndex) = stack.Pop();
                if ((ch == '}' && openBracket != '{') || (ch == ']' && openBracket != '['))
                    return Result.Failure(
                        $"Mismatched brackets: '{openBracket}' at character {openIndex + 1} and '{ch}' at character {i + 1} (line {lineNumber}).");
            }
        }

        if (stack.Count > 0)
        {
            var (unmatchedBracket, unmatchedIndex) = stack.Peek();
            return Result.Failure(
                $"Unclosed bracket '{unmatchedBracket}' at character {unmatchedIndex + 1} (line {lineNumber}).");
        }

        return Result.Success();
    }
}