using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public class TableUtilitiesViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Constructors
    
    public TableUtilitiesViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Table Utilities")
    {
    }
    
    #endregion
}