using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerPdfOutputService : ConverterHandlerOutputAbstract
    {
        private Document? PdfDocument { get; set; } = null;

        private readonly string[] Colours = [
            "#FFFFFF",
            "#F0F0F0",
            "#D9D9D9",
            "#BFBFBF",
            "#A6A6A6",
            "#8C8C8C",
            "#737373",
            "#595959",
            "#404040",
            "#262626",
            "#0D0D0D",
            "#000000"
        ];

        private string SelectedBackgroundColor { get; set; } = "#FFFFFF";

        private string SelectedForegroundColor { get; set; } = "#000000";

        private bool BoldHeader { get; set; } = true;

        private bool ShowGridLines { get; set; } = true;

        public ConverterHandlerPdfOutputService() : base()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            QuestPDF.Settings.EnableCaching = true;
            QuestPDF.Settings.EnableDebugging = false;
        }

        public override void InitializeControls()
        {
            var ColourDataTemplate = new FuncDataTemplate<string>((item, _) =>
            {
                var stack_panel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal 
                };

                var square = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width = 15,
                    Height = 15,
                    Fill = Avalonia.Media.Brush.Parse(item),
                    Margin = new Avalonia.Thickness(0, 0, 10, 0)
                };

                var text_block = new TextBlock 
                { 
                    Text = item,
                };

                stack_panel.Children.Add(square);
                stack_panel.Children.Add(text_block);

                return stack_panel;
            });

            var background_colour_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var background_colour_label = new Label()
            {
                Content = "Background Colour:",
            };

            var background_colour_combo_box = new ComboBox()
            {
                ItemsSource = Colours,
                SelectedItem = SelectedBackgroundColor,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };

            background_colour_combo_box.DataTemplates.Add(ColourDataTemplate);

            background_colour_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_background_colour)
                {
                    SelectedBackgroundColor = selected_background_colour;
                }
            };

            background_colour_stack_panel.Children.Add(background_colour_label);
            background_colour_stack_panel.Children.Add(background_colour_combo_box);

            Controls?.Add(background_colour_stack_panel);

            var foreground_colour_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var foreground_colour_label = new Label()
            {
                Content = "Text Colour:",
            };

            var foreground_colour_combo_box = new ComboBox()
            {
                ItemsSource = Colours,
                SelectedItem = SelectedForegroundColor,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };

            foreground_colour_combo_box.DataTemplates.Add(ColourDataTemplate);

            foreground_colour_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_foreground_colour)
                {
                    SelectedForegroundColor = selected_foreground_colour;
                }
            };

            foreground_colour_stack_panel.Children.Add(foreground_colour_label);
            foreground_colour_stack_panel.Children.Add(foreground_colour_combo_box);

            Controls?.Add(foreground_colour_stack_panel);

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
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            bold_header_stack_panel.Children.Add(bold_header_check_box);
            bold_header_stack_panel.Children.Add(bold_header_label);

            Controls?.Add(bold_header_stack_panel);

            var show_grid_lines_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var show_grid_lines_check_box = new CheckBox()
            {
                IsChecked = ShowGridLines
            };

            show_grid_lines_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    ShowGridLines = check_box.IsChecked ?? false;
                }
            };

            var show_grid_lines_label = new Label()
            {
                Content = "Show Grid Lines",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            show_grid_lines_stack_panel.Children.Add(show_grid_lines_check_box);
            show_grid_lines_stack_panel.Children.Add(show_grid_lines_label);

            Controls?.Add(show_grid_lines_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                PdfDocument = Document.Create(contrainer =>
                {
                    contrainer.Page(page =>
                    {
                        page.Content().Table(table =>
                        {
                            table.ExtendLastCellsToTableBottom();
                            table.ColumnsDefinition(column_definitions =>
                            {
                                for (long i = 0; i < headers.LongLength; i++)
                                {
                                    column_definitions.RelativeColumn();
                                }
                            });

                            for (long i = 0; i < headers.LongLength; i++)
                            {
                                if (BoldHeader)
                                {
                                    table.Cell().Row(1).Column((uint)i + 1).Element(Block).Text(headers[(int)i]).ExtraBold().FontColor(SelectedForegroundColor);
                                }
                                else
                                {
                                    table.Cell().Row(1).Column((uint)i + 1).Element(Block).Text(headers[(int)i]).FontColor(SelectedForegroundColor);
                                }
                            }

                            for (long i = 0; i < rows.LongLength; i++)
                            {
                                for (long j = 0; j < headers.LongLength; j++)
                                {
                                    table.Cell().Row((uint)i + 2).Column((uint)j + 1).Element(Block).Text(rows[i][j]).FontColor(SelectedForegroundColor);
                                }
                            }
                        });
                    });
                });

                SetProgressBarValue(progress_bar, 100, 0, 100);

                return $"Please save the '.pdf' file to view the generated file 😁{Environment.NewLine}";
            });
        }

        private IContainer Block(IContainer container)
        {
            return container
                    .Border(ShowGridLines ? 1 : 0)
                    .Background(SelectedBackgroundColor)
                    .ShowOnce()
                    .AlignCenter()
                    .AlignMiddle();
        }

        public override Task SaveFileAsync(IStorageFile output, ReadOnlyMemory<byte> buffer)
        {
            return Task.Run(async () =>
            {
                if (output is not null)
                {
                    using (var writer = await output.OpenWriteAsync())
                    {
                        writer.Write(PdfDocument?.GeneratePdf());
                        
                        writer.Close();
                    }
                }
            });
        }
    }
}
