using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerMarkdownOutput : ConverterHandlerOutputAbstract<ConverterHandlerMarkdownOutputOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            var ascii_output = new StringBuilder();

            // Calculates the max text character widths of every column.
            long[] max_column_widths = new long[headers.LongLength];

            for (long i = 0; i < headers.LongLength; i++)
            {
                if ((i == 0 && Options!.BoldFirstColumn) || Options!.BoldColumnNames)
                {
                    max_column_widths[i] = headers[i].LongCount() + 6;
                }
                else
                {
                    max_column_widths[i] = headers[i].LongCount() + 2;
                }
            }

            foreach (string[] row in rows)
            {
                for (long i = 0; i < headers.LongLength; i++)
                {
                    if (i == 0 && Options!.BoldFirstColumn)
                    {
                        max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().LongCount() + 6);
                    }
                    else
                    {
                        max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().LongCount() + 2);
                    }
                }
            }

            string line = string.Empty;

            // Bold the data if the user has selected to bold the first column.
            if (Options!.BoldFirstColumn || Options!.BoldColumnNames)
            {
                headers[0] = "**" + headers[0] + "**";
            }

            if (Options!.BoldColumnNames)
            {
                for (long i = 1; i < headers.LongLength; i++)
                {
                    headers[i] = "**" + headers[i] + "**";
                }
            }

            if (Options!.BoldFirstColumn)
            {
                foreach (var row in rows)
                {
                    row[0] = "**" + row[0] + "**";
                }
            }

            // Draw the table.
            switch (Options!.SelectedTableType)
            {
                case "Markdown Table (Normal)":
                    {
                        ascii_output.AppendLine("|" + DrawDataRow(headers, max_column_widths, Options!.TextAlignment[Options!.SelectedTextAlignment], '|') + "|");
                        ascii_output.AppendLine("|" + DrawSeparator(max_column_widths, '|', '-') + "|");

                        foreach (var row in rows)
                        {
                            ascii_output.AppendLine("|" + DrawDataRow(row, max_column_widths, Options!.TextAlignment[Options!.SelectedTextAlignment], '|') + "|");
                        }

                        break;
                    }
                case "Markdown Table (Simple)":
                    {
                        ascii_output.AppendLine(DrawDataRow(headers, max_column_widths, Options!.TextAlignment[Options!.SelectedTextAlignment], '|'));
                        ascii_output.AppendLine(DrawSeparator(max_column_widths, '|', '-'));

                        foreach (var row in rows)
                        {
                            ascii_output.AppendLine(DrawDataRow(row, max_column_widths, Options!.TextAlignment[Options!.SelectedTextAlignment], '|'));
                        }

                        break;
                    }
            }

            return ascii_output.ToString();
        }

        private static string DrawSeparator(long[] column_widths, char intersection_char, char fill_char)
        {
            var separator = new StringBuilder();

            for (long i = 0; i < column_widths.LongLength; i++)
            {
                separator.Append(new string(fill_char, (int)column_widths[i]));

                separator.Append(i == column_widths.LongLength - 1 ? "" : intersection_char);
            }

            return separator.ToString();
        }

        private static string DrawDataRow(string[] row, long[] column_widths, TextAlignment text_alignment, char intersection_char)
        {
            var data_row = new StringBuilder();

            for (long i = 0; i < row.LongLength; i++)
            {
                data_row.Append(ConverterHandlerUtilities.AlignText(row[i], text_alignment, (int)column_widths[i], ' '));

                data_row.Append(i == row.LongLength - 1 ? "" : intersection_char);
            }

            return data_row.ToString();
        }
    }
}
