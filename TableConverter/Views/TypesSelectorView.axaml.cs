using Avalonia.Controls;
using Avalonia.Media;
using System;
using TableConverter.ViewModels;

namespace TableConverter.Views;

public partial class TypesSelectorView : UserControl
{
    public TypesSelectorView()
    {
        InitializeComponent();

        if (DataContext is TypesSelectorViewModel viewModel)
        {
            viewModel.Title = "Test Title";
            viewModel.Values = 
            [
                "Test 1",
                "Test 2",
                "Test 3",
                "Test 4",
                "Test 5"
            ];
        }
    }

    public TypesSelectorView(string title, StreamGeometry icon, string[] values, Action<string> onItemClicked)
    {
        InitializeComponent();

        if (DataContext is TypesSelectorViewModel viewModel)
        {
            viewModel.Title = title;
            viewModel.Icon = icon;
            viewModel.Values = values;
            viewModel.OnItemClicked = onItemClicked;
        }
        else
        {
            throw new NullReferenceException("ViewModel is null");
        }
    }

    private void InitializeComponent()
    {
        DataContext = new TypesSelectorViewModel();

        InitializeComponent(true);
    }
}