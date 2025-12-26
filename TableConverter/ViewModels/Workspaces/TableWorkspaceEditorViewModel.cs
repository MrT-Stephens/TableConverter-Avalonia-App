using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database.Sessions;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Virtualisation;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;

namespace TableConverter.ViewModels.Workspaces;

public partial class TableWorkspaceEditorViewModel : BaseWorkspaceEditorViewModel
{
    #region Constructors
    
    public TableWorkspaceEditorViewModel(IServiceProvider serviceProvider) 
        : base(serviceProvider, "Table Editor", "TableIcon", 2)
    {
    }
    
    #endregion

    #region Methods

    public override void Initialise()
    {
        base.Initialise();
        
        MainCommands.Add(this[TableDataCommandNames.NewFile]);
        MainCommands.Add(this[TableDataCommandNames.ImportFile]);
        MainCommands.Add(this[TableDataCommandNames.Search]);
    }

    public override IPaneDocument CreateNewDocumentInstance()
    {
        return _serviceProvider.GetRequiredService<TableDataViewModel>();
    }

    protected override IPaneDocument CreateDefaultDocumentInstance()
    {
        if (CreateNewDocumentInstance() is not TableDataViewModel tableData)
        {
            throw new InvalidOperationException("Failed to create default TableDataViewModel instance.");
        }
        
        tableData.Title = "Example Table";
        
        return tableData;
    }

    #endregion
}