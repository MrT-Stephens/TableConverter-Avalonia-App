using Avalonia.Controls;
using Avalonia.LogicalTree;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
        new TextBlock
        {
            Text = "No Options Available"
        }
    };

    [ObservableProperty]
    private ushort _BlankPercentage = 0;

    #endregion
}
