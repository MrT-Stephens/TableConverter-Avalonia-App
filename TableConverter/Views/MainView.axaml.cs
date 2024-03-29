using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using Avalonia.Interactivity;

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

        if (top_level_window is not null)
        {
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            {
                MainSplitView.OpenPaneLength = top_level_window.ClientSize.Width;
            }
            else
            {
                MainSplitView.OpenPaneLength = top_level_window.ClientSize.Width / 3;
            }
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
}
