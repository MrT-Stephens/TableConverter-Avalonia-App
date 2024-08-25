using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.ViewModels;

public partial class DataGenerationFieldViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty]
    private string _Name = string.Empty;

    [ObservableProperty]
    private string _Type = "Choose a Type";

    [ObservableProperty]
    private ObservableCollection<Control> _OptionsControls = new()
    {
        new TextBlock()
        {
            Text = "No Options Available"
        }
    };

    [ObservableProperty]
    private ushort _BlankPercentage = 0;

    public IDataGenerationTypeHandler? DataGenerationTypeHandler { get; set; } = null;

    #endregion
}
