using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Avalonia.Data;
using TableConverter.ViewModels;
using System.Text;
using Avalonia.Threading;
using TableConverter.DataModels;
using Avalonia.Media;
using DialogHostAvalonia;

namespace TableConverter.Views;

public partial class TableConverterView : UserControl
{
    private double? ResizeControlOriginalHeight = null;
    private double? ResizeControlOriginalY = null;

    public TableConverterView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        InitializeComponent(true);

        MainSplitView.Loaded += (sender, e) => 
        {
            ResizeInformationSidePanel();
            RefreshEditDataGrid();
        };

        SizeChanged += (sender, e) => ResizeInformationSidePanel();
    }

    private void ResizeInformationSidePanel()
    {
        var top_level_window = TopLevel.GetTopLevel(this);

        if (top_level_window!.ClientSize.Width < 1000)
        {
            MainSplitView.IsPaneOpen = false;
            MainSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
            MainSplitView.OpenPaneLength = top_level_window.ClientSize.Width;
        }
        else
        {
            MainSplitView.DisplayMode = SplitViewDisplayMode.Inline;
            MainSplitView.IsPaneOpen = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() ? false : true;
            MainSplitView.OpenPaneLength = top_level_window.ClientSize.Width / 3;
        }
    }

    private void ResizableControlIconPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is PathIcon resize_icon &&
            resize_icon.Parent is Grid grid &&
            grid.Children[0] is Control resize_control)
        {
            ResizeControlOriginalHeight = resize_control.Bounds.Height;
            ResizeControlOriginalY = e.GetPosition(resize_control).Y;
        }
    }

    private void ResizableControlIconPointerMoved(object sender, PointerEventArgs e)
    {
        if (sender is PathIcon resize_icon &&
            resize_icon.Parent is Grid grid &&
            grid.Children[0] is Control resize_control &&
            ResizeControlOriginalHeight.HasValue &&
            ResizeControlOriginalY.HasValue)
        {
            double delta_y = e.GetPosition(resize_control).Y - (double)ResizeControlOriginalY;
            double new_height = (double)ResizeControlOriginalHeight + delta_y;

            if (new_height > 0)
            {
                resize_control.Height = new_height;
            }
        }
    }

    private void ResizableControlIconPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        ResizeControlOriginalHeight = null;
        ResizeControlOriginalY = null;
    }

    private void CloseAboutSplitViewButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainSplitView.IsPaneOpen = false;
    }

    private void HeaderMenuAboutButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
    }

    private async void OpenFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel table_converter_view_model)
        {
            var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Open {table_converter_view_model.SelectedInputConverter.name} File",
                AllowMultiple = false,
                FileTypeFilter = [
                    new(table_converter_view_model.SelectedInputConverter.name)
                    {
                        Patterns = table_converter_view_model.SelectedInputConverter.extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = table_converter_view_model.SelectedInputConverter.mime_types
                    },
                    FilePickerFileTypes.All
                ]
            });

            if (files.Count >= 1)
            {
                if (table_converter_view_model.SelectedInputConverter.input_converter!.Controls is not null && Parent is DialogHost dialog_host)
                {
                    DialogHostOptionsView options = new()
                    {
                        Title = $"How would you like your {table_converter_view_model.SelectedInputConverter.name} file to be inputted?",
                        DialogOptions = table_converter_view_model.SelectedInputConverter.input_converter!.Controls,
                        OkButtonClick = async () =>
                        {
                            dialog_host.CurrentSession?.Close();

                            table_converter_view_model.ActualInputTextBoxText = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadFileAsync(files[0]);

                            TableData data = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadTextAsync(table_converter_view_model.ActualInputTextBoxText);

                            if (data.headers is not null && data.rows is not null)
                            {
                                table_converter_view_model.EditColumnValues = new ObservableCollection<string>(data.headers);
                                table_converter_view_model.EditRowValues = new ObservableCollection<string[]>(data.rows);

                                RefreshEditDataGrid();
                            }
                        },
                        CancelButtonClick = () =>
                        {
                            dialog_host.CurrentSession?.Close();
                        }
                    };

                    await DialogHost.Show(options, dialog_host);
                }
                else
                {
                    table_converter_view_model.ActualInputTextBoxText = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadFileAsync(files[0]);

                    TableData data = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadTextAsync(table_converter_view_model.ActualInputTextBoxText);

                    if (data.headers is not null && data.rows is not null)
                    {
                        table_converter_view_model.EditColumnValues = new ObservableCollection<string>(data.headers);
                        table_converter_view_model.EditRowValues = new ObservableCollection<string[]>(data.rows);

                        RefreshEditDataGrid();
                    }
                }
            }
        }
    }

    private void ConverterListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AboutSectionOutputConverterInfomation.IsVisible = InputConverterListBox.SelectedItem == OutputConverterListBox.SelectedItem ? false : true;
    }

    private void RefreshEditDataGrid()
    {
        if (DataContext is MainViewModel table_converter_view_model)
        {
            while (EditDataDataGrid.Columns.Count > 0)
            {
                EditDataDataGrid.Columns.RemoveAt(EditDataDataGrid.Columns.Count - 1);
            }

            for (int i = 0; i < table_converter_view_model.EditColumnValues.Count; ++i)
            {
                EditDataDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = new TextBox
                    {
                        [!TextBox.TextProperty] = new Binding($"EditColumnValues[{i}]"),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                        Background = new SolidColorBrush(Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException()
                    },
                    Binding = new Binding($"[{i}]"),
                    CanUserSort = false,
                    IsReadOnly = false,
                    CanUserReorder = false,
                    CanUserResize = true,
                    DisplayIndex = i
                });
            }
        }
    }

    private async void ConvertButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel table_converter_view_model && !string.IsNullOrEmpty(table_converter_view_model.InputTextBoxText))
        {
            if (table_converter_view_model.SelectedOutputConverter.output_converter!.Controls is not null && Parent is DialogHost dialog_host)
            {
                DialogHostOptionsView options = new()
                {
                    Title = $"How would you like your {table_converter_view_model.SelectedOutputConverter.name} file to be outputted?",
                    DialogOptions = table_converter_view_model.SelectedOutputConverter.output_converter!.Controls,
                    OkButtonClick = async () =>
                    {
                        dialog_host.CurrentSession?.Close();

                        table_converter_view_model.ActualOutputTextBoxText = await table_converter_view_model.SelectedOutputConverter.output_converter!.ConvertAsync(
                            table_converter_view_model.EditColumnValues.ToArray(),
                            table_converter_view_model.EditRowValues.ToArray().Select(row => row.ToArray()).ToArray(),
                            ConvertProgressBar
                        );
                    },
                    CancelButtonClick = () =>
                    {
                        dialog_host.CurrentSession?.Close();
                    }
                };

                await DialogHost.Show(options, dialog_host);
            }
            else
            {
                table_converter_view_model.ActualOutputTextBoxText = await table_converter_view_model.SelectedOutputConverter.output_converter!.ConvertAsync(
                    table_converter_view_model.EditColumnValues.ToArray(),
                    table_converter_view_model.EditRowValues.ToArray().Select(row => row.ToArray()).ToArray(),
                    ConvertProgressBar
                );
            }
        }
    }

    private void ClearButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel table_converter_view_model)
        {
            table_converter_view_model.ActualInputTextBoxText = string.Empty;
            table_converter_view_model.EditColumnValues = [];
            table_converter_view_model.EditRowValues = [];
            table_converter_view_model.ActualOutputTextBoxText = string.Empty;
            ConvertProgressBar.Value = 0;

            RefreshEditDataGrid();
        }
    }

    private async void CopyButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel table_converter_view_model && !string.IsNullOrEmpty(table_converter_view_model.ActualOutputTextBoxText))
        {
            await TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(table_converter_view_model.ActualOutputTextBoxText);
        }
    }

    private async void SaveButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel table_converter_view_model && !string.IsNullOrEmpty(table_converter_view_model.ActualOutputTextBoxText))
        {
            var file = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Save {table_converter_view_model.SelectedOutputConverter.name} File",
                FileTypeChoices = [
                    new(table_converter_view_model.SelectedOutputConverter.name)
                    {
                        Patterns = table_converter_view_model.SelectedOutputConverter.extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = table_converter_view_model.SelectedOutputConverter.mime_types
                    },
                    FilePickerFileTypes.All
                ],
                DefaultExtension = table_converter_view_model.SelectedOutputConverter.extensions[0],
                ShowOverwritePrompt = true,
                SuggestedFileName = $"TableConverter-{table_converter_view_model.SelectedInputConverter.name}-{table_converter_view_model.SelectedOutputConverter.name}-{DateTime.Now.ToFileTime()}"
            });

            if (file is not null)
            {
                await table_converter_view_model.SelectedOutputConverter.output_converter!.SaveFileAsync(file, Encoding.UTF8.GetBytes(table_converter_view_model.ActualOutputTextBoxText));
            }
        }
    }

    private async void MainContentScrollViewerStackPanelLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ScrollViewer scroll_viewer)
        {
            await Dispatcher.UIThread.InvokeAsync(scroll_viewer.ScrollToHome);

            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            {
                scroll_viewer.Margin = new Thickness(10, 0, 10, 0);
            }
        }
    }

    private void HeaderBorderLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Control ctrl)
        {
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            {
                ctrl.Margin = new Thickness(10, ctrl.Margin.Top, 10, ctrl.Margin.Bottom);
            }
        }
    }

    private void InputSearchBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (DataContext is MainViewModel table_converter_view_model && 
            sender is AutoCompleteBox auto_complete_box &&
            auto_complete_box.SelectedItem is not null)
        {
            table_converter_view_model.SelectedInputConverter = 
                table_converter_view_model.InputConverters.Where((converter_type) => converter_type.name == auto_complete_box.SelectedItem.ToString()).First();
        }
    }

    private void OutputSearchBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (DataContext is MainViewModel table_converter_view_model &&
            sender is AutoCompleteBox auto_complete_box &&
            auto_complete_box.SelectedItem is not null)
        {
            table_converter_view_model.SelectedOutputConverter =
                table_converter_view_model.OutputConverters.Where((converter_type) => converter_type.name == auto_complete_box.SelectedItem.ToString()).First();
        }
    }

    private void GenerateDataButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel table_converter_view_model)
        {
            table_converter_view_model.CurrentView = new DataGenerationView()
            {
                DataContext = table_converter_view_model
            };
        }
    }
}
