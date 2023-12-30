using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_PDF : ConverterHandlerBase
    {
        private string[] Colours { get; init; } = new string[]
        {
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
        };

        private Document document { get; set; }
        private bool BoldHeader { get; set; } = true;
        private bool ShowGridLines { get; set; } = true;
        private string CurrentBackgroundColor { get; set; } = "#FFFFFF";
        private string CurrentForegroundColor { get; set; } = "#000000";

        public ConverterHandler_PDF()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            QuestPDF.Settings.EnableCaching = true;
        }

        public override void InitializeControls()
        {
            base.InitializeControls();

            var BoldHeaderCheckBox = new CheckBox 
            { 
                Content = "Bold Header", 
                IsChecked = BoldHeader 
            };

            BoldHeaderCheckBox.Checked += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    BoldHeader = ((CheckBox)sender).IsChecked.Value;
                }
            };

            var ShowGridLinesCheckBox = new CheckBox
            {
                Content = "Show Grid Lines", 
                IsChecked = ShowGridLines 
            };

            ShowGridLinesCheckBox.Checked += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    ShowGridLines = ((CheckBox)sender).IsChecked.Value;
                }
            };

            var DataTemplate = new FuncDataTemplate<string>((item, _) =>
            {
                var StackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };

                var Square = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width = 15,
                    Height = 15,
                    Fill = Avalonia.Media.Brush.Parse(item),
                    Margin = new Avalonia.Thickness(0, 0, 10, 0)
                };

                var TextBlock = new TextBlock { Text = item };

                StackPanel.Children.Add(Square);
                StackPanel.Children.Add(TextBlock);

                return StackPanel;
            });

            var BackgroundStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var BackgroundLabel = new Label { Content = "Background Color:" };

            var BackgroundComboBox = new ComboBox
            {
                ItemsSource = Colours,
                SelectedIndex = 0
            };

            BackgroundComboBox.DataTemplates.Add(DataTemplate);

            BackgroundComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentBackgroundColor = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            BackgroundStackPanel.Children.Add(BackgroundLabel);
            BackgroundStackPanel.Children.Add(BackgroundComboBox);

            var ForegroundStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var ForegroundLabel = new Label { Content = "Text Color:" };

            var ForegroundComboBox = new ComboBox
            {
                ItemsSource = Colours,
                SelectedIndex = 11
            };

            ForegroundComboBox.DataTemplates.Add(DataTemplate);

            ForegroundComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentForegroundColor = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            ForegroundStackPanel.Children.Add(ForegroundLabel);
            ForegroundStackPanel.Children.Add(ForegroundComboBox);

            Controls?.Add(BoldHeaderCheckBox);
            Controls?.Add(ShowGridLinesCheckBox);
            Controls?.Add(BackgroundStackPanel);
            Controls?.Add(ForegroundStackPanel);
        }

        public override Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                document = Document.Create(contrainer =>
                {
                    contrainer.Page(page =>
                    {
                        page.Content().Table(table =>
                        {
                            table.ExtendLastCellsToTableBottom();
                            table.ColumnsDefinition(column_definitions =>
                            {
                                for (uint i = 0; i < input.Columns.Count; ++i)
                                {
                                    column_definitions.RelativeColumn();
                                }
                            });

                            for (uint i = 0; i < input.Columns.Count; ++i)
                            {
                                if (BoldHeader)
                                {
                                    table.Cell().Row(1).Column(i + 1).Element(Block).Text(input.Columns[(int)i].ColumnName).ExtraBold().FontColor(CurrentForegroundColor);
                                }
                                else
                                {
                                    table.Cell().Row(1).Column(i + 1).Element(Block).Text(input.Columns[(int)i].ColumnName).FontColor(CurrentForegroundColor);
                                }
                            }

                            for (uint i = 0; i < input.Rows.Count; ++i)
                            {
                                for (uint j = 0; j < input.Columns.Count; ++j)
                                {
                                    table.Cell().Row(i + 2).Column(j + 1).Element(Block).Text(input.Rows[(int)i][(int)j].ToString()).FontColor(CurrentForegroundColor);
                                }
                            }
                        });
                    });
                });

                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = 1000);

                return $"Please download the '.pdf' file to view the generated data 😁{Environment.NewLine}";
            });
        }

        public override Task SaveFileAsync(IStorageFile output, string data)
        {
            return Task.Run(async () =>
            {
                if (output != null)
                {
                    await output.OpenWriteAsync().ContinueWith((task) =>
                    {
                        if (task.Result != null)
                        {
                            task.Result.Write(document.GeneratePdf());

                            task.Result.Close();
                        }
                    });
                }
            });
        }

        private IContainer Block(IContainer container)
        {
            return container
                .Border(ShowGridLines ? 1 : 0)
                .Background(CurrentBackgroundColor)
                .ShowOnce()
                .AlignCenter()
                .AlignMiddle();
        }
    }
}
