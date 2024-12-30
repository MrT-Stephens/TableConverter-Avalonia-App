using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.DataModels;

namespace TableConverter.Components.Xaml;

public partial class DataGenerationListTypesViewModel : ObservableObject
{
    [ObservableProperty] private string _Description;
    [ObservableProperty] private string _DisplayName;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private ObservableCollection<DataGenerationMethod> _Types;
    [ObservableProperty] private DataGenerationMethod _SelectedType;

    public DataGenerationListTypesViewModel(string displayName, string description, object icon,
        IReadOnlyList<DataGenerationMethod> types)
    {
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        Types = new ObservableCollection<DataGenerationMethod>(types);
        SelectedType = Types.First();
    }
}