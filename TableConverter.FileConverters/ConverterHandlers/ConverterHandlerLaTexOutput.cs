using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerLaTexOutput : ConverterHandlerOutputAbstract<ConverterHandlerLaTexOutputOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            StringWriter string_writer = new StringWriter();

            if (Options!.MinimalWorkingExample)
            {
                string_writer.Write("\\documentclass{article}" + Environment.NewLine);
                string_writer.Write("\\begin{document}" + Environment.NewLine + Environment.NewLine);
            }

            string_writer.Write("\\begin{table}" + Environment.NewLine);

            var TableAlignGenerator = () =>
            {
                return Options!.SelectedTableAlignment switch
                {
                    "Left" => "\t\\raggedleft",
                    "Center" => "\t\\centering",
                    "Right" => "\t\\raggedright",
                    _ => "\t\\raggedleft"
                } + Environment.NewLine;
            };

            var BeginTableGenerator = () =>
            {
                string text_alignement_char = Options!.SelectedTextAlignment switch
                {
                    "Left" => "l",
                    "Center" => "c",
                    "Right" => "r",
                    _ => "l"
                };

                return "\t\\begin{tabular}{" + Options!.SelectedTableType switch
                {
                    "All" or "MySQL" or "Markdown" => "|" + string.Join("", Enumerable.Repeat($"{text_alignement_char}|", headers.Length)),
                    "Excel" => $"|{text_alignement_char}|" + string.Join("", Enumerable.Repeat($"{text_alignement_char}", headers.Length - 1)) + "|",
                    "Horizontal" or "None" or _ => string.Join("", Enumerable.Repeat($"{text_alignement_char}", headers.Length)),
                } + "}" + Environment.NewLine;
            };

            var AfterBeginTableGenerator = () =>
            {
                return Options!.SelectedTableType switch
                {
                    "All" or "MySQL" or "Excel" or "Horizontal" => "\t\\hline" + Environment.NewLine,
                    "Markdown" or "None" or _ => string.Empty
                };
            };

            var TableHeaderGenerator = () =>
            {
                return GenerateTableRow(headers, Options!.BoldHeader, Options!.BoldFirstColumn) +
                Options!.SelectedTableType switch
                {
                    "All" or "MySQL" or "Excel" or "Horizontal" or "Markdown" => " \\hline",
                    "None" or _ => string.Empty
                } + Environment.NewLine;
            };

            var TableRowsGenerator = () =>
            {
                StringWriter rows_string_writer = new StringWriter();

                for (long i = 0; i < rows.LongLength; i++)
                {
                    rows_string_writer.Write(GenerateTableRow(rows[i], false, Options!.BoldFirstColumn));

                    if (Options!.SelectedTableType == "None")
                    {
                        rows_string_writer.Write(Environment.NewLine);
                        continue;
                    }
                    else if (Options!.SelectedTableType == "All" || i == rows.LongLength - 1)
                    {
                        rows_string_writer.Write(" \\hline" + Environment.NewLine);
                    }
                    else
                    {
                        rows_string_writer.Write(Environment.NewLine);
                    }
                }

                return rows_string_writer.ToString();
            };

            string_writer.Write(TableAlignGenerator());

            if (Options!.CaptionName != string.Empty && Options!.SelectedCaptionAlignment == "Top")
            {
                string_writer.Write("\t\\caption{" + Options!.CaptionName + "}" + Environment.NewLine);
            }

            string_writer.Write(BeginTableGenerator());
            string_writer.Write(AfterBeginTableGenerator());
            string_writer.Write(TableHeaderGenerator());
            string_writer.Write(TableRowsGenerator());

            string_writer.Write("\t\\end{tabular}" + Environment.NewLine);

            if (Options!.CaptionName != string.Empty && Options!.SelectedCaptionAlignment == "Bottom")
            {
                string_writer.Write("\t\\caption{" + Options!.CaptionName + "}" + Environment.NewLine);
            }

            if (Options!.LabelName != string.Empty)
            {
                string_writer.Write("\t\\label{" + Options!.LabelName + "}" + Environment.NewLine);
            }

            string_writer.Write("\\end{table}" + Environment.NewLine);

            if (Options!.MinimalWorkingExample)
            {
                string_writer.Write(Environment.NewLine + "\\end{document}" + Environment.NewLine);
            }

            return string_writer.ToString();
        }

        private static string GenerateTableRow(string[] items, bool bold_header, bool bold_column)
        {
            StringWriter string_writer = new StringWriter();

            string_writer.Write("\t\t");

            for (long i = 0; i < items.LongLength; i++)
            {
                if ((i == 0 && bold_column) || bold_header)
                {
                    string_writer.Write("\\textbf{" + items[i] + "}");
                }
                else
                {
                    string_writer.Write(items[i] + "");
                }

                if (i != items.Length - 1)
                {
                    string_writer.Write(" & ");
                }
            }

            string_writer.Write(" \\\\");

            return string_writer.ToString();
        }
    }
}
