using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Utilities.Extensions;

namespace TableConverter.ViewModels.Forms;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

public enum NewFileFillMode
{
    Empty,
    MultiplyRowColumn,
    SequentialNumbers,
    RandomNumbers,
    ColumnNumbers,
    RowNumbers
}

public partial class NewFileSettingsTableDataForm : ObservableObject
{
    #region Properties

    [ObservableProperty] [property: 
        Category("Settings"), 
        DisplayName("File Name"),
        Required(ErrorMessage = "Document Name is required."),
        RegularExpression(@"^[^\\\/:\*\?""<>\|\.\s]+$", ErrorMessage = "File name contains invalid characters."),
        Length(2, 255, ErrorMessage = "File name must be between 2 and 255 characters long.")]
    private string _Name = "NewFile-{0}".Format(DateTime.Now.ToFileTime());
    
    [ObservableProperty] [property: 
        Category("Settings"), 
        DisplayName("Columns"),
        Required(ErrorMessage = "Columns is required."),
        Range(1, int.MaxValue, ErrorMessage = "Columns must be at least 1.")]
    private int _Headers = 5;

    [ObservableProperty] [property: 
        Category("Settings"), 
        DisplayName("Rows"),
        Required(ErrorMessage = "Rows is required."),
        Range(1, int.MaxValue, ErrorMessage = "Rows must be at least 1.")]
    private int _Rows = 10;
    
    [ObservableProperty] [property: Category("Settings"), DisplayName("Fill Mode")]
    private NewFileFillMode _FillMode = NewFileFillMode.Empty;

    #endregion
}

#pragma warning restore CS0657 // Not a valid attribute location for this declaration