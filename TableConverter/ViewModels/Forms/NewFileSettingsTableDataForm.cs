using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Utilities.Extensions;

namespace TableConverter.ViewModels.Forms;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

public partial class NewFileSettingsTableDataForm : ObservableObject
{
    #region Properties

    [ObservableProperty]
    [property: Category("Settings"), DisplayName("File Name")]
    private string _Name = "NewFile-{0}".Format(DateTime.Now.ToFileTime());
    
    [ObservableProperty] [property: Category("Settings"), DisplayName("Columns")]
    private int _Headers = 5;

    [ObservableProperty] [property: Category("Settings"), DisplayName("Rows")]
    private int _Rows = 10;
    
    [ObservableProperty] [property: Category("Settings"), DisplayName("Fill with Numbers")]
    private bool _FillWithNumbers = true;

    #endregion

    #region Partial Methods

    partial void OnRowsChanging(int value)
    {
    }

    #endregion
}

#pragma warning restore CS0657 // Not a valid attribute location for this declaration