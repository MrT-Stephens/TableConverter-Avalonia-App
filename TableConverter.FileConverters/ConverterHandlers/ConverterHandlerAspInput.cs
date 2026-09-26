using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerAspInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
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
        var rows = new List<string[]>();

        try
        {
            // The file declares the size of the array up front and then fills the cells by index in
            // whatever order they appear, so the rows are gathered before they can be handed on.
            var firstLine = true;
            long columnsCount = 0, rowsCount = 0;
            long parsedRows = 0;

            for (var line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim();
                 !string.IsNullOrEmpty(line);
                 line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim())
            {
                parsedRows++;

                if (firstLine)
                {
                    line = line.Replace("Dim arr(", "").Replace(")", "");

                    var values = line.Split(",");

                    columnsCount = long.Parse(values[0]);
                    rowsCount = long.Parse(values[1]);

                    for (long i = 0; i < columnsCount; i++) headers.Add(string.Empty);

                    for (long i = 0; i < rowsCount - 1; i++) rows.Add(new string[columnsCount]);

                    firstLine = false;
                }
                else
                {
                    line = line.Replace("arr(", "").Replace(")", "");

                    var indexes = line.Substring(0, line.IndexOf('=')).Trim().Split(',');

                    if (long.Parse(indexes[0]) < columnsCount && long.Parse(indexes[1]) < rowsCount)
                    {
                        if (int.Parse(indexes[1]) == 0)
                        {
                            headers[int.Parse(indexes[0])] = line.Substring(line.IndexOf('=') + 1).Trim();
                        }
                        else
                        {
                            var index1 = int.Parse(indexes[1]) - 1;
                            var index2 = int.Parse(indexes[0]);

                            rows[index1][index2] = line.Substring(line.IndexOf('=') + 1).Trim();
                        }
                    }
                }
            }

            if (columnsCount != 0 && rowsCount != 0 && parsedRows / columnsCount < rowsCount &&
                parsedRows / rowsCount < columnsCount)
            {
                return Result.Failure("Incorrect number of rows of data in the file");
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return await TableRowStream.PushAsync(headers, rows, sink, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}