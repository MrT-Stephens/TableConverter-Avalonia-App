using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public sealed partial class TableDataViewModel : BasePageViewModel
{
    #region Properties

    [ObservableProperty] private ObservableTableData _TableData;

    #endregion

    #region Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager) 
        : base("TableData" ,commandManager, eventManager)
    {
        TableData = new ObservableTableData(["Column1", "Column2", "Column3"], [
            new string[] { "Row1Col1", "Row1Col2", "Row1Col3" },
            new string[] { "Row2Col1", "Row2Col2", "Row2Col3" },
            new string[] { "Row3Col1", "Row3Col2", "Row3Col3" }
        ]);
    }
    
    #endregion
}