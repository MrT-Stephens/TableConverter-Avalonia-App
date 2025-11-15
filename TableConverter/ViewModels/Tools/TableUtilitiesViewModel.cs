using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableUtilitiesViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties

    [ObservableProperty] private int _HeadersCount;
    [ObservableProperty] private int _RowCount;
    [ObservableProperty] private ObservableCollection<ICommandInstance> _GeneralCommands;

    #endregion
    
    #region Constructors
    
    public TableUtilitiesViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Table Utilities")
    {
        HeadersCount = 0;
        RowCount = 0;
        GeneralCommands = [];
    }
    
    #endregion

    #region Overrides
    
    

    #endregion
}