using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Contracts;
using TableConverter.Utilities.Extensions;

namespace TableConverter.ViewModels.Dialogs;

public partial class DataGenerationTypesSelectionListViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty] private string _Description;
    [ObservableProperty] private string _DisplayName;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private DataGenerationMethod _SelectedType;
    [ObservableProperty] private ObservableCollection<DataGenerationMethod> _FilteredTypes;

    private readonly IReadOnlyList<DataGenerationMethod> _types;

    #endregion

    #region Constructor

    public DataGenerationTypesSelectionListViewModel(string displayName, string description, 
        object icon, IReadOnlyList<DataGenerationMethod> types)
    {
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        _types = types;
        FilteredTypes = new ObservableCollection<DataGenerationMethod>(_types);
        
        SelectedType = FilteredTypes.First();
    }

    #endregion

    #region Methods

    public void FilterItems(string search)
    {
        FilteredTypes.ClearAndAddRange(_types.Where(method =>
            method.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)));
    }

    #endregion
}