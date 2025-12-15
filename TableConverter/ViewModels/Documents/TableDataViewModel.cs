using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Documents;

public partial class TableDataViewModel : BaseDocumentViewModel
{
    #region Properties

    [ObservableProperty] private ObservableCollection<string> _Headers;
    [ObservableProperty] private ObservableCollection<string[]> _Rows;

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
        Headers = [];
        Rows = [];
    }

    #endregion

    #region Methods

    public ITableDataTransaction CreateTransaction()
    {
        return new TableDataTransaction(Headers, Rows);
    }

    #endregion
}