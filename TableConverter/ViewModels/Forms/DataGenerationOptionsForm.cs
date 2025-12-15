using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TableConverter.ViewModels.Forms;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

public partial class DataGenerationOptionsForm : ObservableObject
{
    [ObservableProperty] [property: Category("Settings"), DisplayName("Number of Rows")]
    private int _NumberOfRows = 1000;

    [ObservableProperty] [property: Category("Settings"), DisplayName("Document Name")]
    private string _DocumentName = string.Empty;
    
    [ObservableProperty] [property: Category("Settings"), DisplayName("Seed")]
    private int _Seed = Guid.NewGuid().GetHashCode() 
                        ^ DateTime.UtcNow.Ticks.GetHashCode() 
                        ^ Environment.TickCount.GetHashCode();
    
    [ObservableProperty] [property: Category("Settings"), DisplayName("Locale")]
    private string _Locale = string.Empty;
}

#pragma warning restore CS0657 // Not a valid attribute location for this declaration