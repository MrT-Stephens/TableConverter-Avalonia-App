using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_LaTex : ConverterHandlerBase
    {
        private string[] TableTypes { get; init; } = 
        { 
            "All",
            "MySQL",
            "Excel",
            "Horizontal",
            "Markdown",
            "None"
        };
        private string[] Alignments { get; init; } = { "Left", "Center", "Right" };
        private string[] CaptionAlignments { get; init; } = { "Top", "Bottom" };

        private string CurrentTableType { get; set; } = "All";
        private string CurrentTextAlignment { get; set; } = "Left";
        private string CurrentTableAlignment { get; set; } = "Left";
        private string CurrentCaptionAlignment { get; set; } = "Top";
        private string CaptionName { get; set; } = "";
        private string LabelName { get; set; } = "";
        private bool MinimalWorkingExample { get; set; } = false;
        private bool BoldHeader { get; set; } = false;
        private bool BoldColumn { get; set; } = false;

        public override void InitializeControls()
        {
            base.InitializeControls();

            var TableTypeStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var TableTypeLabel = new Label { Content = "Table Type:" };

            var TableTypeComboBox = new ComboBox
            {
                ItemsSource = TableTypes,
                SelectedIndex = 0
            };

            TableTypeComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentTableType = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            TableTypeStackPanel.Children.Add(TableTypeLabel);
            TableTypeStackPanel.Children.Add(TableTypeComboBox);

            var TextAlignmentStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var TextAlignmentLabel = new Label { Content = "Text Alignment:" };

            var TextAlignmentComboBox = new ComboBox
            {
                ItemsSource = Alignments,
                SelectedIndex = 0
            };

            TextAlignmentComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentTextAlignment = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            TextAlignmentStackPanel.Children.Add(TextAlignmentLabel);
            TextAlignmentStackPanel.Children.Add(TextAlignmentComboBox);

            var TableAlignmentStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var TableAlignmentLabel = new Label { Content = "Table Alignment:" };

            var TableAlignmentComboBox = new ComboBox
            {
                ItemsSource = Alignments,
                SelectedIndex = 0
            };

            TableAlignmentComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentTableAlignment = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            TableAlignmentStackPanel.Children.Add(TableAlignmentLabel);
            TableAlignmentStackPanel.Children.Add(TableAlignmentComboBox);

            var CaptionAlignmentStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var CaptionAlignmentLabel = new Label { Content = "Caption Alignment:" };

            var CaptionAlignmentComboBox = new ComboBox
            {
                ItemsSource = CaptionAlignments,
                SelectedIndex = 0
            };

            CaptionAlignmentComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentCaptionAlignment = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            CaptionAlignmentStackPanel.Children.Add(CaptionAlignmentLabel);
            CaptionAlignmentStackPanel.Children.Add(CaptionAlignmentComboBox);

            var CaptionNameStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var CaptionNameLabel = new Label { Content = "Caption Name:" };

            var CaptionNameTextBox = new TextBox
            {
                Text = CaptionName
            };

            CaptionNameTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    CaptionName = ((TextBox)sender).Text;
                }
            };

            CaptionNameStackPanel.Children.Add(CaptionNameLabel);
            CaptionNameStackPanel.Children.Add(CaptionNameTextBox);

            var LabelNameStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var LabelNameLabel = new Label { Content = "Label Name:" };

            var LabelNameTextBox = new TextBox
            {
                Text = LabelName
            };

            LabelNameTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    LabelName = ((TextBox)sender).Text;
                }
            };

            LabelNameStackPanel.Children.Add(LabelNameLabel);
            LabelNameStackPanel.Children.Add(LabelNameTextBox);

            var MinimalWorkingExampleCheckBox = new CheckBox
            {
                Content = "Minimal Working"
            };

            MinimalWorkingExampleCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    MinimalWorkingExample = ((CheckBox)sender).IsChecked.Value;
                }
            };

            var BoldHeaderCheckBox = new CheckBox
            {
                Content = "Bold Header",
                IsChecked = BoldHeader
            };

            BoldHeaderCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    BoldHeader = ((CheckBox)sender).IsChecked.Value;
                }
            };

            var BoldColumnCheckBox = new CheckBox
            {
                Content = "Bold Column",
                IsChecked = BoldColumn
            };

            BoldColumnCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    BoldColumn = ((CheckBox)sender).IsChecked.Value;
                }
            };

            Controls?.Add(TableTypeStackPanel);
            Controls?.Add(TextAlignmentStackPanel);
            Controls?.Add(TableAlignmentStackPanel);
            Controls?.Add(CaptionAlignmentStackPanel);
            Controls?.Add(CaptionNameStackPanel);
            Controls?.Add(LabelNameStackPanel);
            Controls?.Add(MinimalWorkingExampleCheckBox);
            Controls?.Add(BoldHeaderCheckBox);
            Controls?.Add(BoldColumnCheckBox);
        }

        public override Task<string> ConvertAsync(string[] column_values, string[][] row_values, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = string.Empty;

                if (MinimalWorkingExample)
                {
                    output += "\\documentclass{article}" + Environment.NewLine + "\\begin{document}" + Environment.NewLine + Environment.NewLine;
                }

                output += "\\begin{table}" + Environment.NewLine;

                var TableAlignGenerator = () =>
                {
                    return CurrentTableAlignment switch
                    {
                        "Left" => "\t\\raggedleft",
                        "Center" => "\t\\centering",
                        "Right" => "\t\\raggedright"
                    } + Environment.NewLine;
                };

                var BeginTableGenerator = () =>
                {
                    string text_alignement_char = CurrentTextAlignment switch
                    {
                        "Left" => "l",
                        "Center" => "c",
                        "Right" => "r"
                    };

                    return "\t\\begin{tabular}{" + CurrentTableType switch
                    {
                        "All" or "MySQL" or "Markdown" => "|" + string.Join("", Enumerable.Repeat($"{text_alignement_char}|", column_values.Length)),
                        "Excel" => $"|{text_alignement_char}|" + string.Join("", Enumerable.Repeat($"{text_alignement_char}", column_values.Length - 1)) + "|",
                        "Horizontal" or "None" => string.Join("", Enumerable.Repeat($"{text_alignement_char}", column_values.Length)),
                    } + "}" + Environment.NewLine;
                };

                var AfterBeginTableGenerator = () =>
                {
                    return CurrentTableType switch
                    {
                        "All" or "MySQL" or "Excel" or "Horizontal" => "\t\\hline" + Environment.NewLine,
                        "Markdown" or "None" or _ => string.Empty
                    };
                };

                var TableHeaderGenerator = () =>
                {
                    return GenerateTableRow(column_values, BoldHeader, BoldColumn) +
                    CurrentTableType switch
                    {
                        "All" or "MySQL" or "Excel" or "Horizontal" or "Markdown" => " \\hline",
                        "None" or _ => string.Empty
                    } + Environment.NewLine;
                };

                var TableRowsGenerator = () =>
                {
                    string temp = string.Empty;

                    for (int i = 0; i < row_values.Length; i++)
                    {
                        temp += GenerateTableRow(row_values[i], false, BoldColumn);

                        Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));

                        if (CurrentTableType == "None")
                        {
                            temp += Environment.NewLine;
                            continue;
                        }
                        else if (CurrentTableType == "All" || i == row_values.Length - 1)
                        {
                            temp += " \\hline" + Environment.NewLine;
                        }
                        else
                        {
                            temp += Environment.NewLine;
                        }
                    }

                    return temp;
                };

                output += TableAlignGenerator();

                if (CaptionName != string.Empty && CurrentCaptionAlignment == "Top")
                {
                    output += "\t\\caption{" + CaptionName + "}" + Environment.NewLine;
                }
                
                output += BeginTableGenerator() + AfterBeginTableGenerator() + TableHeaderGenerator() + TableRowsGenerator();

                output += "\t\\end{tabular}" + Environment.NewLine;

                if (CaptionName != string.Empty && CurrentCaptionAlignment == "Bottom")
                {
                    output += "\t\\caption{" + CaptionName + "}" + Environment.NewLine;
                }

                if (LabelName != string.Empty)
                {
                    output += "\t\\label{" + LabelName + "}" + Environment.NewLine;
                }

                output += "\\end{table}" + Environment.NewLine;

                if (MinimalWorkingExample)
                {
                    output += Environment.NewLine + "\\end{document}" + Environment.NewLine;
                }

                return output;
            });
        }

        private static string GenerateTableRow(string[] items, bool bold_header, bool bold_column)
        {
            string output = "\t\t";

            for (int i = 0; i < items.Length; ++i)
            {
                if ((i == 0 && bold_column) || bold_header)
                {
                    output += "\\textbf{" + items[i] + "}";
                }
                else
                {
                    output += items[i] + "";
                }

                if (i != items.Length - 1)
                {
                    output += " & ";
                }
            }

            return output + " \\\\";
        }
    }
}
