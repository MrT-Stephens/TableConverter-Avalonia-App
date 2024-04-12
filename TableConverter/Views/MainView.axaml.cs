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

namespace TableConverter.Views;

public partial class MainView : UserControl
{
    private double? ResizeControlOriginalHeight = null;
    private double? ResizeControlOriginalY = null;

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        InitializeComponent(true);

        MainSplitView.Loaded += (sender, e) => ResizeInformationSidePanel();
        SizeChanged += (sender, e) => ResizeInformationSidePanel();
    }

    private void HeaderMenuLightDarkModeButtonClicked(object? sender, RoutedEventArgs e)
    {
        App.ThemeManager?.Switch(App.Current?.ActualThemeVariant.ToString() == "Dark" ? 0 : 1);
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
        if (DataContext is MainViewModel main_view_model)
        {
            var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Open {main_view_model.SelectedInputConverter.name} File",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>{
                    new(main_view_model.SelectedInputConverter.name)
                    {
                        Patterns = main_view_model.SelectedInputConverter.extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = main_view_model.SelectedInputConverter.mime_types
                    },
                    FilePickerFileTypes.All
                }
            });

            if (files.Count >= 1)
            {
                if (main_view_model.SelectedInputConverter.input_converter!.Controls is not null)
                {
                    DialogHostOptionsView options = new DialogHostOptionsView()
                    {
                        Title = $"How would you like your {main_view_model.SelectedInputConverter.name} file to be inputted?",
                        DialogOptions = main_view_model.SelectedInputConverter.input_converter!.Controls,
                        OkButtonClick = async () =>
                        {
                            MainViewDialogHost.CurrentSession?.Close();

                            main_view_model.ActualInputTextBoxText = await main_view_model.SelectedInputConverter.input_converter!.ReadFileAsync(files[0]);

                            var (column_values, row_values) = await main_view_model.SelectedInputConverter.input_converter!.ReadTextAsync(main_view_model.ActualInputTextBoxText);

                            if (column_values is not null && row_values is not null)
                            {
                                main_view_model.EditColumnValues = new ObservableCollection<string>(column_values);
                                main_view_model.EditRowValues = new ObservableCollection<string[]>(row_values);

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
                    main_view_model.ActualInputTextBoxText = await main_view_model.SelectedInputConverter.input_converter!.ReadFileAsync(files[0]);

                    var (column_values, row_values) = await main_view_model.SelectedInputConverter.input_converter!.ReadTextAsync(main_view_model.ActualInputTextBoxText);

                    if (column_values is not null && row_values is not null)
                    {
                        main_view_model.EditColumnValues = new ObservableCollection<string>(column_values);
                        main_view_model.EditRowValues = new ObservableCollection<string[]>(row_values);

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
        if (DataContext is MainViewModel main_view_model)
        {
            while (EditDataDataGrid.Columns.Count > 0)
            {
                EditDataDataGrid.Columns.RemoveAt(EditDataDataGrid.Columns.Count - 1);
            }

            for (int i = 0; i < main_view_model.EditColumnValues.Count; ++i)
            {
                EditDataDataGrid.Columns.Add(new DataGridTextColumn 
                    { 
                        Header = main_view_model.EditColumnValues[i], 
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
        if (DataContext is MainViewModel main_view_model && !string.IsNullOrEmpty(main_view_model.InputTextBoxText))
        {
            if (main_view_model.SelectedOutputConverter.output_converter!.Controls is not null)
            {
                DialogHostOptionsView options = new DialogHostOptionsView()
                {
                    Title = $"How would you like your {main_view_model.SelectedOutputConverter.name} file to be outputted?",
                    DialogOptions = main_view_model.SelectedOutputConverter.output_converter!.Controls,
                    OkButtonClick = async () =>
                    {
                        MainViewDialogHost.CurrentSession?.Close();

                        main_view_model.ActualOutputTextBoxText = await main_view_model.SelectedOutputConverter.output_converter!.ConvertAsync(
                            main_view_model.EditColumnValues.ToArray(),
                            main_view_model.EditRowValues.ToArray().Select(row => row.ToArray()).ToArray(),
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
                main_view_model.ActualOutputTextBoxText = await main_view_model.SelectedOutputConverter.output_converter!.ConvertAsync(
                    main_view_model.EditColumnValues.ToArray(),
                    main_view_model.EditRowValues.ToArray().Select(row => row.ToArray()).ToArray(),
                    ConvertProgressBar
                );
            }
        }
    }

    private void ClearButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel main_view_model)
        {
            main_view_model.ActualInputTextBoxText = string.Empty;
            main_view_model.EditColumnValues = new ObservableCollection<string>();
            main_view_model.EditRowValues = new ObservableCollection<string[]>();
            main_view_model.ActualOutputTextBoxText = string.Empty;
            ConvertProgressBar.Value = 0;

            RefreshEditDataGrid();
        }
    }

    private async void CopyButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel main_view_model && !string.IsNullOrEmpty(main_view_model.ActualOutputTextBoxText))
        {
            await TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(main_view_model.ActualOutputTextBoxText);
        }
    }

    private async void SaveButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel main_view_model && !string.IsNullOrEmpty(main_view_model.ActualOutputTextBoxText))
        {
            var file = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Save {main_view_model.SelectedInputConverter.name} File",
                FileTypeChoices = new List<FilePickerFileType>{
                    new(main_view_model.SelectedInputConverter.name)
                    {
                        Patterns = main_view_model.SelectedInputConverter.extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = main_view_model.SelectedInputConverter.mime_types
                    },
                    FilePickerFileTypes.All
                },
                DefaultExtension = main_view_model.SelectedInputConverter.extensions[0],
                ShowOverwritePrompt = true,
                SuggestedFileName = $"TableConverter-{main_view_model.SelectedInputConverter.name}-{main_view_model.SelectedOutputConverter.name}-{DateTime.Now.ToFileTime()}"
            });

            if (file is not null)
            {
                await main_view_model.SelectedOutputConverter.output_converter!.SaveFileAsync(file, Encoding.UTF8.GetBytes(main_view_model.ActualOutputTextBoxText));
            }
        }
    }
}
