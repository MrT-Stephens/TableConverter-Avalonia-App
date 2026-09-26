using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerLaTexOutput : ConverterHandlerOutputAbstract<ConverterHandlerLaTexOutputOptions>
{
    private readonly char[] _EscapeChars = ['&', '%', '$', '#', '_', '{', '}'];

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

        if (Options!.MinimalWorkingExample)
        {
            writer.Write("\\documentclass{article}" + Environment.NewLine);
            writer.Write("\\begin{document}" + Environment.NewLine + Environment.NewLine);
        }

        writer.Write("\\begin{table}" + Environment.NewLine);
        writer.Write(TableAlignGenerator());

        if (Options!.CaptionName != string.Empty
            && Options!.SelectedCaptionAlignment == ConverterHandlerLaTexOutputOptions.CaptionAlignments.Top)
        {
            writer.Write("\t\\caption{" + Options!.CaptionName + "}" + Environment.NewLine);
        }

        writer.Write(BeginTableGenerator(headers.Count));
        writer.Write(AfterBeginTableGenerator());
        writer.Write(TableHeaderGenerator(headers));

        // The separator that follows a row depends on whether it is the last one, which is only known
        // once the rows run out, so the separator for the previous row is written before the next one.
        var wroteAny = false;

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            if (wroteAny) writer.Write(RowSeparator(isLastRow: false));

            writer.Write(GenerateTableRow(row, headers.Count, false, Options!.BoldFirstColumn));

            wroteAny = true;
        }

        if (wroteAny) writer.Write(RowSeparator(isLastRow: true));

        writer.Write("\t\\end{tabular}" + Environment.NewLine);

        if (Options!.CaptionName != string.Empty
            && Options!.SelectedCaptionAlignment == ConverterHandlerLaTexOutputOptions.CaptionAlignments.Bottom)
        {
            writer.Write("\t\\caption{" + Options!.CaptionName + "}" + Environment.NewLine);
        }

        if (Options!.LabelName != string.Empty)
        {
            writer.Write("\t\\label{" + Options!.LabelName + "}" + Environment.NewLine);
        }

        writer.Write("\\end{table}" + Environment.NewLine);

        if (Options!.MinimalWorkingExample)
        {
            writer.Write(Environment.NewLine + "\\end{document}" + Environment.NewLine);
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private string TableAlignGenerator()
    {
        return Options!.SelectedTableAlignment switch
        {
            TextAlignment.Left => "\t\\raggedleft",
            TextAlignment.Center => "\t\\centering",
            TextAlignment.Right => "\t\\raggedright",
            _ => "\t\\raggedleft"
        } + Environment.NewLine;
    }

    private string BeginTableGenerator(int columnCount)
    {
        var textAlignementChar = Options!.SelectedTextAlignment switch
        {
            TextAlignment.Left => "l",
            TextAlignment.Center => "c",
            TextAlignment.Right => "r",
            _ => "l"
        };

        return "\t\\begin{tabular}{" + Options!.SelectedTableType switch
        {
            ConverterHandlerLaTexOutputOptions.TableTypes.All or
            ConverterHandlerLaTexOutputOptions.TableTypes.MySQL or
            ConverterHandlerLaTexOutputOptions.TableTypes.Markdown => "|" + string.Join("",
                Enumerable.Repeat($"{textAlignementChar}|", columnCount)),
            ConverterHandlerLaTexOutputOptions.TableTypes.Excel => $"|{textAlignementChar}|" +
                       string.Join("", Enumerable.Repeat($"{textAlignementChar}", columnCount - 1)) + "|",
            ConverterHandlerLaTexOutputOptions.TableTypes.Horizontal or
            ConverterHandlerLaTexOutputOptions.TableTypes.None or _ => string.Join("",
                Enumerable.Repeat($"{textAlignementChar}", columnCount))
        } + "}" + Environment.NewLine;
    }

    private string AfterBeginTableGenerator()
    {
        return Options!.SelectedTableType switch
        {
            ConverterHandlerLaTexOutputOptions.TableTypes.All or
            ConverterHandlerLaTexOutputOptions.TableTypes.MySQL or
            ConverterHandlerLaTexOutputOptions.TableTypes.Excel or
            ConverterHandlerLaTexOutputOptions.TableTypes.Horizontal => "\t\\hline" + Environment.NewLine,
            ConverterHandlerLaTexOutputOptions.TableTypes.Markdown or
            ConverterHandlerLaTexOutputOptions.TableTypes.None or _ => string.Empty
        };
    }

    private string TableHeaderGenerator(IReadOnlyList<string> headers)
    {
        return GenerateTableRow(headers, headers.Count, Options!.BoldHeader, Options!.BoldFirstColumn) +
               Options!.SelectedTableType switch
               {
                   ConverterHandlerLaTexOutputOptions.TableTypes.All or
                   ConverterHandlerLaTexOutputOptions.TableTypes.MySQL or
                   ConverterHandlerLaTexOutputOptions.TableTypes.Excel or
                   ConverterHandlerLaTexOutputOptions.TableTypes.Horizontal or
                   ConverterHandlerLaTexOutputOptions.TableTypes.Markdown => " \\hline",
                   ConverterHandlerLaTexOutputOptions.TableTypes.None or _ => string.Empty
               } + Environment.NewLine;
    }

    /// <summary>
    ///     The text that follows a row, which differs between the last row and the ones before it for
    ///     every table style but <see cref="ConverterHandlerLaTexOutputOptions.TableTypes.All" /> and
    ///     <see cref="ConverterHandlerLaTexOutputOptions.TableTypes.None" />.
    /// </summary>
    private string RowSeparator(bool isLastRow)
    {
        return Options!.SelectedTableType switch
        {
            ConverterHandlerLaTexOutputOptions.TableTypes.None => Environment.NewLine,
            ConverterHandlerLaTexOutputOptions.TableTypes.All => " \\hline" + Environment.NewLine,
            _ => isLastRow ? " \\hline" + Environment.NewLine : Environment.NewLine
        };
    }

    private string GenerateTableRow(IReadOnlyList<string> items, int columnCount, bool boldHeader, bool boldColumn)
    {
        using var stringWriter = new StringWriter();

        stringWriter.Write("\t\t");

        for (var i = 0; i < columnCount; i++)
        {
            // Iterate the headers so a ragged row is padded rather than producing too few cells.
            var value = i < items.Count ? items[i] : string.Empty;

            if ((i == 0 && boldColumn) || boldHeader)
            {
                stringWriter.Write("\\textbf{" + EscapeLaTexString(value) + "}");
            }
            else
            {
                stringWriter.Write(EscapeLaTexString(value) + "");
            }

            if (i != columnCount - 1)
            {
                stringWriter.Write(" & ");
            }
        }

        stringWriter.Write(@" \\");

        return stringWriter.ToString();
    }

    private string EscapeLaTexString(string text)
    {
        foreach (var character in _EscapeChars)
        {
            if (text.Contains(character))
            {
                text = text.Replace(character.ToString(), "\\" + character);
            }
        }

        return text;
    }
}