using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.Services.Providers;
using TableConverter.Utilities;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Virtualisation;
using TableConverter.Utilities.Virtualisation.Interfaces;
using TableConverter.ViewModels.Base;
using Path = System.IO.Path;

namespace TableConverter.ViewModels.Documents;

public partial class TableDataViewModel : BaseDocumentViewModel
{
    #region Properties

    [ObservableProperty] private ObservableCollection<string> _Headers;
    [ObservableProperty] private AsyncVirtualisationCollection<string[]> _Rows;

    public override bool CanClose => !IsDirty;
    public override bool CanUndo { get; }
    public override bool CanRedo { get; }
    
    public IItemsProvider<string[]> RowsProvider { get; }

    #endregion

    #region  Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IUndoRedo undoRedo,
        [FromKeyedServices("TableData")] IConnectionFactory connectionFactory)
        : base(commandManager, eventManager, dialogManager, toastManager, undoRedo)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TableConverter", "TestData.csv");

        RowsProvider = new TableDataItemsSourceProvider(connectionFactory.Open(
            path
        ));
        
        Headers = [];
        Rows = new AsyncVirtualisationCollection<string[]>(RowsProvider, 50, 500);
    }

    #endregion

    #region Methods

    public ITableDataTransaction CreateTransaction()
    {
        return new TableDataTransaction(Headers, []);
    }

    #endregion
}