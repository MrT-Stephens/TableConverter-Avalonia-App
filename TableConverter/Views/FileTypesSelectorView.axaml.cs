using Avalonia.Controls;
using System;
using TableConverter.ViewModels;

namespace TableConverter.Views;

public partial class FileTypesSelectorView : UserControl
{
    public FileTypesSelectorView()
    {
        InitializeComponent();
    }

    public FileTypesSelectorView(string title, string[] values, Action<string> onOkClicked)
    {
        InitializeComponent();

        if (DataContext is FileTypesSelectorViewModel viewModel)
        {
            viewModel.Title = title;
            viewModel.Values = values;
            viewModel.OnOkClicked = onOkClicked;
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