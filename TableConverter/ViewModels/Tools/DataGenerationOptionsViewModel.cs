using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class DataGenerationOptionsViewModel : BaseScopedPaneToolViewModel<DataGenerationWorkspaceEditorViewModel>
{
    #region Properties

    [ObservableProperty] [Range(0, 100000)] private int _NumberOfRows;

    [ObservableProperty] private string _DocumentName;

    [ObservableProperty] private int _Seed;
    
    [ObservableProperty] private string _Locale = string.Empty;
    
    [ObservableProperty] private ObservableCollection<string> _Locales;

    #endregion
    
    #region Constructors
    
    public DataGenerationOptionsViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        IDataGenerationTypes generationTypes) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Generation Options")
    {
        NumberOfRows = 1000;
        DocumentName = string.Empty;
        Seed = Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode() ^ Environment.TickCount.GetHashCode();
        Locales = generationTypes.AvailableLocales.ToObservableCollection();
    }
    
    #endregion
}