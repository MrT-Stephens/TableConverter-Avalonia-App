using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TableConverter.DataModels;

namespace TableConverter.Components.Xaml;

public partial class DataGenerationListTypesViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty] private string _Description;
    [ObservableProperty] private string _DisplayName;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private DataGenerationMethod _SelectedType;
    [ObservableProperty] private ObservableCollection<DataGenerationMethod> _FilteredTypes;

    private readonly IReadOnlyList<DataGenerationMethod> _Types;

    #endregion

    #region Constructor

    public DataGenerationListTypesViewModel(string displayName, string description, object icon,
        IReadOnlyList<DataGenerationMethod> types)
    {
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        _Types = types;
        FilteredTypes = new ObservableCollection<DataGenerationMethod>(_Types);
        
        SelectedType = FilteredTypes.First();
    }

    #endregion

    #region Methods

    public void FilterItems(string search)
    {
        FilteredTypes.Clear();

        FilteredTypes.AddRange(_Types.Where(method =>
            method.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)));
    }

    #endregion
}