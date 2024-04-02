using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;

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
            MainSplitView.OpenPaneLength = TopLevel.GetTopLevel(this)!.ClientSize.Width;
        }
        else
        {
            MainSplitView.DisplayMode = SplitViewDisplayMode.Inline;
            MainSplitView.IsPaneOpen = (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() ? false : true);
            MainSplitView.OpenPaneLength = TopLevel.GetTopLevel(this)!.ClientSize.Width / 3;
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
        if (DataContext is ViewModels.MainViewModel main_view_model)
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
                main_view_model.InputTextBoxText = await main_view_model.SelectedInputConverter.input_converter!.ReadFileAsync(files[0]);
            }
        }
    }

    private async void PasteFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel main_view_model)
        {
            var clipboard_text = await TopLevel.GetTopLevel(this)!.Clipboard!.GetTextAsync();

            if (!string.IsNullOrWhiteSpace(clipboard_text))
            {
                main_view_model.InputTextBoxText = clipboard_text;
            }
        }
    }
}
