using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerLaTexOutput : ConverterHandlerOutputAbstract<ConverterHandlerLaTexOutputOptions>
{
    private readonly char[] _EscapeChars = ['&', '%', '$', '#', '_', '{', '}'];

    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        var stringWriter = new StringWriter();

        if (Options!.MinimalWorkingExample)
        {
            stringWriter.Write("\\documentclass{article}" + Environment.NewLine);
            stringWriter.Write("\\begin{document}" + Environment.NewLine + Environment.NewLine);
        }

        stringWriter.Write("\\begin{table}" + Environment.NewLine);

        string TableAlignGenerator()
        {
            return Options!.SelectedTableAlignment switch
            {
                TextAlignment.Left => "\t\\raggedleft",
                TextAlignment.Center => "\t\\centering",
                TextAlignment.Right => "\t\\raggedright",
                _ => "\t\\raggedleft"
            } + Environment.NewLine;
        }

        string BeginTableGenerator()
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
                    Enumerable.Repeat($"{textAlignementChar}|", headers.Length)),
                ConverterHandlerLaTexOutputOptions.TableTypes.Excel => $"|{textAlignementChar}|" +
                           string.Join("", Enumerable.Repeat($"{textAlignementChar}", headers.Length - 1)) + "|",
                ConverterHandlerLaTexOutputOptions.TableTypes.Horizontal or
                ConverterHandlerLaTexOutputOptions.TableTypes.None or _ => string.Join("",
                    Enumerable.Repeat($"{textAlignementChar}", headers.Length))
            } + "}" + Environment.NewLine;
        }

        string AfterBeginTableGenerator()
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

        string TableHeaderGenerator()
        {
            return GenerateTableRow(headers, Options!.BoldHeader, Options!.BoldFirstColumn) +
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

        string TableRowsGenerator()
        {
            var rowsStringWriter = new StringWriter();

            for (var i = 0; i < rows.Length; i++)
            {
                rowsStringWriter.Write(GenerateTableRow(rows[i], false, Options!.BoldFirstColumn));

                if (Options!.SelectedTableType == ConverterHandlerLaTexOutputOptions.TableTypes.None)
                {
                    rowsStringWriter.Write(Environment.NewLine);
                }
                else if (Options!.SelectedTableType == ConverterHandlerLaTexOutputOptions.TableTypes.All 
                    || i == rows.LongLength - 1)
                {
                    rowsStringWriter.Write(" \\hline" + Environment.NewLine);
                }
                else
                {
                    rowsStringWriter.Write(Environment.NewLine);
                }
            }

            return rowsStringWriter.ToString();
        }

        stringWriter.Write(TableAlignGenerator());

        if (Options!.CaptionName != string.Empty
            && Options!.SelectedCaptionAlignment == ConverterHandlerLaTexOutputOptions.CaptionAlignments.Top)
        {
            stringWriter.Write("\t\\caption{" + Options!.CaptionName + "}" + Environment.NewLine);
        }

        stringWriter.Write(BeginTableGenerator());
        stringWriter.Write(AfterBeginTableGenerator());
        stringWriter.Write(TableHeaderGenerator());
        stringWriter.Write(TableRowsGenerator());

        stringWriter.Write("\t\\end{tabular}" + Environment.NewLine);

        if (Options!.CaptionName != string.Empty 
            && Options!.SelectedCaptionAlignment == ConverterHandlerLaTexOutputOptions.CaptionAlignments.Bottom)
        {
            stringWriter.Write("\t\\caption{" + Options!.CaptionName + "}" + Environment.NewLine);
        }

        if (Options!.LabelName != string.Empty)
        {
            stringWriter.Write("\t\\label{" + Options!.LabelName + "}" + Environment.NewLine);
        }

        stringWriter.Write("\\end{table}" + Environment.NewLine);

        if (Options!.MinimalWorkingExample)
        {
            stringWriter.Write(Environment.NewLine + "\\end{document}" + Environment.NewLine);
        }

        return Result<string>.Success(stringWriter.ToString());
    }

    private string GenerateTableRow(string[] items, bool boldHeader, bool boldColumn)
    {
        var stringWriter = new StringWriter();

        stringWriter.Write("\t\t");

        for (long i = 0; i < items.LongLength; i++)
        {
            if ((i == 0 && boldColumn) || boldHeader)
            {
                stringWriter.Write("\\textbf{" + EscapeLaTexString(items[i]) + "}");
            }
            else
            {
                stringWriter.Write(EscapeLaTexString(items[i]) + "");
            }

            if (i != items.Length - 1)
            {
                stringWriter.Write(" & ");
            }
        }

        stringWriter.Write(" \\\\");

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