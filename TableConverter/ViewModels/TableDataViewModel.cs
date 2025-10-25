using System.Collections.ObjectModel;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public sealed partial class TableDataViewModel : BasePageViewModel
{
    #region Properties

    [ObservableProperty] private ObservableCollection<string> _Headers;
    [ObservableProperty] private ObservableCollection<object[]> _Rows;
    
    #endregion

    #region Constructors
    
    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager) 
        : base("TableData" ,commandManager, eventManager)
    {
        Headers = [];
        Rows = [];
        
        Headers = ["Column1", "Column2", "Column3"];
        Rows =
        [
            new[] { "Row1Col1", "Row1Col2", "Row1Col3" },
            new[] { "Row2Col1", "Row2Col2", "Row2Col3" },
            new[] { "Row3Col1", "Row3Col2", "Row3Col3" }
        ];
    }
    
    #endregion
}