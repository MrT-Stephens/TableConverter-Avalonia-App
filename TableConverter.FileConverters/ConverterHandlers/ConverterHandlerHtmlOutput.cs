using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerHtmlOutput : ConverterHandlerOutputAbstract<ConverterHandlerHtmlOutputOptions>
    {
        public override Result<string> Convert(string[] headers, string[][] rows)
        {
            var stringWriter = new StringWriter();

            var tabCount = 0;

            stringWriter.Write($"<table>{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tabCount))}");

            if (Options!.IncludeTheadTbody)
            {
                stringWriter.Write($"<thead>{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tabCount))}");
            }

            stringWriter.Write($"<tr>{(Options!.MinifyHtml ? "" : Environment.NewLine)}");

            tabCount++;

            for (long i = 0; i < headers.LongLength; i++)
            {
                stringWriter.Write($"{(Options!.MinifyHtml ? "" : new string('\t', tabCount))}<th>");
                stringWriter.Write(headers[i]);
                stringWriter.Write($"</th>{(Options!.MinifyHtml ? "" : Environment.NewLine)}");
            }

            stringWriter.Write($"{(Options!.MinifyHtml ? "" : new string('\t', --tabCount))}</tr>");

            if (Options!.IncludeTheadTbody)
            {
                stringWriter.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tabCount))}</thead>");
            }

            for (long i = 0; i < rows.LongLength; i++)
            {
                if (Options!.IncludeTheadTbody)
                {
                    stringWriter.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tabCount++))}<tbody>");
                }

                stringWriter.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tabCount++))}<tr>");

                for (long j = 0; j < headers.LongLength; j++)
                {
                    stringWriter.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tabCount))}<td>");
                    stringWriter.Write(rows[i][j]);
                    stringWriter.Write("</td>");
                }

                stringWriter.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tabCount))}</tr>");

                if (Options!.IncludeTheadTbody)
                {
                    stringWriter.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tabCount))}</tbody>");
                }
            }

            stringWriter.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine)}</table>");

            return Result<string>.Success(stringWriter.ToString());
        }
    }
}
