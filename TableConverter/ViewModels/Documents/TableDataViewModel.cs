using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Documents;

public partial class TableDataViewModel : BaseDocumentViewModel
{
    #region Properties
    
    [ObservableProperty] private ITableData _TableData;

    public override bool CanClose => !IsDirty;
    public override bool CanUndo { get; }
    public override bool CanRedo { get; }

    #endregion

    #region  Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IUndoRedo undoRedo)
        : base(commandManager, eventManager, dialogManager, toastManager, undoRedo)
    {
        TableData = new ObservableTableData();
    }

    #endregion
}