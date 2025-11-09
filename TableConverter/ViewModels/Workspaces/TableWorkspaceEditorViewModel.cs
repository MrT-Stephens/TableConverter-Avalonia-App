using System;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Contracts;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels.Workspaces;

public partial class TableWorkspaceEditorViewModel : BaseWorkspaceEditorViewModel
{
    #region Constructors
    
    public TableWorkspaceEditorViewModel(IServiceProvider serviceProvider) 
        : base(serviceProvider, "Table Editor", "TableIcon", 1)
    {
        var tableData = serviceProvider.GetRequiredService<Documents.TableDataViewModel>();
        
        tableData.Title =  "Table Data";
        tableData.TableData = new ObservableTableData(["Test"], [["test"]]);
        tableData.IsEnabled = false;
        
        Documents.Add(tableData);
    }
    
    #endregion
}