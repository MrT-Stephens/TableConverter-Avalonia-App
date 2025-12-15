using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Dialogs;

public partial class DataGenerationTypesSelectionViewModel : ObservableObject
{
    #region Constructor

    public DataGenerationTypesSelectionViewModel(ISukiDialog dialog, IDataGenerationTypes dataGenerationTypes)
    {
        Dialog = dialog;
        
        var categories = dataGenerationTypes.Types.Select(
            type => new DataGenerationTypesSelectionListViewModel(
                type.Name,
                type.Description,
                type.Icon,
                type.Methods
            )
        ).OrderBy(type => type.DisplayName).ToList();

        categories.Insert(0,
            new DataGenerationTypesSelectionListViewModel("All",
                "All of the available data generation methods.",
                Application.Current?.Resources["DataGenerationAllIcon"] ??
                throw new KeyNotFoundException("Icon not found"),
                dataGenerationTypes.Types
                    .SelectMany(type => type.Methods)
                    .ToList()));

        Categories = new ObservableCollection<DataGenerationTypesSelectionListViewModel>(categories);

        SelectedCategory = Categories.First();

        SearchText = string.Empty;
    }

    #endregion

    #region Misc

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SearchText))
        {
            SelectedCategory.FilterItems(SearchText);
        }
        
        if (e.PropertyName == nameof(SelectedCategory))
        {
            SearchText = string.Empty;
        }
    }

    #endregion

    #region Properties

    [ObservableProperty] private string _SearchText;

    [ObservableProperty] private DataGenerationTypesSelectionListViewModel _SelectedCategory;

    [ObservableProperty] private ObservableCollection<DataGenerationTypesSelectionListViewModel> _Categories;
    
    public ISukiDialog Dialog { get; }

    #endregion
}