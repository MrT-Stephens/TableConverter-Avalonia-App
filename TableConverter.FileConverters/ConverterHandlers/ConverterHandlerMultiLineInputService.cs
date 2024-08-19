﻿using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerMultiLineInputService : ConverterHandlerInputAbstract
    {
        private string RowSeparator { get; set; } = "---";

        public override void InitializeControls()
        {
            Controls = new();

            var row_separator_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var row_separator_label = new TextBlock()
            {
                Text = "Row Separator:",
            };

            var row_separator_text_box = new TextBox()
            {
                Text = RowSeparator,
            };

            row_separator_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    RowSeparator = text_box.Text ?? "";
                }
            };

            row_separator_stack_panel.Children.Add(row_separator_label);
            row_separator_stack_panel.Children.Add(row_separator_text_box);

            Controls?.Add(row_separator_stack_panel);
        }

        public override Task<TableData> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                using (var reader = new StringReader(text))
                {
                    bool first_line = true;

                    List<string> row = new List<string>();

                    for (string? line = reader?.ReadLine()?.Trim();
                         !string.IsNullOrEmpty(line);
                         line = reader?.ReadLine()?.Trim())
                    {
                        if (line == RowSeparator)
                        {
                            if (first_line)
                            {
                                first_line = false;
                            }
                            else
                            {
                                rows.Add(row.ToArray());
                                row.Clear();
                            }
                        }
                        else if (first_line)
                        {
                            headers.Add(line);
                        }
                        else
                        {
                            row.Add(line);
                        }
                    }

                    if (row.Count > 0)
                    {
                        rows.Add(row.ToArray());
                    }
                }

                return new TableData(headers, rows);
            });
        }
    }
}