using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TableConverter.DataModels;

namespace TableConverter.ViewModels;

public partial class DataGenerationTypesSelectorViewModel : ObservableObject
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
        var categories = new List<KeyValuePair<string, string>>();

        foreach (string category in value.Select(x => x.Category).Distinct())
        {
            categories.Add(new(category, $"({value.Count(x => x.Category == category)})"));
        }

        categories.Sort((x, y) => x.Key.CompareTo(y.Key));

        categories.Insert(0, new("All", $"({value.Count})"));

        TypesCategories = new(categories);

        TypesSearchItems = new(value.Select(x => x.Name));

        SelectedCategory = TypesCategories.First();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            CurrentDataGenerationTypes = new(DataGenerationTypes.Where(x => x.Name.ToLower().Contains(value.ToLower())));
        }
        else
        {
            if (SelectedCategory.Key == "All")
            {
                CurrentDataGenerationTypes = new(DataGenerationTypes);
            }
            else
            {
                CurrentDataGenerationTypes = new(DataGenerationTypes.Where(x => x.Category == SelectedCategory.Key));
            }
        }
    }
}
