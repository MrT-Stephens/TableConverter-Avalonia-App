using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Linq;
using TableConverter.DataModels;
using TableConverter.ViewModels;

namespace TableConverter.Views;

public partial class DataGenerationTypesSelectorView : UserControl
{
    public static readonly StyledProperty<Action<DataGenerationType?>> TypesSelectorViewCloseProperty =
            AvaloniaProperty.Register<DialogHostOptionsView, Action<DataGenerationType?>>(nameof(TypesSelectorViewClose));

    public Action<DataGenerationType?> TypesSelectorViewClose
    {
        get => GetValue(TypesSelectorViewCloseProperty);
        set => SetValue(TypesSelectorViewCloseProperty, value);
    }

    public DataGenerationTypesSelectorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        InitializeComponent(true);

        Loaded += (sender, e) => 
        {
            HandleResize();
            SplitViewPaneStateChanged(MainSplitViewButton);
        };
    }

    private void TypesCategoriesListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is DataGenerationTypesSelectorViewModel data_generation_types_view_model)
        {
            if (data_generation_types_view_model.SelectedCategory.Key == "All")
            {
                data_generation_types_view_model.CurrentDataGenerationTypes = new(data_generation_types_view_model.DataGenerationTypes);
            }
            else
            {
                data_generation_types_view_model.CurrentDataGenerationTypes = new(data_generation_types_view_model.DataGenerationTypes.Where(
                    x => x.Category == data_generation_types_view_model.SelectedCategory.Key)
                );
            }
        }
    }

    private void TypesListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is DataGenerationTypesSelectorViewModel data_generation_types_view_model)
        {
            if (data_generation_types_view_model.SelectedDataGenerationType is not null)
            {
                TypesSelectorViewClose?.Invoke(data_generation_types_view_model.SelectedDataGenerationType);
            }
        }
    }

    private void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        TypesSelectorViewClose?.Invoke(null);
    }

    public void HandleResize()
    {
        var top_level_window = TopLevel.GetTopLevel(this);

        if (top_level_window is not null)
        {
            var window_width = top_level_window.ClientSize.Width / 1.5;
            var window_height = top_level_window.ClientSize.Height / 1.5;

            MainBorder.Width = (window_width < 800) ? top_level_window.ClientSize.Width - 40 : window_width;
            MainBorder.Height = window_height;

            if (window_width < 1000)
            {
                MainSplitView.IsPaneOpen = false;
                MainSplitView.OpenPaneLength = window_width;
            }
            else
            {
                MainSplitView.IsPaneOpen = true;
                MainSplitView.OpenPaneLength = 300;
            }

            SplitViewPaneStateChanged(MainSplitViewButton);
        }
    }

    private void SplitViewPaneButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;

        SplitViewPaneStateChanged(sender);
    }

    private void SplitViewPaneStateChanged(object? sender)
    {
        if (sender is Button button)
        {
            if (MainSplitView.IsPaneOpen)
            {
                button.Content = new PathIcon()
                {
                    Data = App.Current?.Resources["ArrowBack"] as Geometry ?? throw new NullReferenceException(),
                    Width = 15,
                    Height = 15
                };
            }
            else
            {
                button.Content = new PathIcon()
                {
                    Data = App.Current?.Resources["ArrowForward"] as Geometry ?? throw new NullReferenceException(),
                    Width = 15,
                    Height = 15
                };
            }
        }
    }
}