using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using TableConverter.ViewModels;

namespace TableConverter.Views;

public partial class ConvertFilesOptionsView : UserControl
{
    public ConvertFilesOptionsView()
    {
        InitializeComponent();
    }

    public ConvertFilesOptionsView(string title, Collection<Control> options, Action onOkClicked)
    {
        InitializeComponent();

        if (DataContext is ConvertFilesOptionsViewModel viewModel)
        {
            viewModel.Title = title;
            viewModel.Options = new(options);
            viewModel.OnOkClicked = onOkClicked;
        }
        else
        {
            throw new NullReferenceException("ViewModel is null");
        }
    }

    private void InitializeComponent()
    {
        DataContext = new ConvertFilesOptionsViewModel();

        InitializeComponent(true);
    }
}