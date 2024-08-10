using Avalonia.Controls;
using System;
using TableConverter.ViewModels;

namespace TableConverter.Views;

public partial class DataGenerationTypesView : UserControl
{
    public DataGenerationTypesView()
    {
        InitializeComponent();
    }

    public DataGenerationTypesView(Action<string> onOkClicked)
    {
        InitializeComponent();

        if (DataContext is DataGenerationTypesViewModel viewModel)
        {
            viewModel.OnOkClicked = onOkClicked;
        }
        else
        {
            throw new NullReferenceException("ViewModel is null");
        }
    }

    private void InitializeComponent()
    {
        DataContext = new DataGenerationTypesViewModel();

        InitializeComponent(true);
    }
}