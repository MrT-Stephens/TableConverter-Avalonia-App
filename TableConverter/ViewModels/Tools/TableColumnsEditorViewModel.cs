using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableColumnsEditorViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties

    [ObservableProperty] private ObservableCollection<ColumnEntity> _Columns;

    #endregion
    
    #region Constructor
    
    public TableColumnsEditorViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Columns Editor")
    {
    }
    
    #endregion
}