using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Forms;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

public partial class ToolsSettingsForm : ObservableObject, IToolSettings
{
    #region Properties
    
    [ObservableProperty] [property: Category("Display"), DisplayName("Position")]
    private Dock _Position;
    
    [ObservableProperty] [property: 
        Category("Display"), 
        DisplayName("Size"),
        Required(ErrorMessage = "Size is required."),
        Range(0, double.MaxValue, ErrorMessage = "Size must be non-negative.")]
    private double _Size;
    
    #endregion

    #region Constructors

    public ToolsSettingsForm(Dock position, double size)
    {
        Position = position;
        Size = size;
    }
    
    #endregion
}

#pragma warning restore CS0657 // Not a valid attribute location for this declaration