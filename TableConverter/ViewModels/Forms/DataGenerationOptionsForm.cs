using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Utilities.Extensions;
using TableConverter.Views.Controls.PropertyGrid.ViewModels.Attributes;

namespace TableConverter.ViewModels.Forms;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

public partial class DataGenerationOptionsForm : ObservableObject
{
    #region Properties
    
    [ObservableProperty] [property: 
        Category("Settings"), 
        DisplayName("Number of Rows"),
        Required(ErrorMessage = "Number of Rows is required."),
        Range(1, int.MaxValue, ErrorMessage = "Number of Rows must be at least 1.")]
    private int _NumberOfRows = 1000;

    [ObservableProperty] [property: 
        Category("Settings"), 
        DisplayName("Document Name"),
        Required(ErrorMessage = "Document Name is required."),
        RegularExpression(@"^[^\\\/:\*\?""<>\|\.\s]+$", ErrorMessage = "File name contains invalid characters."),
        Length(2, 255, ErrorMessage = "File name must be between 2 and 255 characters long.")]
    private string _DocumentName = "NewFile".GetUniqueString();
    
    [ObservableProperty] [property: 
        Category("Settings"), 
        DisplayName("Seed"),
        Required(ErrorMessage = "Seed is required."),
        Range(int.MinValue, int.MaxValue, ErrorMessage = "Seed must be a valid integer.")]
    private int _Seed = Guid.NewGuid().GetHashCode() 
                        ^ DateTime.UtcNow.Ticks.GetHashCode() 
                        ^ Environment.TickCount.GetHashCode();
    
    [ObservableProperty] [property: 
        Category("Settings"), 
        DisplayName("Locale"), 
        RuntimeValues("Locales")]
    private string _Locale = string.Empty;
    
    #endregion

    #region Dynamic Values
    
    [ObservableProperty] [property: Ignore]
    private ObservableCollection<string> _Locales = [];
    
    #endregion
}

#pragma warning restore CS0657 // Not a valid attribute location for this declaration