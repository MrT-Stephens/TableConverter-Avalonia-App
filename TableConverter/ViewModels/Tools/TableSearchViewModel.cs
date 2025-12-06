using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableSearchViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties
    
    [ObservableProperty] private SearchSettingsFrom _SearchSettings;
    [ObservableProperty] private ObservableCollection<TableSearchResult> _SearchResults;
    [ObservableProperty] private ObservableCollection<ICommandInstance> _SearchCommands;
    
    #endregion
    
    #region Constructors
    
    public TableSearchViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Search Table Data")
    {
        SearchSettings = new SearchSettingsFrom();
        SearchResults = [];
        SearchCommands = [];
    }
    
    #endregion
    
    #region Overrides

    public override void Initialise()
    {
        base.Initialise();
        
        SearchCommands.Add(this[TableDataCommandNames.Search]);
        SearchCommands.Add(this[TableDataCommandNames.Replace]);
    }

    protected override void OnSelectedDocumentChanged(IWorkspace workspace, IPaneDocument? oldDocument, IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);
        
        if (oldDocument?.ID != newDocument?.ID)
        {
            SearchSettings = new SearchSettingsFrom();
            SearchResults.Clear();
        }
    }

    #endregion
}