using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Dialogs;
using TableConverter.ViewModels.Models;

namespace TableConverter.ViewModels.Documents;

public partial class DataGenerationSchemaViewModel : BaseDocumentViewModel
{
    #region Properties
    
    [ObservableProperty] private ObservableCollection<DataGenerationFieldViewModel> _Fields;
    
    public override bool CanClose => false;

    private readonly IDataGenerationTypes _dataGenerationTypes;

    #endregion

    #region Constructors

    public DataGenerationSchemaViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IDataGenerationTypes dataGenerationTypes)
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Fields = [new DataGenerationFieldViewModel()];

        _dataGenerationTypes = dataGenerationTypes;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SelectDataGenerationTypeButton(DataGenerationFieldViewModel field)
    {
        var builder = _dialogManager.CreateDialog()
            .WithYesNoResult("Select", "Cancel")
            .Dismiss().ByClickingBackground();

        var viewModel = new DataGenerationTypesSelectionViewModel(builder.Dialog, _dataGenerationTypes);
        
        builder.SetViewModel(_ => viewModel, true);
        builder.SetShowCardBackground(false);

        if (await builder.TryShowAsync())
        {
            var selectedField = viewModel.SelectedCategory.SelectedType;
            field.SetDataGenerationMethod(selectedField);
        }
    }
    
    [RelayCommand]
    private void AddFieldButtonClicked(DataGenerationFieldViewModel field)
    {
        if (Fields.Last() == field)
        {
            Fields.Add(new DataGenerationFieldViewModel());
        }
        else
        {
            Fields.Insert(Fields.IndexOf(field) + 1, new DataGenerationFieldViewModel());
        }
    }
    
    [RelayCommand]
    private void RemoveFieldButtonClicked(DataGenerationFieldViewModel field)
    {
        if (Fields.Count > 1)
        {
            Fields.Remove(field);
        }
    }

    #endregion
}