using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class DataGenerationOptionsViewModel : BaseScopedPaneToolViewModel<DataGenerationWorkspaceEditorViewModel>
{
    #region Properties

    [ObservableProperty] private DataGenerationOptionsForm _Options;

    #endregion
    
    #region Constructors
    
    public DataGenerationOptionsViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        IDataGenerationTypes generationTypes) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Generation Options", false)
    {
        Options = new DataGenerationOptionsForm();
        Options.Locales = generationTypes.AvailableLocales.ToObservableCollection();
        Options.Locale = Options.Locales.First(x => x.Equals("en_GB", StringComparison.OrdinalIgnoreCase));
    }
    
    #endregion
}