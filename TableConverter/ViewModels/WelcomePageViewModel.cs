using Avalonia;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public class WelcomePageViewModel : BasePageViewModel
{
    #region Constructors

    public WelcomePageViewModel(ConverterTypesService converterTypes, DataGenerationTypesService dataGenerationTypes,
        ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        : base(dialogManager, toastManager, "Welcome", Application.Current?.Resources["HandWaveIcon"])
    {
        ConverterTypes = converterTypes;

        DataGenerationTypes = dataGenerationTypes;
    }

    #endregion

    #region Services

    public ConverterTypesService ConverterTypes { get; }

    public DataGenerationTypesService DataGenerationTypes { get; }

    #endregion
}