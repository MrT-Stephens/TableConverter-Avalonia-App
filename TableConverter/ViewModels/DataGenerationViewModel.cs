using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TableConverter.DataModels;

namespace TableConverter.ViewModels;

public partial class DataGenerationViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    private ObservableCollection<DataGenerationField> _DataGenerationFields = new();

    [ObservableProperty]
    private int _NumberOfRows;

    #endregion

    public DataGenerationViewModel()
    {
        DataGenerationFields.Add(new DataGenerationField());
        DataGenerationFields.Add(new DataGenerationField());
        DataGenerationFields.Add(new DataGenerationField());
        DataGenerationFields.Add(new DataGenerationField());
    }

    #region Commands

    [RelayCommand]
    public void GoBackButtonClicked()
    {
        PageRouterService.NavigateBack();
    }

    #endregion
}
