using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerLaTexOutputService : ConverterHandlerOutputAbstract
    {
        private readonly string[] TableTypes = [
            "All",
            "MySQL",
            "Excel",
            "Horizontal",
            "Markdown",
            "None"
        ];

        private string SelectedTableType { get; set; } = "All";

        private readonly string[] Alignments = [ 
            "Left", 
            "Center", 
            "Right" 
        ];

        private string SelectedTextAlignment { get; set; } = "Left";

        private string SelectedTableAlignment { get; set; } = "Left";

        private readonly string[] CaptionAlignments = [ 
            "Top", 
            "Bottom" 
        ];

        private string SelectedCaptionAlignment { get; set; } = "Top";

        private string CaptionName { get; set; } = "";

        private string LabelName { get; set; } = "";

        private bool MinimalWorkingExample { get; set; } = false;

        private bool BoldHeader { get; set; } = false;

        private bool BoldFirstColumn { get; set; } = false;

        public override void InitializeControls()
        {
            Controls = new();

            var table_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_type_label = new Label()
            {
                Content = "LaTex Table Type:",
            };

            var table_type_combo_box = new ComboBox()
            {
                ItemsSource = TableTypes,
                SelectedItem = SelectedTableType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            table_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_table_type)
                {
                    SelectedTableType = selected_table_type;
                }
            };

            table_type_stack_panel.Children.Add(table_type_label);
            table_type_stack_panel.Children.Add(table_type_combo_box);

            Controls?.Add(table_type_stack_panel);

            var text_alignment_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var text_alignment_label = new Label()
            {
                Content = "Text Alignment:",
            };

            var text_alignment_combo_box = new ComboBox()
            {
                ItemsSource = Alignments,
                SelectedItem = SelectedTextAlignment,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            text_alignment_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_text_alignment)
                {
                    SelectedTextAlignment = selected_text_alignment;
                }
            };

            text_alignment_stack_panel.Children.Add(text_alignment_label);
            text_alignment_stack_panel.Children.Add(text_alignment_combo_box);

            Controls?.Add(text_alignment_stack_panel);

            var table_alignment_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_alignment_label = new Label()
            {
                Content = "Table Alignment:",
            };

            var table_alignment_combo_box = new ComboBox()
            {
                ItemsSource = Alignments,
                SelectedItem = SelectedTableAlignment,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            table_alignment_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_table_alignment)
                {
                    SelectedTableAlignment = selected_table_alignment;
                }
            };

            table_alignment_stack_panel.Children.Add(table_alignment_label);
            table_alignment_stack_panel.Children.Add(table_alignment_combo_box);

            Controls?.Add(table_alignment_stack_panel);

            var caption_alignment_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var caption_alignment_label = new Label()
            {
                Content = "Caption Alignment:",
            };

            var caption_alignment_combo_box = new ComboBox()
            {
                ItemsSource = CaptionAlignments,
                SelectedItem = SelectedCaptionAlignment,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            caption_alignment_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_caption_alignment)
                {
                    SelectedCaptionAlignment = selected_caption_alignment;
                }
            };

            caption_alignment_stack_panel.Children.Add(caption_alignment_label);
            caption_alignment_stack_panel.Children.Add(caption_alignment_combo_box);

            Controls?.Add(caption_alignment_stack_panel);

            var caption_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var caption_name_label = new Label()
            {
                Content = "Caption Name:",
            };

            var caption_name_text_box = new TextBox()
            {
                Text = CaptionName,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            caption_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    CaptionName = text_box.Text ?? "";
                }
            };

            caption_name_stack_panel.Children.Add(caption_name_label);
            caption_name_stack_panel.Children.Add(caption_name_text_box);

            Controls?.Add(caption_name_stack_panel);

            var label_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var label_name_label = new Label()
            {
                Content = "Label Name:",
            };

            var label_name_text_box = new TextBox()
            {
                Text = LabelName,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            label_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    LabelName = text_box.Text ?? "";
                }
            };

            label_name_stack_panel.Children.Add(label_name_label);
            label_name_stack_panel.Children.Add(label_name_text_box);

            Controls?.Add(label_name_stack_panel);

            var minimal_working_example_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var minimal_working_example_check_box = new CheckBox()
            {
                IsChecked = MinimalWorkingExample
            };

            minimal_working_example_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    MinimalWorkingExample = check_box.IsChecked ?? false;
                }
            };

            var minimal_working_example_label = new Label()
            {
                Content = "Minimal Working Example",
                VerticalAlignment = VerticalAlignment.Center
            };

            minimal_working_example_stack_panel.Children.Add(minimal_working_example_check_box);
            minimal_working_example_stack_panel.Children.Add(minimal_working_example_label);

            Controls?.Add(minimal_working_example_stack_panel);

            var bold_header_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var bold_header_check_box = new CheckBox()
            {
                IsChecked = BoldHeader
            };

            bold_header_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    BoldHeader = check_box.IsChecked ?? false;
                }
            };

            var bold_header_label = new Label()
            {
                Content = "Bold Header",
                VerticalAlignment = VerticalAlignment.Center
            };

            bold_header_stack_panel.Children.Add(bold_header_check_box);
            bold_header_stack_panel.Children.Add(bold_header_label);

            Controls?.Add(bold_header_stack_panel);

            var bold_first_column_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var bold_first_column_check_box = new CheckBox()
            {
                IsChecked = BoldFirstColumn
            };

            bold_first_column_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    BoldFirstColumn = check_box.IsChecked ?? false;
                }
            };

            var bold_first_column_label = new Label()
            {
                Content = "Bold First Column",
                VerticalAlignment = VerticalAlignment.Center
            };

            bold_first_column_stack_panel.Children.Add(bold_first_column_check_box);
            bold_first_column_stack_panel.Children.Add(bold_first_column_label);

            Controls?.Add(bold_first_column_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows)
        {
            return Task.Run(() =>
            {
                StringWriter string_writer = new StringWriter();

                if (MinimalWorkingExample)
                {
                    string_writer.Write("\\documentclass{article}" + Environment.NewLine);
                    string_writer.Write("\\begin{document}" + Environment.NewLine + Environment.NewLine);
                }

                string_writer.Write("\\begin{table}" + Environment.NewLine);

                var TableAlignGenerator = () =>
                {
                    return SelectedTableAlignment switch
                    {
                        "Left" => "\t\\raggedleft",
                        "Center" => "\t\\centering",
                        "Right" => "\t\\raggedright",
                        _ => "\t\\raggedleft"
                    } + Environment.NewLine;
                };

                var BeginTableGenerator = () =>
                {
                    string text_alignement_char = SelectedTextAlignment switch
                    {
                        "Left" => "l",
                        "Center" => "c",
                        "Right" => "r",
                        _ => "l"
                    };

                    return "\t\\begin{tabular}{" + SelectedTableType switch
                    {
                        "All" or "MySQL" or "Markdown" => "|" + string.Join("", Enumerable.Repeat($"{text_alignement_char}|", headers.Length)),
                        "Excel" => $"|{text_alignement_char}|" + string.Join("", Enumerable.Repeat($"{text_alignement_char}", headers.Length - 1)) + "|",
                        "Horizontal" or "None" or _ => string.Join("", Enumerable.Repeat($"{text_alignement_char}", headers.Length)),
                    } + "}" + Environment.NewLine;
                };

                var AfterBeginTableGenerator = () =>
                {
                    return SelectedTableType switch
                    {
                        "All" or "MySQL" or "Excel" or "Horizontal" => "\t\\hline" + Environment.NewLine,
                        "Markdown" or "None" or _ => string.Empty
                    };
                };

                var TableHeaderGenerator = () =>
                {
                    return GenerateTableRow(headers, BoldHeader, BoldFirstColumn) +
                    SelectedTableType switch
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
                        rows_string_writer.Write(GenerateTableRow(rows[i], false, BoldFirstColumn));

                        if (SelectedTableType == "None")
                        {
                            rows_string_writer.Write(Environment.NewLine);
                            continue;
                        }
                        else if (SelectedTableType == "All" || i == rows.LongLength - 1)
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

                if (CaptionName != string.Empty && SelectedCaptionAlignment == "Top")
                {
                    string_writer.Write("\t\\caption{" + CaptionName + "}" + Environment.NewLine);
                }

                string_writer.Write(BeginTableGenerator());
                string_writer.Write(AfterBeginTableGenerator());
                string_writer.Write(TableHeaderGenerator());
                string_writer.Write(TableRowsGenerator());

                string_writer.Write("\t\\end{tabular}" + Environment.NewLine);

                if (CaptionName != string.Empty && SelectedCaptionAlignment == "Bottom")
                {
                    string_writer.Write("\t\\caption{" + CaptionName + "}" + Environment.NewLine);
                }

                if (LabelName != string.Empty)
                {
                    string_writer.Write("\t\\label{" + LabelName + "}" + Environment.NewLine);
                }

                string_writer.Write("\\end{table}" + Environment.NewLine);

                if (MinimalWorkingExample)
                {
                    string_writer.Write(Environment.NewLine + "\\end{document}" + Environment.NewLine);
                }

                return string_writer.ToString();
            });
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
