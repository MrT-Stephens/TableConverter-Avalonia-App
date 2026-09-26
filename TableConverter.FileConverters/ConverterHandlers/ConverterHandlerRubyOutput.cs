using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerRubyOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
{
    public override async Task<Result> ConvertToStreamAsync(
        Stream? stream,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(source);

        await using var writer = TableRowStream.CreateTextWriter(stream);

        var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

        writer.Write("[");
        writer.Write(Environment.NewLine);

        WriteRubyArray(writer, headers);

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            WriteRubyArray(writer, row);

        writer.Write("];");
        writer.Write(Environment.NewLine);

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static void WriteRubyArray(TextWriter writer, IReadOnlyList<string> values)
    {
        writer.Write("\t{");

        for (var i = 0; i < values.Count; i++)
        {
            writer.Write($"\"val{i}\"=>\"{values[i]}\"");

            if (i < values.Count - 1) writer.Write(",");
        }

        writer.Write("}");
        writer.Write(Environment.NewLine);
    }
}