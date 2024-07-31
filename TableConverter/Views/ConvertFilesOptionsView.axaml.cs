using Avalonia.Controls;
using Avalonia.Media;
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

    public ConvertFilesOptionsView(string title, StreamGeometry icon, Collection<Control> options, Action onContinueClicked)
    {
        InitializeComponent();

        if (DataContext is ConvertFilesOptionsViewModel viewModel)
        {
            viewModel.Title = title;
            viewModel.Icon = icon;
            viewModel.Options = new(options);
            viewModel.OnContinueClicked = onContinueClicked;
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