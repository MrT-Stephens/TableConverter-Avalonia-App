using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TableConverter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private Control? _CurrentView = null;

    public MainViewModel()
    {
        PageRouterService.Initialise((view) => CurrentView = view);

        PageRouterService.NavigatePage<TableConverterViewModel>();
    }
}
