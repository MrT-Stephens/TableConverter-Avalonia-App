using System.Text;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerAsciiOutput : ConverterHandlerOutputAbstract<ConverterHandlerAsciiOutputOptions>
    {
        public override string Convert(string[] headers, string[][] rows)
        {
            var ascii_output = new StringBuilder();

            var table_character_config = Options!.TableTypes[Options!.SelectedTableType];

            // Calculates the max text character widths of every column.
            long[] max_column_widths = new long[headers.LongLength];

            for (long i = 0; i < headers.LongLength; i++)
            {
                max_column_widths[i] = headers[i].Length + 2;
            }

            foreach (string[] row in rows)
            {
                for (long i = 0; i < row.LongLength; i++)
                {
                    max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().LongCount() + 2);
                }
            }

            string comment = Options!.SelectedCommentType == Options!.CommentTypes.First().Key ? "" : $"{Options!.CommentTypes[Options!.SelectedCommentType]}    ";

            // Draws the table header.
            ascii_output.AppendLine(comment +
                    DrawSeparator(max_column_widths, 
                    table_character_config.HeaderTopLeft, 
                    table_character_config.HeaderTopRight, 
                    table_character_config.TopIntersection,
                    table_character_config.Horizontal));

            ascii_output.AppendLine(comment +
                    DrawDataRow(headers, max_column_widths,
                    Options!.TextAlignment[Options!.SelectedTextAlignment],
                    table_character_config.Vertical,
                    table_character_config.Vertical,
                    table_character_config.Vertical));

            ascii_output.AppendLine(comment +
                    DrawSeparator(max_column_widths,
                    table_character_config.LeftIntersection,
                    table_character_config.RightIntersection,
                    table_character_config.MiddleIntersection,
                    table_character_config.Horizontal));

            // Draws the table rows.
            for (long i = 0; i < rows.LongLength; i++)
            {
                ascii_output.AppendLine(comment +
                        DrawDataRow(rows[i], max_column_widths,
                        Options!.TextAlignment[Options!.SelectedTextAlignment],
                        table_character_config.Vertical,
                        table_character_config.Vertical,
                        table_character_config.Vertical));

                if (i < rows.LongLength - 1 && Options!.ForceRowSeparators)
                {
                    ascii_output.AppendLine(comment +
                            DrawSeparator(max_column_widths,
                            table_character_config.LeftIntersection,
                            table_character_config.RightIntersection,
                            table_character_config.MiddleIntersection,
                            table_character_config.Horizontal));
                }
                else if (i == rows.LongLength - 1)
                {
                    ascii_output.AppendLine(comment +
                            DrawSeparator(max_column_widths,
                            table_character_config.BottomLeft,
                            table_character_config.BottomRight,
                            table_character_config.BottomIntersection,
                            table_character_config.Horizontal));
                }
            }

            return ascii_output.ToString();
        }

        private static string DrawSeparator(long[] column_widths, char left_char, char right_char, char intersection_char, char fill_char)
        {
            var separator = new StringBuilder();

            separator.Append(left_char);

            for (long i = 0; i < column_widths.LongLength; i++)
            {
                separator.Append(new string(fill_char, (int)column_widths[i]));

                separator.Append(i == column_widths.LongLength - 1 ? right_char : intersection_char);
            }

            return separator.ToString();
        }

        private static string DrawDataRow(string[] row, long[] column_widths, TextAlignment text_alignment, char left_char, char right_char, char intersection_char)
        {
            var data_row = new StringBuilder();

            data_row.Append(left_char);

            for (long i = 0; i < row.LongLength; i++)
            {
                data_row.Append(ConverterHandlerUtilities.AlignText(row[i], text_alignment, (int)column_widths[i], ' '));

                data_row.Append(i == row.LongLength - 1 ? right_char : intersection_char);
            }

            return data_row.ToString();
        }
    }
}
