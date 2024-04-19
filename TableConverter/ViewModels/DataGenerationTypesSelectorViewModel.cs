using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TableConverter.DataModels;

namespace TableConverter.ViewModels;

public partial class DataGenerationTypesSelectorViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    private ObservableCollection<KeyValuePair<string, string>> _TypesCategories = new();

    [ObservableProperty]
    private ObservableCollection<DataGenerationType> _CurrentDataGenerationTypes = new();

    [ObservableProperty]
    private ObservableCollection<string> _TypesSearchItems = new();

    [ObservableProperty]
    private ObservableCollection<DataGenerationType> _DataGenerationTypes = new();

    [ObservableProperty]
    private KeyValuePair<string, string> _SelectedCategory;

    [ObservableProperty]
    private DataGenerationType? _SelectedDataGenerationType;

    [ObservableProperty]
    private string _SearchText = string.Empty;

    #endregion

    partial void OnDataGenerationTypesChanged(ObservableCollection<DataGenerationType> value)
    {
        TypesCategories = new ObservableCollection<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("All", $"({value.Count})")
        };

        foreach (string category in value.Select(x => x.Category).Distinct())
        {
            TypesCategories.Add(new KeyValuePair<string, string>(category, $"({value.Count(x => x.Category == category)})"));
        }

        TypesSearchItems = new ObservableCollection<string>(value.Select(x => x.Name));

        SelectedCategory = TypesCategories.First();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            CurrentDataGenerationTypes = new ObservableCollection<DataGenerationType>(DataGenerationTypes.Where(x => x.Name.ToLower().Contains(value.ToLower())));
        }
        else
        {
            if (SelectedCategory.Key == "All")
            {
                CurrentDataGenerationTypes = new ObservableCollection<DataGenerationType>(DataGenerationTypes);
            }
            else
            {
                CurrentDataGenerationTypes = new ObservableCollection<DataGenerationType>(DataGenerationTypes.Where(x => x.Category == SelectedCategory.Key));
            }
        }
    }
}
