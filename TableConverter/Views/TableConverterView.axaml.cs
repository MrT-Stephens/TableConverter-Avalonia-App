using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Avalonia.Data;
using TableConverter.ViewModels;
using System.Text;
using Avalonia.Threading;

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
            CheckIfDataGenerationOnLoaded();
            ResizeInformationSidePanel(); 
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
            MainSplitView.IsPaneOpen = (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() ? false : true);
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
        if (DataContext is TableConverterViewModel table_converter_view_model)
        {
            var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Open {table_converter_view_model.SelectedInputConverter.name} File",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>{
                    new(table_converter_view_model.SelectedInputConverter.name)
                    {
                        Patterns = table_converter_view_model.SelectedInputConverter.extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = table_converter_view_model.SelectedInputConverter.mime_types
                    },
                    FilePickerFileTypes.All
                }
            });

            if (files.Count >= 1)
            {
                if (table_converter_view_model.SelectedInputConverter.input_converter!.Controls is not null)
                {
                    DialogHostOptionsView options = new DialogHostOptionsView()
                    {
                        Title = $"How would you like your {table_converter_view_model.SelectedInputConverter.name} file to be inputted?",
                        DialogOptions = table_converter_view_model.SelectedInputConverter.input_converter!.Controls,
                        OkButtonClick = async () =>
                        {
                            MainViewDialogHost.CurrentSession?.Close();

                            table_converter_view_model.ActualInputTextBoxText = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadFileAsync(files[0]);

                            var (column_values, row_values) = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadTextAsync(table_converter_view_model.ActualInputTextBoxText);

                            if (column_values is not null && row_values is not null)
                            {
                                table_converter_view_model.EditColumnValues = new ObservableCollection<string>(column_values);
                                table_converter_view_model.EditRowValues = new ObservableCollection<string[]>(row_values);

                                RefreshEditDataGrid();
                            }
                        },
                        CancelButtonClick = () =>
                        {
                            MainViewDialogHost.CurrentSession?.Close();
                        }
                    };

                    await DialogHostAvalonia.DialogHost.Show(options, MainViewDialogHost);
                }
                else
                {
                    table_converter_view_model.ActualInputTextBoxText = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadFileAsync(files[0]);

                    var (column_values, row_values) = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadTextAsync(table_converter_view_model.ActualInputTextBoxText);

                    if (column_values is not null && row_values is not null)
                    {
                        table_converter_view_model.EditColumnValues = new ObservableCollection<string>(column_values);
                        table_converter_view_model.EditRowValues = new ObservableCollection<string[]>(row_values);

                        RefreshEditDataGrid();
                    }
                }
            }
        }
    }

    private void ConverterListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AboutSectionOutputConverterInfomation.IsVisible = (InputConverterListBox.SelectedItem == OutputConverterListBox.SelectedItem) ? false : true;
    }

    private void RefreshEditDataGrid()
    {
        if (DataContext is TableConverterViewModel table_converter_view_model)
        {
            while (EditDataDataGrid.Columns.Count > 0)
            {
                EditDataDataGrid.Columns.RemoveAt(EditDataDataGrid.Columns.Count - 1);
            }

            for (int i = 0; i < table_converter_view_model.EditColumnValues.Count; ++i)
            {
                EditDataDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = table_converter_view_model.EditColumnValues[i],
                    Binding = new Binding($"[{i}]"),
                    CanUserSort = false,
                    IsReadOnly = false,
                    CanUserReorder = false,
                    CanUserResize = true,
                    DisplayIndex = i
                }
                );
            }
        }
    }

    private async void ConvertButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableConverterViewModel table_converter_view_model && !string.IsNullOrEmpty(table_converter_view_model.InputTextBoxText))
        {
            if (table_converter_view_model.SelectedOutputConverter.output_converter!.Controls is not null)
            {
                DialogHostOptionsView options = new DialogHostOptionsView()
                {
                    Title = $"How would you like your {table_converter_view_model.SelectedOutputConverter.name} file to be outputted?",
                    DialogOptions = table_converter_view_model.SelectedOutputConverter.output_converter!.Controls,
                    OkButtonClick = async () =>
                    {
                        MainViewDialogHost.CurrentSession?.Close();

                        table_converter_view_model.ActualOutputTextBoxText = await table_converter_view_model.SelectedOutputConverter.output_converter!.ConvertAsync(
                            table_converter_view_model.EditColumnValues.ToArray(),
                            table_converter_view_model.EditRowValues.ToArray().Select(row => row.ToArray()).ToArray(),
                            ConvertProgressBar
                        );
                    },
                    CancelButtonClick = () =>
                    {
                        MainViewDialogHost.CurrentSession?.Close();
                    }
                };

                await DialogHostAvalonia.DialogHost.Show(options, MainViewDialogHost);
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
        if (DataContext is TableConverterViewModel table_converter_view_model)
        {
            table_converter_view_model.ActualInputTextBoxText = string.Empty;
            table_converter_view_model.EditColumnValues = new ObservableCollection<string>();
            table_converter_view_model.EditRowValues = new ObservableCollection<string[]>();
            table_converter_view_model.ActualOutputTextBoxText = string.Empty;
            ConvertProgressBar.Value = 0;

            RefreshEditDataGrid();
        }
    }

    private async void CopyButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableConverterViewModel table_converter_view_model && !string.IsNullOrEmpty(table_converter_view_model.ActualOutputTextBoxText))
        {
            await TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(table_converter_view_model.ActualOutputTextBoxText);
        }
    }

    private async void SaveButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableConverterViewModel table_converter_view_model && !string.IsNullOrEmpty(table_converter_view_model.ActualOutputTextBoxText))
        {
            var file = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Save {table_converter_view_model.SelectedOutputConverter.name} File",
                FileTypeChoices = new List<FilePickerFileType>{
                    new(table_converter_view_model.SelectedOutputConverter.name)
                    {
                        Patterns = table_converter_view_model.SelectedOutputConverter.extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = table_converter_view_model.SelectedOutputConverter.mime_types
                    },
                    FilePickerFileTypes.All
                },
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

    private async void MainContentScrollViewerLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ScrollViewer scroll_viewer)
        {
            await Dispatcher.UIThread.InvokeAsync(scroll_viewer.ScrollToHome);
        }
    }

    private async void CheckIfDataGenerationOnLoaded()
    {
        if (DataContext is TableConverterViewModel table_converter_view_model && !string.IsNullOrEmpty(ViewModelBase.GeneratedData))
        {
            table_converter_view_model.SelectedInputConverter = table_converter_view_model.InputConverters.First(converter => converter.name == "Generated Data");

            table_converter_view_model.ActualInputTextBoxText = ViewModelBase.GeneratedData;

            ViewModelBase.GeneratedData = null;

            var (column_values, row_values) = await table_converter_view_model.SelectedInputConverter.input_converter!.ReadTextAsync(table_converter_view_model.ActualInputTextBoxText);

            if (column_values is not null && row_values is not null)
            {
                table_converter_view_model.EditColumnValues = new ObservableCollection<string>(column_values);
                table_converter_view_model.EditRowValues = new ObservableCollection<string[]>(row_values);

                RefreshEditDataGrid();
            }
        }
    }
}
