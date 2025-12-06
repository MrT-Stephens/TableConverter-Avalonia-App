using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TableConverter.ViewModels.Forms;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

public partial class SearchSettingsFrom : ObservableObject
{
    #region Properties
    
    [ObservableProperty] [property: Category("Search"), DisplayName("Search Text")]
    private string _SearchText = string.Empty;
    
    [ObservableProperty] [property: Category("Search"), DisplayName("Match Case")]
    private bool _MatchCase = false;
    
    [ObservableProperty] [property: Category("Search"), DisplayName("Match Whole Cell")]
    private bool _MatchWholeWord = false;
    
    [ObservableProperty] [property: Category("Search"), DisplayName("Use Regular Expressions")]
    private bool _UseRegularExpressions = false;
    
    [ObservableProperty] [property: Category("Search"), DisplayName("Search In Headers")]
    private bool _SearchInHeaders = true;
    
    [ObservableProperty] [property: Category("Search"), DisplayName("Search In Rows")]
    private bool _SearchInRows = true;
    
    [ObservableProperty] [property: Category("Replace"), DisplayName("Replace Text")]
    private string _ReplaceText = string.Empty;

    [ObservableProperty] [property: Category("Replace"), DisplayName("Replace In Headers")]
    private bool _ReplaceInHeaders = true;
    
    [ObservableProperty] [property: Category("Replace"), DisplayName("Replace In Rows")]
    private bool _ReplaceInRows = true;

    #endregion
}

#pragma warning restore CS0657 // Not a valid attribute location for this declaration