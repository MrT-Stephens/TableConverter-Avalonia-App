using CommunityToolkit.Mvvm.Input;

namespace TableConverter.ViewModels;

public partial class DataGenerationViewModel : ViewModelBase
{
    #region Properties

    #endregion

    #region Commands

    [RelayCommand]
    public void GoBackButtonClicked()
    {
        PageRouterService.NavigateBack();
    }

    #endregion
}
