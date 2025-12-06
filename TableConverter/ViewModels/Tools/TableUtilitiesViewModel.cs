using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;
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

    protected override void OnSelectedDocumentChanged(IWorkspace workspace, 
        IPaneDocument? oldDocument, IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);

        if (Workspace == workspace && newDocument is TableDataViewModel tableDataViewModel)
        {
            RowCount = tableDataViewModel.TableData.RowCount;
            HeadersCount = tableDataViewModel.TableData.ColumnCount;
        }
    }

    #endregion
}