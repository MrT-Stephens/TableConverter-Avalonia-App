using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using TableConverter.DataModels;
using TableConverter.Services;

namespace TableConverter.Components.Xaml;

public partial class DataGenerationTypesViewModel : BaseDialogViewModel
{
    #region Constructor

    public DataGenerationTypesViewModel(ISukiDialog dialog, DataGenerationTypesService dataGenerationTypesService)
        : base(dialog)
    {
        var categories = dataGenerationTypesService.Types.Select(
            type => new DataGenerationListTypesViewModel(
                type.Name,
                type.Description,
                type.Icon,
                type.Methods
            )
        ).OrderBy(type => type.DisplayName).ToList();

        categories.Insert(0,
            new DataGenerationListTypesViewModel("All",
                "All of the available data generation methods.",
                Application.Current?.Resources["DataGenerationAllIcon"] ??
                throw new KeyNotFoundException("Icon not found"),
                dataGenerationTypesService.Types.SelectMany(type => type.Methods).ToList()));

        Categories = new ObservableCollection<DataGenerationListTypesViewModel>(categories);

        SelectedCategory = Categories.First();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task ButtonClicked(object? name)
    {
        if (name is string buttonName)
        {
            Close();

            switch (buttonName)
            {
                case "Select":
                    if (OnOkClicked is not null) await OnOkClicked(SelectedCategory.SelectedType);
                    break;
                case "Cancel":
                    break;
                default:
                    throw new NotImplementedException($"Button {buttonName} is not implemented");
            }
        }
    }

    #endregion

    #region Properties

    [ObservableProperty] private DataGenerationListTypesViewModel _SelectedCategory;

    [ObservableProperty] private ObservableCollection<DataGenerationListTypesViewModel> _Categories;

    public AsyncAction<DataGenerationMethod>? OnOkClicked { get; set; }

    #endregion
}