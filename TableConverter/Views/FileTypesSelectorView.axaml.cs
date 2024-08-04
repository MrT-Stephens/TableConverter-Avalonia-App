using Avalonia.Controls;
using Avalonia.Media;
using System;
using TableConverter.ViewModels;

namespace TableConverter.Views;

public partial class FileTypesSelectorView : UserControl
{
    public FileTypesSelectorView()
    {
        InitializeComponent();
    }

    public FileTypesSelectorView(string title, StreamGeometry icon, string[] values, Action<string> onItemClicked)
    {
        InitializeComponent();

        if (DataContext is FileTypesSelectorViewModel viewModel)
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
        DataContext = new FileTypesSelectorViewModel();

        InitializeComponent(true);
    }
}