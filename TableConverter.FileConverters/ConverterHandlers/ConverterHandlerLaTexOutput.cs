using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerLaTexOutput : ConverterHandlerOutputAbstract<ConverterHandlerLaTexOutputOptions>
    {
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
                    "Left" => "\t\\raggedleft",
                    "Center" => "\t\\centering",
                    "Right" => "\t\\raggedright",
                    _ => "\t\\raggedleft"
                } + Environment.NewLine;
            }

            string BeginTableGenerator()
            {
                var textAlignementChar = Options!.SelectedTextAlignment switch
                {
                    "Left" => "l",
                    "Center" => "c",
                    "Right" => "r",
                    _ => "l"
                };

                return "\t\\begin{tabular}{" + Options!.SelectedTableType switch
                {
                    "All" or "MySQL" or "Markdown" => "|" + string.Join("", Enumerable.Repeat($"{textAlignementChar}|", headers.Length)),
                    "Excel" => $"|{textAlignementChar}|" + string.Join("", Enumerable.Repeat($"{textAlignementChar}", headers.Length - 1)) + "|",
                    "Horizontal" or "None" or _ => string.Join("", Enumerable.Repeat($"{textAlignementChar}", headers.Length)),
                } + "}" + Environment.NewLine;
            }

            string AfterBeginTableGenerator()
            {
                return Options!.SelectedTableType switch
                {
                    "All" or "MySQL" or "Excel" or "Horizontal" => "\t\\hline" + Environment.NewLine,
                    "Markdown" or "None" or _ => string.Empty
                };
            }

            string TableHeaderGenerator()
            {
                return GenerateTableRow(headers, Options!.BoldHeader, Options!.BoldFirstColumn) + Options!.SelectedTableType switch
                {
                    "All" or "MySQL" or "Excel" or "Horizontal" or "Markdown" => " \\hline",
                    "None" or _ => string.Empty
                } + Environment.NewLine;
            }

            string TableRowsGenerator()
            {
                var rowsStringWriter = new StringWriter();

                for (var i = 0; i < rows.Length; i++)
                {
                    rowsStringWriter.Write(GenerateTableRow(rows[i], false, Options!.BoldFirstColumn));

                    if (Options!.SelectedTableType == "None")
                    {
                        rowsStringWriter.Write(Environment.NewLine);
                    }
                    else if (Options!.SelectedTableType == "All" || i == rows.LongLength - 1)
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

            if (Options!.CaptionName != string.Empty && Options!.SelectedCaptionAlignment == "Top")
            {
                stringWriter.Write("\t\\caption{" + Options!.CaptionName + "}" + Environment.NewLine);
            }

            stringWriter.Write(BeginTableGenerator());
            stringWriter.Write(AfterBeginTableGenerator());
            stringWriter.Write(TableHeaderGenerator());
            stringWriter.Write(TableRowsGenerator());

            stringWriter.Write("\t\\end{tabular}" + Environment.NewLine);

            if (Options!.CaptionName != string.Empty && Options!.SelectedCaptionAlignment == "Bottom")
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

        private static string GenerateTableRow(string[] items, bool boldHeader, bool boldColumn)
        {
            var stringWriter = new StringWriter();

            stringWriter.Write("\t\t");

            for (long i = 0; i < items.LongLength; i++)
            {
                if ((i == 0 && boldColumn) || boldHeader)
                {
                    stringWriter.Write("\\textbf{" + items[i] + "}");
                }
                else
                {
                    stringWriter.Write(items[i] + "");
                }

                if (i != items.Length - 1)
                {
                    stringWriter.Write(" & ");
                }
            }

            stringWriter.Write(" \\\\");

            return stringWriter.ToString();
        }
    }
}
