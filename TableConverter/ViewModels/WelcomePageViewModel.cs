using Avalonia;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class WelcomePageViewModel : BasePageViewModel
{
    #region Services

    public ConverterTypesService ConverterTypes { get; }

    #endregion

    #region Constructors

    public WelcomePageViewModel(ConverterTypesService converterTypes, ISukiDialogManager dialogManager, ISukiToastManager toastManager) 
        : base(dialogManager, toastManager, "Welcome", Application.Current?.Resources["HandWaveIcon"])
    {
        ConverterTypes = converterTypes;
    }

    #endregion
}
