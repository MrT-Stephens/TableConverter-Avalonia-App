using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerHtmlOutput : ConverterHandlerOutputAbstract<ConverterHandlerHtmlOutputOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            StringWriter string_writer = new StringWriter();

            int tab_count = 0;

            string_writer.Write($"<table>{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tab_count))}");

            if (Options!.IncludeTheadTbody)
            {
                string_writer.Write($"<thead>{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tab_count))}");
            }

            string_writer.Write($"<tr>{(Options!.MinifyHtml ? "" : Environment.NewLine)}");

            tab_count++;

            for (long i = 0; i < headers.LongLength; i++)
            {
                string_writer.Write($"{(Options!.MinifyHtml ? "" : new string('\t', tab_count))}<th>");
                string_writer.Write(headers[i]);
                string_writer.Write($"</th>{(Options!.MinifyHtml ? "" : Environment.NewLine)}");
            }

            string_writer.Write($"{(Options!.MinifyHtml ? "" : new string('\t', --tab_count))}</tr>");

            if (Options!.IncludeTheadTbody)
            {
                string_writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</thead>");
            }

            for (long i = 0; i < rows.LongLength; i++)
            {
                if (Options!.IncludeTheadTbody)
                {
                    string_writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count++))}<tbody>");
                }

                string_writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count++))}<tr>");

                for (long j = 0; j < headers.LongLength; j++)
                {
                    string_writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count))}<td>");
                    string_writer.Write(rows[i][j]);
                    string_writer.Write("</td>");
                }

                string_writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</tr>");

                if (Options!.IncludeTheadTbody)
                {
                    string_writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</tbody>");
                }
            }

            string_writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine)}</table>");

            return string_writer.ToString();
        }
    }
}
