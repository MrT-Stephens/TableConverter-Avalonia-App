using Avalonia;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using TableConverter.DataGeneration.DataModels;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class WelcomePageViewModel : BasePageViewModel
{
    #region Services

    public ConverterTypesService ConverterTypes { get; }

    #endregion

    #region Properties
    
    

    #endregion

    #region Constructors

    public WelcomePageViewModel(ConverterTypesService converterTypes, DataGenerationTypesService dataGenerationTypes, ISukiDialogManager dialogManager, ISukiToastManager toastManager) 
        : base(dialogManager, toastManager, "Welcome", Application.Current?.Resources["HandWaveIcon"], 0)
    {
        ConverterTypes = converterTypes;


    }

    #endregion
}
