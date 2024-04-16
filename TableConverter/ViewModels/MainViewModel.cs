using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TableConverter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private UserControl? _CurrentView = null;

    public MainViewModel()
    {
        CurrentView = new TableConverter.Views.TableConverterView()
        {
            DataContext = new TableConverter.ViewModels.TableConverterViewModel() 
        };
    }
}
