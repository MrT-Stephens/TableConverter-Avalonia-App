using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Documents;

public partial class TableDataViewModel : BaseViewModel, IPaneDocument
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private bool _IsDirty;
    [ObservableProperty] private ObservableTableData _TableData;

    public bool CanClose => !IsDirty;

    #endregion

    #region  Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = string.Empty;
        IsEnabled = true;
        IsDirty = false;
        TableData = new ObservableTableData();
    }

    #endregion
}