using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerJsonLinesInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
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
        var firstLine = true;
        var lineNumber = 0;
        var begun = false;

        for (var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
             line is not null;
             line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))
        {
            lineNumber++;
            line = line.Trim(); // Clean up whitespace

            if (string.IsNullOrEmpty(line))
                continue;

            // Validate the schema for the current line
            if (ValidateJsonSchema(line, lineNumber) is { IsSuccess: false } validationResult)
                return Result.Failure($"Error in line {lineNumber}: {line} Details: {validationResult.Error}");

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

                if (!begun)
                {
                    await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                    begun = true;
                }

                await sink.WriteRowAsync(row.ToArray(), cancellationToken).ConfigureAwait(false);
            }
            else if (line.StartsWith('[') && line.EndsWith(']'))
            {
                var strings = line.Trim('[', ']').Split(',');

                if (firstLine)
                {
                    headers.AddRange(strings.Select(str => str.Trim().Trim('"')));
                }
                else
                {
                    if (!begun)
                    {
                        await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                        begun = true;
                    }

                    await sink.WriteRowAsync(strings.Select(str => str.Trim().Trim('"')).ToArray(), cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            // A line that is neither an object nor an array still fixes the header row, so the columns
            // declared by this first line have to be handed to the sink before any row can be written.
            if (!begun)
            {
                await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                begun = true;
            }

            firstLine = false;
        }

        if (!begun)
        {
            await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
        }

        await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
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