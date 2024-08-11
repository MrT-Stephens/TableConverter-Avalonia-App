using Avalonia.Controls;
using System;
using System.Collections.Generic;
using TableConverter.ViewModels;

namespace TableConverter.Views;

public partial class DataGenerationTypesView : UserControl
{
    public DataGenerationTypesView()
    {
        InitializeComponent();
    }

    public DataGenerationTypesView(IEnumerable<KeyValuePair<string, string>> dataGenerationTypes, Action<string> onOkClicked)
    {
        InitializeComponent();

        if (DataContext is DataGenerationTypesViewModel viewModel)
        {
            viewModel.GenerationTypes = new(dataGenerationTypes);
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